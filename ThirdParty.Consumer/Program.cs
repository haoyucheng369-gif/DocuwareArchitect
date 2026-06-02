using Platform.DotNetApi;
using Platform.DotNetApi.Models;
using Platform.Identity;

Console.WriteLine("Third-party consumer sample starting...");

var identityService = new SimpleIdentityService();
using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };

var client = new DocuwareClient(httpClient, identityService);

var documents = await client.GetDocumentsAsync();
Console.WriteLine("Documents returned by Platform.RestApi via Platform.DotNetApi:");

foreach (var document in documents)
{
    Console.WriteLine($"- [{document.Id}] {document.Title}: {document.Content}");
}

var newDocument = await client.CreateDocumentAsync(new Document
{
    Title = "Third-party created document",
    Content = "This document was created by an external consumer calling the DLL."
});

Console.WriteLine($"Created document ID {newDocument.Id}.");

Console.WriteLine("Done. Run the WebClient or REST API to see the sample architecture in action.");
