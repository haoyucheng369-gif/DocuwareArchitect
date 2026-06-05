using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.WebClient.Clients;
using Platform.WebClient.Models;

namespace Platform.WebClient.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IPlatformApiClient _platformApiClient;

    public HomeController(IPlatformApiClient platformApiClient)
    {
        _platformApiClient = platformApiClient;
    }

    public async Task<IActionResult> Index()
    {
        // 普通文档要求 platform-user；未登录用户会先被 [Authorize] 带去 Keycloak 登录。
        var model = new DocumentsPageViewModel
        {
            Documents = await _platformApiClient.GetDocumentsAsync()
        };

        try
        {
            // confidential 文档要求 platform-admin，用来展示用户级权限差异。
            model.ConfidentialDocuments = await _platformApiClient.GetConfidentialDocumentsAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            model.ConfidentialAccessMessage = "The current user is signed in but does not have the platform-admin role.";
        }

        return View(model);
    }

    public async Task<IActionResult> CreateSample()
    {
        // 创建文档也通过 WebClient 后端调用 RestApi，Authorization header 由 UserAccessTokenHandler 自动附加。
        await _platformApiClient.CreateDocumentAsync(new DocumentViewModel
        {
            Title = "Created by WebClient",
            Content = "This document was added through the platform web client."
        });

        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }
}
