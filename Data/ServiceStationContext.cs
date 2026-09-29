using Microsoft.EntityFrameworkCore;
using CustomHome.Models;

namespace CustomHome.Data
{
    public class ServiceStationContext : DbContext
    {
        public ServiceStationContext(DbContextOptions<ServiceStationContext> options)
            : base(options)
        {
        }

        public DbSet<ServiceToken> ServiceTokens { get; set; }

        public DbSet<QueueSettings> QueueSettings { get; set; }

         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceToken>() // Configure the ServiceToken entity to store the Status enum as a string in the database
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<ServiceToken>() // enforce unique constraint on TokenNumber
                .HasIndex(t => t.TokenNumber)
                .IsUnique();

            modelBuilder.Entity<ServiceToken>() // create a composite index on Status, CreatedAt and Id for efficient querying
                .HasIndex(t => new
                {
                    t.Status,
                    t.CreatedAt,
                    t.Id
                });

            modelBuilder.Entity<QueueSettings>().HasData(
                new QueueSettings
                {
                    Id = 1,
                    MaxWaiting = 5,
                    MaxServing = 2
                }
            );
        }
    }
}