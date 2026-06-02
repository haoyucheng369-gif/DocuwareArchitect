using Platform.DotNetApi;
using Platform.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IUserIdentityService, SimpleIdentityService>();
builder.Services.AddHttpClient<IDocuwareClient, DocuwareClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RestApiBaseUrl"] ?? "http://restapi");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
