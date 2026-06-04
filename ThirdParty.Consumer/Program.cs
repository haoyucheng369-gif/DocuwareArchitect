using Microsoft.OpenApi;
using System.Net;
using Platform.DotNetApi;
using Platform.DotNetApi.Extensions;
using Platform.DotNetApi.Models;
using ThirdParty.Consumer.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter an access token issued for platform-rest-api."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
// 第三方应用选择“当前请求 token 透传”策略。
// 调用方先在 Swagger Authorize 中填 Bearer token，SDK 再把同一个 token 转发给 Platform.RestApi。
builder.Services.AddDocuwareClient<CurrentRequestAccessTokenProvider>(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/api/documents", async (IDocuwareClient client) =>
{
    try
    {
        return Results.Ok(await client.GetDocumentsAsync());
    }
    catch (Exception ex)
    {
        return ToIntegrationError(ex);
    }
});

app.MapGet("/api/documents/confidential", async (IDocuwareClient client) =>
{
    try
    {
        return Results.Ok(await client.GetConfidentialDocumentsAsync());
    }
    catch (Exception ex)
    {
        return ToIntegrationError(ex);
    }
});

app.MapPost("/api/documents", async (IDocuwareClient client) =>
{
    try
    {
        var document = new Document
        {
            Title = "Third-party created document",
            Content = "This document was created by the consumer API."
        };
        return Results.Ok(await client.CreateDocumentAsync(document));
    }
    catch (Exception ex)
    {
        return ToIntegrationError(ex);
    }
});

app.MapGet("/api/documents-from-factory", async (IDocuwareClient client) =>
{
    try
    {
        return Results.Ok(await client.GetDocumentsAsync());
    }
    catch (Exception ex)
    {
        return ToIntegrationError(ex);
    }
})
    .WithName("GetDocumentsFromFactory");

app.Run();

static IResult ToIntegrationError(Exception exception)
{
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
