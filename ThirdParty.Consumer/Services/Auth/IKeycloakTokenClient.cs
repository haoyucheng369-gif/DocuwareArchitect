using ThirdParty.Consumer.Models.Auth;

namespace ThirdParty.Consumer.Services.Auth;

public interface IKeycloakTokenClient
{
    Task<TokenResponse> RequestClientCredentialsTokenAsync(ClientCredentialsTokenRequest request);
}
