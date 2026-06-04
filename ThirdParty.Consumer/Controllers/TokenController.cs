using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ThirdParty.Consumer.Models.Auth;

namespace ThirdParty.Consumer.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TokenController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // 这个 controller 是第三方应用侧的 Swagger 验证辅助。
    // 真实系统中，第三方应用也应使用自己的 OAuth client 向 IdP 换用户 token。
    [HttpPost("user")]
    [SwaggerOperation(
        Summary = "Get a user token",
        Description = "Default user: architect.user / password. Candidate users: architect.user has platform-user; architect.admin has platform-user and platform-admin.")]
    public async Task<ActionResult<TokenResponse>> GetUserToken([FromBody] UserTokenRequest request)
    {
        return await RequestPasswordTokenAsync(request);
    }

    [HttpPost("admin")]
    [SwaggerOperation(
        Summary = "Get an admin token",
        Description = "Default user: architect.admin / password. Candidate users: architect.user has platform-user; architect.admin has platform-user and platform-admin.")]
    public async Task<ActionResult<TokenResponse>> GetAdminToken([FromBody] AdminTokenRequest request)
    {
        return await RequestPasswordTokenAsync(request);
    }

    private async Task<ActionResult<TokenResponse>> RequestPasswordTokenAsync(PasswordTokenRequest request)
    {
        return await RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = request.ClientId,
            ["client_secret"] = request.ClientSecret,
            ["username"] = request.Username,
            ["password"] = request.Password
        });
    }

    private async Task<ActionResult<TokenResponse>> RequestTokenAsync(Dictionary<string, string> form)
    {
        var httpClient = _httpClientFactory.CreateClient();

        using var response = await httpClient.PostAsync(GetRequiredSetting("TokenClient:TokenEndpoint"), new FormUrlEncodedContent(form));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Problem(error, statusCode: (int)response.StatusCode, title: "Keycloak token request failed");
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return token is null ? Problem("Keycloak returned an empty token response.") : Ok(token);
    }

    private string GetRequiredSetting(string key)
    {
        return _configuration[key] ?? throw new InvalidOperationException($"{key} is required");
    }
}
