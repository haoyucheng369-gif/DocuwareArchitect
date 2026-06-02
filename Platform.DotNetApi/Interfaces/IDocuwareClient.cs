using Platform.DotNetApi.Models;

namespace Platform.DotNetApi;

public interface IDocuwareClient
{
    Task<IReadOnlyList<Document>> GetDocumentsAsync();
    Task<Document> CreateDocumentAsync(Document document);
}
