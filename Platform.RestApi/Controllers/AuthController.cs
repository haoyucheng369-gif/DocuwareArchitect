using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Platform.RestApi.Controllers;

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

    [HttpGet("token")]
    public async Task<ActionResult<TokenResponse>> GetToken()
    {
        var tokenEndpoint = GetRequiredSetting("TokenClient:TokenEndpoint");
        var clientId = GetRequiredSetting("TokenClient:ClientId");
        var clientSecret = GetRequiredSetting("TokenClient:ClientSecret");

        var httpClient = _httpClientFactory.CreateClient();

        using var response = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        }));

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return token is null ? Problem("Keycloak returned an empty token response.") : Ok(token);
    }

    private string GetRequiredSetting(string key)
    {
        return _configuration[key] ?? throw new InvalidOperationException($"{key} is required");
    }
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
