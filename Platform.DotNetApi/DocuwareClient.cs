using System.Net.Http.Headers;
using System.Net.Http.Json;
using Platform.DotNetApi.Models;
using Platform.Identity;

namespace Platform.DotNetApi;

public class DocuwareClient : IDocuwareClient
{
    private readonly HttpClient _httpClient;
    private readonly IUserIdentityService _identityService;

    public DocuwareClient(HttpClient httpClient, IUserIdentityService identityService)
    {
        _httpClient = httpClient;
        _identityService = identityService;
        ConfigureAuthorization();
    }

    private void ConfigureAuthorization()
    {
        var current = _identityService.GetCurrentUser();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", current.Token);
    }

    public static DocuwareClient Create(string baseUrl, string username, string password)
    {
        var identityService = new SimpleIdentityService();
        identityService.Login(username, password);

        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identityService.GetCurrentUser().Token);

        return new DocuwareClient(client, identityService);
    }

    public async Task<IReadOnlyList<Document>> GetDocumentsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<Document>>("api/documents");
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
