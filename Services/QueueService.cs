using Microsoft.EntityFrameworkCore;
using CustomHome.Data;
using CustomHome.Models;

namespace CustomHome.Services
{
    public class QueueService
    {
        private readonly ServiceStationContext _context;

        public QueueService(ServiceStationContext context)
        {
            _context = context;
        }

        public async Task<QueueOperationResponse> GetToken()
        {
            var response = new QueueOperationResponse
            {
                Result = QueueOperationResult.QueueFull
            };

            await ExecuteWithQueueLockAsync(async settings =>
            {
                var waitingCount = await _context.ServiceTokens
                    .CountAsync(t => t.Status == ServiceTokenStatus.Waiting);

                if (waitingCount < settings.MaxWaiting)
                {
                    var token = new ServiceToken
                    {
                        TokenNumber = await GenerateUniqueTokenNumberAsync(),
                        Status = ServiceTokenStatus.Waiting,
                        CreatedAt = DateTime.Now
                    };

                    _context.ServiceTokens.Add(token); // do not need await as it is C# internal operation, not a mysql operation
                    await _context.SaveChangesAsync();

                    response.Result = QueueOperationResult.Success;
                    response.TokenNumber = token.TokenNumber;
                }
            });

            return response;
        }

        public async Task<QueueOperationResult> ServeNext()
        {
            var result = QueueOperationResult.NoWaitingCustomer;

            await ExecuteWithQueueLockAsync(async settings =>
            {
                var servingCount = await _context.ServiceTokens
                    .CountAsync(t => t.Status == ServiceTokenStatus.Serving);

                if (servingCount >= settings.MaxServing)
                {
                    result = QueueOperationResult.ServingCapacityFull;
                    return;
                }

                var token = await _context.ServiceTokens
                    .Where(t => t.Status == ServiceTokenStatus.Waiting)
                    .OrderBy(t => t.CreatedAt)
                    .ThenBy(t => t.Id) // if two tokens have the same CreatedAt timestamp, order by Id to ensure consistent behavior
                    .FirstOrDefaultAsync();

                if (token != null)
                {
                    token.Status = ServiceTokenStatus.Serving;
                    await _context.SaveChangesAsync();

                    result = QueueOperationResult.Success;
                }
            });

            return result;
        }

        public async Task<QueueOperationResult> Complete(int id)
        {
            var result = QueueOperationResult.TokenNotFound;

            await ExecuteWithQueueLockAsync(async settings =>    // settings only used for locking here
            {
                var token = await _context.ServiceTokens
                    .FirstOrDefaultAsync(t =>
                        t.Id == id &&
                        t.Status == ServiceTokenStatus.Serving);

                if (token != null)
                {
                    token.Status = ServiceTokenStatus.Completed;
                    await _context.SaveChangesAsync();

                    result = QueueOperationResult.Success;
                }
            });

            return result;
        }

        public async Task<List<ServiceToken>> GetWaitingTokens() // reading operation, no need for transaction and locking
        {
            return await _context.ServiceTokens
                .Where(t => t.Status == ServiceTokenStatus.Waiting)
                .AsNoTracking() // no need to track the entities for read-only operations
                .OrderBy(t => t.CreatedAt)
                .ThenBy(t => t.Id) // if two tokens have the same CreatedAt timestamp, order by Id to ensure consistent behavior
                .ToListAsync();
        }

        public async Task<List<ServiceToken>> GetServingTokens() // reading operation, no need for transaction and locking
        {
            return await _context.ServiceTokens
                .Where(t => t.Status == ServiceTokenStatus.Serving)
                .AsNoTracking() // no need to track the entities for read-only operations
                .OrderBy(t => t.CreatedAt)
                .ThenBy(t => t.Id) // if two tokens have the same CreatedAt timestamp, order by Id to ensure consistent behavior
                .ToListAsync();
        }

        private async Task<int> GenerateUniqueTokenNumberAsync() // generally not needed as 100000 - 999999 is a large range
        {
            int tokenNumber;

            do
            {
                tokenNumber = Random.Shared.Next(100000, 999999);
            }
            while (await _context.ServiceTokens
                .AnyAsync(t => t.TokenNumber == tokenNumber));

            return tokenNumber;
        }

        private async Task ExecuteWithQueueLockAsync(
            Func<QueueSettings, Task> action)
        {
            await using var transaction =   // await using -> when the transaction leaves its scope, clean it up asynchronously
                await _context.Database.BeginTransactionAsync();

            try
            {
                var settings = await _context.QueueSettings
                    .FromSqlRaw(
                        "SELECT * FROM QueueSettings WHERE Id = 1 FOR UPDATE")
                    .FirstAsync();

                await action(settings);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}