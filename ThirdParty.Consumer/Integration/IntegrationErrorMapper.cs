using System.Net;
using ThirdParty.Consumer.Services.Auth;

namespace ThirdParty.Consumer.Integration;

public static class IntegrationErrorMapper
{
    public static IResult ToResult(Exception exception)
    {
        if (exception is KeycloakTokenRequestException tokenException)
        {
            return Results.Problem(
                tokenException.ResponseBody,
                statusCode: tokenException.StatusCode,
                title: "Keycloak token request failed");
        }

        if (exception is UnauthorizedAccessException)
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (exception is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized })
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (exception is HttpRequestException { StatusCode: HttpStatusCode.Forbidden })
        {
            return Results.Problem(exception.Message, statusCode: StatusCodes.Status403Forbidden);
        }

        return Results.Problem(exception.Message, statusCode: StatusCodes.Status502BadGateway);
    }
}
