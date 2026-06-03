using System.Net.Http.Headers;
using System.Net.Http.Json;
using Platform.DotNetApi.Auth;
using Platform.DotNetApi.Models;

namespace Platform.DotNetApi;

public class DocuwareClient : IDocuwareClient
{
    private readonly HttpClient _httpClient;
    private readonly IAccessTokenProvider _accessTokenProvider;

    public DocuwareClient(HttpClient httpClient, IAccessTokenProvider accessTokenProvider)
    {
        _httpClient = httpClient;
        _accessTokenProvider = accessTokenProvider;
    }

    private async Task ConfigureAuthorizationAsync()
    {
        var accessToken = await _accessTokenProvider.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsAsync()
    {
        await ConfigureAuthorizationAsync();
        var result = await _httpClient.GetFromJsonAsync<List<Document>>("api/documents");
        return result ?? new List<Document>();
    }

    public async Task<Document> CreateDocumentAsync(Document document)
    {
        await ConfigureAuthorizationAsync();
        var response = await _httpClient.PostAsJsonAsync("api/documents", document);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<Document>();
        return created ?? throw new InvalidOperationException("Document creation failed.");
    }
}
