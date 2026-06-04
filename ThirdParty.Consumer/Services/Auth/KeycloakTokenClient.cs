using ThirdParty.Consumer.Models.Auth;

namespace ThirdParty.Consumer.Services.Auth;

public class KeycloakTokenClient : IKeycloakTokenClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public KeycloakTokenClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<TokenResponse> RequestPasswordTokenAsync(PasswordTokenRequest request)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = request.ClientId,
            ["client_secret"] = request.ClientSecret,
            ["username"] = request.Username,
            ["password"] = request.Password
        };

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.PostAsync(GetRequiredSetting("TokenClient:TokenEndpoint"), new FormUrlEncodedContent(form));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new KeycloakTokenRequestException((int)response.StatusCode, error);
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return token ?? throw new InvalidOperationException("Keycloak returned an empty token response.");
    }

    private string GetRequiredSetting(string key)
    {
        return _configuration[key] ?? throw new InvalidOperationException($"{key} is required");
    }
}

public class KeycloakTokenRequestException : Exception
{
    public KeycloakTokenRequestException(int statusCode, string responseBody)
        : base(responseBody)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public int StatusCode { get; }
    public string ResponseBody { get; }
}
