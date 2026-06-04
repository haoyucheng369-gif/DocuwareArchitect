using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.DotNetSdk.Auth;

namespace Platform.DotNetSdk.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformClient<TAccessTokenProvider>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TAccessTokenProvider : class, IAccessTokenProvider
    {
        var section = configuration.GetSection("PlatformClient");

        services.AddOptions<PlatformClientOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "PlatformClient:BaseUrl is required");

        // SDK 只定义 token provider 抽象，具体认证方式由宿主应用注入。
        // 宿主可以实现 Keycloak、Azure AD、静态 token、当前请求 token 等不同策略。
        services.AddSingleton<IAccessTokenProvider, TAccessTokenProvider>();
        services.AddTransient<BearerTokenHandler>();

        services.AddHttpClient<IPlatformClient, PlatformClient>((provider, client) =>
        {
            var clientOptions = provider.GetRequiredService<IOptions<PlatformClientOptions>>().Value;
            client.BaseAddress = new Uri(clientOptions.BaseUrl);
        })
        // SDK 发出的 HTTP 请求统一经过 BearerTokenHandler 附加 Authorization header。
        .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
