using Platform.DotNetSdk.Models;

namespace Platform.DotNetSdk;

public interface IPlatformClient
{
    Task<IReadOnlyList<Document>> GetDocumentsAsync();
    Task<IReadOnlyList<Document>> GetConfidentialDocumentsAsync();
    Task<IReadOnlyList<Document>> GetIntegrationExportDocumentsAsync();
    Task<Document> CreateDocumentAsync(Document document);
}
