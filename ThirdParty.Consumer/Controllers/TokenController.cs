using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ThirdParty.Consumer.Integration;
using ThirdParty.Consumer.Models.Auth;
using ThirdParty.Consumer.Services.Auth;

namespace ThirdParty.Consumer.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IKeycloakTokenClient _tokenClient;

    public TokenController(IKeycloakTokenClient tokenClient)
    {
        _tokenClient = tokenClient;
    }

    // 第三方后台集成的 Swagger 验证辅助。
    // 真实系统中，后台服务也会使用自己的 OAuth client 向 IdP 换应用 token。
    [HttpPost("client")]
    [SwaggerOperation(
        Summary = "Get a client credentials token",
        Description = "Default client: thirdparty-consumer. This token represents the third-party application, not an end user.")]
    public async Task<IResult> GetClientToken([FromBody] ClientCredentialsTokenRequest request)
    {
        try
        {
            return Results.Ok(await _tokenClient.RequestClientCredentialsTokenAsync(request));
        }
        catch (Exception ex)
        {
            return IntegrationErrorMapper.ToResult(ex);
        }
    }
}
