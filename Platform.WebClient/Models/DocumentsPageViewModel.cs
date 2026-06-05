namespace Platform.WebClient.Models;

public class DocumentsPageViewModel
{
    public IReadOnlyList<DocumentViewModel> Documents { get; set; } = [];
    public IReadOnlyList<DocumentViewModel> ConfidentialDocuments { get; set; } = [];
    public string? ConfidentialAccessMessage { get; set; }
}
