using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Platform.WebClient.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        // 触发 OIDC challenge：ASP.NET Core 会把浏览器重定向到 Keycloak 登录页。
        return Challenge(
            new AuthenticationProperties { RedirectUri = GetSafeReturnUrl(returnUrl) },
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        // 同时清理 WebClient 本地 cookie，并触发 OIDC sign-out。
        return SignOut(
            new AuthenticationProperties { RedirectUri = Url.Action("Index", "Home") },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        // 防止开放重定向，只允许跳回本应用内部路径。
        return Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home")!;
    }
}
