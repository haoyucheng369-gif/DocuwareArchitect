namespace Platform.DotNetApi.Auth;

public interface IAccessTokenProvider
{
    Task<string> GetAccessTokenAsync();
}
