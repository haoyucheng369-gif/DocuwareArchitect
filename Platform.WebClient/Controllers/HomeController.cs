using Microsoft.AspNetCore.Mvc;
using Platform.WebClient.Clients;
using Platform.WebClient.Models;

namespace Platform.WebClient.Controllers;

public class HomeController : Controller
{
    private readonly IPlatformApiClient _platformApiClient;

    public HomeController(IPlatformApiClient platformApiClient)
    {
        _platformApiClient = platformApiClient;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _platformApiClient.GetDocumentsAsync();
        return View(documents);
    }

    public async Task<IActionResult> CreateSample()
    {
        await _platformApiClient.CreateDocumentAsync(new DocumentViewModel
        {
            Title = "Created by WebClient",
            Content = "This document was added through the platform web client."
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
