using System.Net.Http.Json;
using Platform.WebClient.Models;

namespace Platform.WebClient.Clients;

public class PlatformApiClient : IPlatformApiClient
{
    private readonly HttpClient _httpClient;

    public PlatformApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<DocumentViewModel>> GetDocumentsAsync()
    {
        var documents = await _httpClient.GetFromJsonAsync<List<DocumentViewModel>>("api/documents");
        return documents ?? new List<DocumentViewModel>();
    }

    public async Task<DocumentViewModel> CreateDocumentAsync(DocumentViewModel document)
    {
        var response = await _httpClient.PostAsJsonAsync("api/documents", document);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<DocumentViewModel>();
        return created ?? throw new InvalidOperationException("Document creation failed.");
    }
}
