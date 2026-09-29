using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CustomHome.Models;
using CustomHome.Services;

namespace CustomHome.Controllers;

public class HomeController : Controller
{
    private readonly QueueService _queueService;

    public HomeController(QueueService queueService)
    {
        _queueService = queueService;
    }

    public async Task<IActionResult> Index()
    {
        var waitingTokens = await _queueService.GetWaitingTokens();
        var servingTokens = await _queueService.GetServingTokens();

        var model = new HomeViewModel
        {
            WaitingTokens = waitingTokens,
            ServingTokens = servingTokens
        };
        
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetToken(
        CancellationToken cancellationToken)
    {
        var response = await _queueService.GetToken(cancellationToken);

        if (response.Result == QueueOperationResult.Success)
        {
            TempData["Message"] = $"Your token number is {response.TokenNumber}.";
        }
        else if (response.Result == QueueOperationResult.QueueFull)
        {
            TempData["Message"] = "The waiting queue is currently full.";
        }
        
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ServeNext(
        CancellationToken cancellationToken)
    {
        var result = await _queueService.ServeNext(cancellationToken);

        if (result == QueueOperationResult.Success)
        {
            TempData["Message"] = "Next customer is now being served.";
        }
        else if (result == QueueOperationResult.ServingCapacityFull)
        {
            TempData["Message"] = "All serving positions are currently occupied.";
        }
        else if (result == QueueOperationResult.NoWaitingCustomer)
        {
            TempData["Message"] = "There are no waiting customers.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteCurrent(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await _queueService.Complete(id, cancellationToken);

        if (result == QueueOperationResult.Success)
        {
            TempData["Message"] = "Customer has been marked as completed.";
        }
        else if (result == QueueOperationResult.TokenNotFound)
        {
            TempData["Message"] = "The selected customer is no longer being served.";
        }

        return RedirectToAction("Index");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
