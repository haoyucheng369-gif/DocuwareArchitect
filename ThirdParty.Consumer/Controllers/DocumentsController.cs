using Microsoft.AspNetCore.Mvc;
using Platform.DotNetSdk;
using Platform.DotNetSdk.Models;
using ThirdParty.Consumer.Integration;

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
        return await ExecuteAsync(() => _client.GetDocumentsAsync());
    }

    [HttpGet("confidential")]
    public async Task<IResult> GetConfidentialDocuments()
    {
        return await ExecuteAsync(() => _client.GetConfidentialDocumentsAsync());
    }

    [HttpPost]
    public async Task<IResult> CreateDocument()
    {
        var document = new Document
        {
            Title = "Third-party created document",
            Content = "This document was created by the consumer API."
        };

        return await ExecuteAsync(() => _client.CreateDocumentAsync(document));
    }

    [HttpGet("from-sdk")]
    public async Task<IResult> GetDocumentsFromSdk()
    {
        return await ExecuteAsync(() => _client.GetDocumentsAsync());
    }

    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Results.Ok(await action());
        }
        catch (Exception ex)
        {
            return IntegrationErrorMapper.ToResult(ex);
        }
    }
}
