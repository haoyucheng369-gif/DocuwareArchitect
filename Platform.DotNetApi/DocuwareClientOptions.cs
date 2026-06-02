namespace Platform.DotNetApi;

public sealed record DocuwareClientOptions
{
    public string BaseUrl { get; init; } = "http://restapi";
    public string Username { get; init; } = "platform-client";
    public string Password { get; init; } = "password";
}
