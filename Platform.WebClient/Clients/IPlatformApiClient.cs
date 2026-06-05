using Platform.WebClient.Models;

namespace Platform.WebClient.Clients;

public interface IPlatformApiClient
{
    Task<IReadOnlyList<DocumentViewModel>> GetDocumentsAsync();
    Task<IReadOnlyList<DocumentViewModel>> GetConfidentialDocumentsAsync();
    Task<DocumentViewModel> CreateDocumentAsync(DocumentViewModel document);
}
