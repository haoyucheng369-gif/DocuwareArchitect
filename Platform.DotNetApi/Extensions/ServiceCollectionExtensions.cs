using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.DotNetApi.Auth;

namespace Platform.DotNetApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuwareClient(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("DocuwareClient");

        services.AddOptions<DocuwareClientOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "DocuwareClient:BaseUrl is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.TokenEndpoint), "DocuwareClient:TokenEndpoint is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "DocuwareClient:ClientId is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "DocuwareClient:ClientSecret is required");

        services.AddSingleton<IAccessTokenProvider, KeycloakClientCredentialsTokenProvider>();

        services.AddHttpClient<IDocuwareClient, DocuwareClient>((provider, client) =>
        {
            var clientOptions = provider.GetRequiredService<IOptions<DocuwareClientOptions>>().Value;

            client.BaseAddress = new Uri(clientOptions.BaseUrl);
        });

        return services;
    }
}
