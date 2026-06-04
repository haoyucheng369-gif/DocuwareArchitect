using System.ComponentModel;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ThirdParty.Consumer.Models.Auth;

[SwaggerSchema(Description = "Password grant request for local Swagger verification. Candidate users: architect.user / password, architect.admin / password. Third-party OAuth client: thirdparty-consumer / thirdparty-consumer-secret.")]
public class PasswordTokenRequest
{
    [DefaultValue("thirdparty-consumer")]
    [SwaggerSchema(Description = "OIDC client id registered for this third-party integration.")]
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = "thirdparty-consumer";

    [DefaultValue("thirdparty-consumer-secret")]
    [SwaggerSchema(Description = "OIDC client secret for the thirdparty-consumer client.")]
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = "thirdparty-consumer-secret";

    [DefaultValue("architect.user")]
    [SwaggerSchema(Description = "Candidate values: architect.user for platform-user, architect.admin for platform-admin.")]
    [JsonPropertyName("username")]
    public virtual string Username { get; set; } = "architect.user";

    [DefaultValue("password")]
    [SwaggerSchema(Description = "Local realm test password for both architect.user and architect.admin.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = "password";
}
