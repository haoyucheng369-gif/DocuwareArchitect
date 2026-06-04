namespace Platform.DotNetSdk;

public sealed record PlatformClientOptions
{
    public string BaseUrl { get; init; } = "http://restapi";
}
