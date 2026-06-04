using Platform.DotNetApi.Auth;

namespace ThirdParty.Consumer.Auth;

public class CurrentRequestAccessTokenProvider : IAccessTokenProvider
{
    private const string BearerPrefix = "Bearer ";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentRequestAccessTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> GetAccessTokenAsync()
    {
        // 这里不主动去 IdP 换 token，而是读取调用 ThirdParty API 时传进来的 Bearer token。
        // 这个 token 会被 SDK 原样转发给 Platform.RestApi，属于真正的 token pass-through。
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) ||
            !authorization.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("A bearer token must be supplied to the third-party API request.");
        }

        return Task.FromResult(authorization[BearerPrefix.Length..].Trim());
    }
}
