using System.Net.Http.Headers;

namespace Platform.DotNetApi.Auth;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IAccessTokenProvider _accessTokenProvider;

    public BearerTokenHandler(IAccessTokenProvider accessTokenProvider)
    {
        _accessTokenProvider = accessTokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // SDK 不关心 token 从哪里来，只调用宿主应用提供的 IAccessTokenProvider。
        // 对 ThirdParty.Consumer 来说，这里拿到的是当前请求传入的同一个 Bearer token。
        var accessToken = await _accessTokenProvider.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
