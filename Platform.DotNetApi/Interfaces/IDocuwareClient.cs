using Platform.DotNetApi.Models;

namespace Platform.DotNetApi;

public interface IDocuwareClient
{
    Task<IReadOnlyList<Document>> GetDocumentsAsync();
    Task<IReadOnlyList<Document>> GetConfidentialDocumentsAsync();
    Task<Document> CreateDocumentAsync(Document document);
}
