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

    // 第三方应用侧的 Swagger 验证辅助。
    // 真实系统中，第三方应用也应使用自己的 OAuth client 向 IdP 换用户 token。
    [HttpPost("user")]
    [SwaggerOperation(
        Summary = "Get a user token",
        Description = "Default user: architect.user / password. Candidate users: architect.user has platform-user; architect.admin has platform-user and platform-admin.")]
    public async Task<IResult> GetUserToken([FromBody] UserTokenRequest request)
    {
        return await RequestTokenAsync(request);
    }

    [HttpPost("admin")]
    [SwaggerOperation(
        Summary = "Get an admin token",
        Description = "Default user: architect.admin / password. Candidate users: architect.user has platform-user; architect.admin has platform-user and platform-admin.")]
    public async Task<IResult> GetAdminToken([FromBody] AdminTokenRequest request)
    {
        return await RequestTokenAsync(request);
    }

    private async Task<IResult> RequestTokenAsync(PasswordTokenRequest request)
    {
        try
        {
            return Results.Ok(await _tokenClient.RequestPasswordTokenAsync(request));
        }
        catch (Exception ex)
        {
            return IntegrationErrorMapper.ToResult(ex);
        }
    }
}
