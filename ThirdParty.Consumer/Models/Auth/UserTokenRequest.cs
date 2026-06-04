using System.ComponentModel;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ThirdParty.Consumer.Models.Auth;

public sealed class UserTokenRequest : PasswordTokenRequest
{
    [DefaultValue("architect.user")]
    [SwaggerSchema(Description = "Default user token. Candidate values: architect.user for platform-user, architect.admin for platform-admin.")]
    [JsonPropertyName("username")]
    public override string Username { get; set; } = "architect.user";
}
