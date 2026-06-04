using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.DotNetApi.Auth;

namespace Platform.DotNetApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuwareClient<TAccessTokenProvider>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TAccessTokenProvider : class, IAccessTokenProvider
    {
        var section = configuration.GetSection("DocuwareClient");

        services.AddOptions<DocuwareClientOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "DocuwareClient:BaseUrl is required");

        // SDK 只定义 IAccessTokenProvider 接口，具体认证方式由宿主应用注入。
        // 宿主可以实现 Keycloak、Azure AD、静态 token、当前请求 token 等不同策略。
        services.AddSingleton<IAccessTokenProvider, TAccessTokenProvider>();
        services.AddTransient<BearerTokenHandler>();

        services.AddHttpClient<IDocuwareClient, DocuwareClient>((provider, client) =>
        {
            var clientOptions = provider.GetRequiredService<IOptions<DocuwareClientOptions>>().Value;

            client.BaseAddress = new Uri(clientOptions.BaseUrl);
        })
        // 所有 SDK 发出的 HTTP 请求都会经过 BearerTokenHandler，自动附加 Authorization header。
        .AddHttpMessageHandler<BearerTokenHandler>();

        return services;
    }
}
