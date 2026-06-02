using Microsoft.AspNetCore.Mvc;
using Platform.DotNetApi;
using Platform.DotNetApi.Models;

namespace Platform.WebClient.Controllers;

public class HomeController : Controller
{
    private readonly IDocuwareClient _docuwareClient;

    public HomeController(IDocuwareClient docuwareClient)
    {
        _docuwareClient = docuwareClient;
    }

    public async Task<IActionResult> Index()
    {
        var documents = await _docuwareClient.GetDocumentsAsync();
        return View(documents);
    }

    public async Task<IActionResult> CreateSample()
    {
        await _docuwareClient.CreateDocumentAsync(new Document
        {
            Title = "Created by WebClient",
            Content = "This document was added through the MVC platform layer."
        });

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }
}
