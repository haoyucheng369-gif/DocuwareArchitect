using System.Net.Http.Json;
using Platform.DotNetApi.Models;

namespace Platform.DotNetApi;

public class DocuwareClient : IDocuwareClient
{
    private readonly HttpClient _httpClient;

    public DocuwareClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<Document>>("api/documents");
        return result ?? new List<Document>();
    }

    public async Task<IReadOnlyList<Document>> GetConfidentialDocumentsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<Document>>("api/documents/confidential");
        return result ?? new List<Document>();
    }

    public async Task<Document> CreateDocumentAsync(Document document)
    {
        var response = await _httpClient.PostAsJsonAsync("api/documents", document);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<Document>();
        return created ?? throw new InvalidOperationException("Document creation failed.");
    }
}
