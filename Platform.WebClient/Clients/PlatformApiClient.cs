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
        // 这里保持薄封装：只描述 RestApi 资源路径和序列化，不处理认证细节。
        var documents = await _httpClient.GetFromJsonAsync<List<DocumentViewModel>>("api/documents");
        return documents ?? new List<DocumentViewModel>();
    }

    public async Task<IReadOnlyList<DocumentViewModel>> GetConfidentialDocumentsAsync()
    {
        // 如果当前用户没有 platform-admin，RestApi 会返回 403，由上层 controller 转成页面提示。
        var documents = await _httpClient.GetFromJsonAsync<List<DocumentViewModel>>("api/documents/confidential");
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
