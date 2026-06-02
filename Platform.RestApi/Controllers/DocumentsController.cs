using Microsoft.AspNetCore.Mvc;
using Platform.RestApi.Models;

namespace Platform.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private static readonly List<DocumentDto> Documents = new()
    {
        new DocumentDto { Id = 1, Title = "Invoice 2026-01", Content = "Customer A, amount 1,200" },
        new DocumentDto { Id = 2, Title = "Purchase Order 2026-02", Content = "Customer B, amount 3,400" },
    };

    [HttpGet]
    public ActionResult<IEnumerable<DocumentDto>> Get()
    {
        return Ok(Documents);
    }

    [HttpGet("{id}")]
    public ActionResult<DocumentDto> GetById(int id)
    {
        var document = Documents.FirstOrDefault(x => x.Id == id);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    public ActionResult<DocumentDto> Create(DocumentDto request)
    {
        request.Id = Documents.Any() ? Documents.Max(x => x.Id) + 1 : 1;
        Documents.Add(request);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
    }
}
