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

    // 保密文档用于展示用户级授权：只有 platform-admin 可以读取。
    private static readonly List<DocumentDto> ConfidentialDocuments = new()
    {
        new DocumentDto { Id = 1001, Title = "Board Contract 2026", Content = "Restricted document for platform administrators." },
        new DocumentDto { Id = 1002, Title = "Legal Hold Case", Content = "Confidential legal metadata and retention notes." },
    };

    // 集成导出用于展示应用级授权：client credentials token 可以访问，但不代表某个用户。
    private static readonly List<DocumentDto> IntegrationExportDocuments = new()
    {
        new DocumentDto { Id = 2001, Title = "Integration Export Manifest", Content = "Document metadata prepared for a third-party integration." },
        new DocumentDto { Id = 2002, Title = "External Sync Batch", Content = "Non-confidential batch payload for machine-to-machine processing." },
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

    [HttpGet("integration-export")]
    [Authorize(Policy = "PlatformIntegration")]
    public ActionResult<IEnumerable<DocumentDto>> GetIntegrationExport()
    {
        return Ok(IntegrationExportDocuments);
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
