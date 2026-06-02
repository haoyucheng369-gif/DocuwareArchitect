using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.Identity;

namespace Platform.DotNetApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocuwareClient(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("DocuwareClient");

        services.AddOptions<DocuwareClientOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "DocuwareClient:BaseUrl is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "DocuwareClient:Username is required")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "DocuwareClient:Password is required");

        services.AddSingleton<IUserIdentityService, SimpleIdentityService>();

        services.AddHttpClient<IDocuwareClient, DocuwareClient>((provider, client) =>
        {
            var clientOptions = provider.GetRequiredService<IOptions<DocuwareClientOptions>>().Value;
            var identityService = provider.GetRequiredService<IUserIdentityService>();

            identityService.Login(clientOptions.Username, clientOptions.Password);
            client.BaseAddress = new Uri(clientOptions.BaseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identityService.GetCurrentUser().Token);
        });

        return services;
    }
}
