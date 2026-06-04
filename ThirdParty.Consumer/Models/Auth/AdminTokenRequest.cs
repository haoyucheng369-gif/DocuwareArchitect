using System.ComponentModel;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace ThirdParty.Consumer.Models.Auth;

public sealed class AdminTokenRequest : PasswordTokenRequest
{
    [DefaultValue("architect.admin")]
    [SwaggerSchema(Description = "Default admin token. Candidate values: architect.user for platform-user, architect.admin for platform-admin.")]
    [JsonPropertyName("username")]
    public override string Username { get; set; } = "architect.admin";
}
