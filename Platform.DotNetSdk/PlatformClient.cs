using System.Net.Http.Json;
using Platform.DotNetSdk.Models;

namespace Platform.DotNetSdk;

public class PlatformClient : IPlatformClient
{
    private readonly HttpClient _httpClient;

    public PlatformClient(HttpClient httpClient)
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

    public async Task<IReadOnlyList<Document>> GetIntegrationExportDocumentsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<Document>>("api/documents/integration-export");
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
