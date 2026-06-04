using System.ComponentModel;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ThirdParty.Consumer.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    // 这个 controller 是第三方应用侧的 Swagger 验证辅助。
    // 真实系统中，第三方应用也应使用自己的 OAuth client 向 IdP 换用户 token。
    [HttpPost("token/user")]
    [SwaggerOperation(
        Summary = "Get a user token",
        Description = "Default user: architect.user / password. Candidate users: architect.user has platform-user; architect.admin has platform-user and platform-admin.")]
    public async Task<ActionResult<TokenResponse>> GetUserToken([FromBody] UserTokenRequest request)
    {
        return await RequestPasswordTokenAsync(request);
    }

    [HttpPost("token/admin")]
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

[SwaggerSchema(Description = "Password grant request for local Swagger verification. Candidate users: architect.user / password, architect.admin / password. Third-party OAuth client: thirdparty-consumer / thirdparty-consumer-secret.")]
public class PasswordTokenRequest
{
    [DefaultValue("thirdparty-consumer")]
    [SwaggerSchema(Description = "OIDC client id registered for this third-party integration.")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = "thirdparty-consumer";

    [DefaultValue("thirdparty-consumer-secret")]
    [SwaggerSchema(Description = "OIDC client secret for the thirdparty-consumer client.")]
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = "thirdparty-consumer-secret";

    [DefaultValue("architect.user")]
    [SwaggerSchema(Description = "Candidate values: architect.user for platform-user, architect.admin for platform-admin.")]
    [JsonPropertyName("username")]
    public virtual string Username { get; set; } = "architect.user";

    [DefaultValue("password")]
    [SwaggerSchema(Description = "Local realm test password for both architect.user and architect.admin.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = "password";
}

public sealed class UserTokenRequest : PasswordTokenRequest
{
    [DefaultValue("architect.user")]
    [SwaggerSchema(Description = "Default user token. Candidate values: architect.user for platform-user, architect.admin for platform-admin.")]
    [JsonPropertyName("username")]
    public override string Username { get; set; } = "architect.user";
}

public sealed class AdminTokenRequest : PasswordTokenRequest
{
    [DefaultValue("architect.admin")]
    [SwaggerSchema(Description = "Default admin token. Candidate values: architect.user for platform-user, architect.admin for platform-admin.")]
    [JsonPropertyName("username")]
    public override string Username { get; set; } = "architect.admin";
}

public sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
