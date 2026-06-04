namespace Platform.DotNetSdk.Auth;

public interface IAccessTokenProvider
{
    Task<string> GetAccessTokenAsync();
}
