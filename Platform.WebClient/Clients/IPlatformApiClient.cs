using Platform.WebClient.Models;

namespace Platform.WebClient.Clients;

public interface IPlatformApiClient
{
    Task<IReadOnlyList<DocumentViewModel>> GetDocumentsAsync();
    Task<DocumentViewModel> CreateDocumentAsync(DocumentViewModel document);
}
