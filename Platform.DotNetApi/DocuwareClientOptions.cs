namespace Platform.DotNetApi;

public sealed record DocuwareClientOptions
{
    public string BaseUrl { get; init; } = "http://restapi";
    public string TokenEndpoint { get; init; } = "http://keycloak:8080/realms/docuware-architect/protocol/openid-connect/token";
    public string ClientId { get; init; } = "platform-dotnet-sdk";
    public string ClientSecret { get; init; } = "platform-dotnet-sdk-secret";
}
