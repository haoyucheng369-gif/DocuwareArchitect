namespace Platform.WebClient.Models;

public class DocumentsPageViewModel
{
    public string UserName { get; set; } = string.Empty;
    public IReadOnlyList<string> Roles { get; set; } = [];
    public string AuthenticationType { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public IReadOnlyList<string> Audiences { get; set; } = [];
    public IReadOnlyList<string> Scopes { get; set; } = [];
    public IReadOnlyList<DocumentViewModel> Documents { get; set; } = [];
    public IReadOnlyList<DocumentViewModel> ConfidentialDocuments { get; set; } = [];
    public string? ConfidentialAccessMessage { get; set; }
}
