using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform.RestApi.Models;

namespace Platform.RestApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private static readonly List<DocumentDto> Documents = new()
    {
        new DocumentDto { Id = 1, Title = "Invoice 2026-01", Content = "Customer A, amount 1,200" },
        new DocumentDto { Id = 2, Title = "Purchase Order 2026-02", Content = "Customer B, amount 3,400" },
    };

    // 保密文档用于展示资源级授权：只有 platform-admin 可以读取。
    private static readonly List<DocumentDto> ConfidentialDocuments = new()
    {
        new DocumentDto { Id = 1001, Title = "Board Contract 2026", Content = "Restricted document for platform administrators." },
        new DocumentDto { Id = 1002, Title = "Legal Hold Case", Content = "Confidential legal metadata and retention notes." },
    };

    [HttpGet]
    [Authorize(Policy = "PlatformUser")]
    public ActionResult<IEnumerable<DocumentDto>> Get()
    {
        return Ok(Documents);
    }

    [HttpGet("confidential")]
    [Authorize(Policy = "PlatformAdmin")]
    public ActionResult<IEnumerable<DocumentDto>> GetConfidential()
    {
        return Ok(ConfidentialDocuments);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "PlatformUser")]
    public ActionResult<DocumentDto> GetById(int id)
    {
        var document = Documents.FirstOrDefault(x => x.Id == id);
        return document is null ? NotFound() : Ok(document);
    }

    [HttpPost]
    [Authorize(Policy = "PlatformUser")]
    public ActionResult<DocumentDto> Create(DocumentDto request)
    {
        request.Id = Documents.Any() ? Documents.Max(x => x.Id) + 1 : 1;
        Documents.Add(request);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
    }
}
