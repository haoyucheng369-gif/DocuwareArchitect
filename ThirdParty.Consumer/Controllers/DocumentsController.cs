using System.Net;
using Microsoft.AspNetCore.Mvc;
using Platform.DotNetSdk;
using Platform.DotNetSdk.Models;

namespace ThirdParty.Consumer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IPlatformClient _client;

    public DocumentsController(IPlatformClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IResult> GetDocuments()
    {
        try
        {
            return Results.Ok(await _client.GetDocumentsAsync());
        }
        catch (Exception ex)
        {
            return ToIntegrationError(ex);
        }
    }

    [HttpGet("confidential")]
    public async Task<IResult> GetConfidentialDocuments()
    {
        try
        {
            return Results.Ok(await _client.GetConfidentialDocumentsAsync());
        }
        catch (Exception ex)
        {
            return ToIntegrationError(ex);
        }
    }

    [HttpPost]
    public async Task<IResult> CreateDocument()
    {
        try
        {
            var document = new Document
            {
                Title = "Third-party created document",
                Content = "This document was created by the consumer API."
            };

            return Results.Ok(await _client.CreateDocumentAsync(document));
        }
        catch (Exception ex)
        {
            return ToIntegrationError(ex);
        }
    }

    [HttpGet("from-sdk")]
    public async Task<IResult> GetDocumentsFromSdk()
    {
        try
        {
            return Results.Ok(await _client.GetDocumentsAsync());
        }
        catch (Exception ex)
        {
            return ToIntegrationError(ex);
        }
    }

    private static IResult ToIntegrationError(Exception exception)
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
}
