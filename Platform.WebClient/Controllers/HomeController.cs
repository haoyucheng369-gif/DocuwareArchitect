using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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
        try
        {
            // 普通文档要求 platform-user；未登录用户会先被 [Authorize] 带到 Keycloak 登录页。
            var tokenInfo = await GetAccessTokenInfoAsync();
            var model = new DocumentsPageViewModel
            {
                UserName = User.Identity?.Name ?? "unknown",
                Roles = GetRoles(),
                AuthenticationType = User.Identity?.AuthenticationType ?? "unknown",
                ClientId = tokenInfo.ClientId,
                Audiences = tokenInfo.Audiences,
                Scopes = tokenInfo.Scopes,
                Documents = await _platformApiClient.GetDocumentsAsync()
            };

            try
            {
                // Confidential documents 要求 platform-admin，用来展示用户级权限差异。
                model.ConfidentialDocuments = await _platformApiClient.GetConfidentialDocumentsAsync();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                model.ConfidentialAccessMessage = "The current user is signed in but does not have the platform-admin role.";
            }

            return View(model);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return await HandleUnauthorizedApiCallAsync();
        }
    }

    public async Task<IActionResult> CreateSample()
    {
        try
        {
            // 创建文档也通过 WebClient 后端调用 RestApi；UserAccessTokenHandler 会自动附加用户 token。
            await _platformApiClient.CreateDocumentAsync(new DocumentViewModel
            {
                Title = "Created by WebClient",
                Content = "This document was added through the platform web client."
            });

            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return await HandleUnauthorizedApiCallAsync();
        }
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    private async Task<IActionResult> HandleUnauthorizedApiCallAsync()
    {
        // 本地 cookie 可能还在，但其中保存的 access_token 已过期或已失效。
        // 清掉本地 cookie 后重新触发 OIDC 登录，避免用户看到 500。
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Challenge(
            new AuthenticationProperties { RedirectUri = Url.Action(nameof(Index), "Home") },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    private IReadOnlyList<string> GetRoles()
    {
        return User.Claims
            .Where(claim => claim.Type is ClaimTypes.Role or "roles")
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role)
            .ToArray();
    }

    private async Task<AccessTokenInfo> GetAccessTokenInfoAsync()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new AccessTokenInfo("not available", [], []);
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var clientId = jwt.Claims.FirstOrDefault(claim => claim.Type is "azp" or "client_id")?.Value
            ?? "not available";
        var audiences = jwt.Audiences
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(audience => audience)
            .ToArray();
        var scopes = jwt.Claims
            .Where(claim => claim.Type == "scope")
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(scope => scope)
            .ToArray();

        return new AccessTokenInfo(clientId, audiences, scopes);
    }

    private sealed record AccessTokenInfo(
        string ClientId,
        IReadOnlyList<string> Audiences,
        IReadOnlyList<string> Scopes);
}
