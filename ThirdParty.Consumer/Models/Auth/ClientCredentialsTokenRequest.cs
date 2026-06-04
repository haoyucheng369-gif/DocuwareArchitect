using System.ComponentModel;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ThirdParty.Consumer.Models.Auth;

[SwaggerSchema(Description = "Client credentials request for local Swagger verification. This token represents the third-party application, not an end user.")]
public class ClientCredentialsTokenRequest
{
    [DefaultValue("thirdparty-consumer")]
    [SwaggerSchema(Description = "OIDC client id registered for this third-party integration.")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = "thirdparty-consumer";

    [DefaultValue("thirdparty-consumer-secret")]
    [SwaggerSchema(Description = "OIDC client secret for the thirdparty-consumer client.")]
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = "thirdparty-consumer-secret";
}
