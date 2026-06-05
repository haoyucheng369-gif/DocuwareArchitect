using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Platform.WebClient.Auth;
using Platform.WebClient.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// WebClient 是服务器端 MVC 应用：浏览器只持有本应用的 cookie，
// 授权码换 token 的过程由后端完成，用户密码不会进入 WebClient。
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        var oidcSection = builder.Configuration.GetSection("Authentication:Oidc");
        var externalAuthority = oidcSection["ExternalAuthority"]?.TrimEnd('/');

        // Authorization Code flow：confidential client 使用 client secret 换 token。
        // SaveTokens=true 会把 access_token 保存到认证 ticket，后续调用 REST API 时再取出。
        options.Authority = oidcSection["Authority"];
        options.ClientId = oidcSection["ClientId"];
        options.ClientSecret = oidcSection["ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuers = BuildValidIssuers(options.Authority, externalAuthority),
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };

        // Docker 场景下，WebClient 后端用容器内地址访问 Keycloak metadata/token endpoint，
        // 但浏览器跳转必须使用宿主机可访问的 localhost 地址。
        if (!string.IsNullOrWhiteSpace(externalAuthority))
        {
            options.Events = new OpenIdConnectEvents
            {
                OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.IssuerAddress = RewriteIssuerAddress(
                        context.ProtocolMessage.IssuerAddress,
                        options.Authority!,
                        externalAuthority);

                    return Task.CompletedTask;
                },
                OnRedirectToIdentityProviderForSignOut = context =>
                {
                    context.ProtocolMessage.IssuerAddress = RewriteIssuerAddress(
                        context.ProtocolMessage.IssuerAddress,
                        options.Authority!,
                        externalAuthority);

                    return Task.CompletedTask;
                }
            };
        }
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<UserAccessTokenHandler>();

// WebClient 调 Platform.RestApi 时不重新登录，也不自己生成 token。
// UserAccessTokenHandler 会从当前用户 cookie session 里取 access_token 并加到 HTTP header。
builder.Services.AddHttpClient<IPlatformApiClient, PlatformApiClient>(client =>
{
    var baseUrl = builder.Configuration["PlatformApi:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("PlatformApi:BaseUrl is required");
    }

    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<UserAccessTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
// 顺序很重要：先还原 cookie 中的用户身份，再执行授权判断。
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// 只改浏览器重定向地址，不改后端 metadata/token 调用地址。
static string RewriteIssuerAddress(string issuerAddress, string internalAuthority, string externalAuthority)
{
    return issuerAddress.StartsWith(internalAuthority, StringComparison.OrdinalIgnoreCase)
        ? externalAuthority + issuerAddress[internalAuthority.Length..]
        : issuerAddress;
}

static IEnumerable<string> BuildValidIssuers(string? internalAuthority, string? externalAuthority)
{
    return new[] { internalAuthority, externalAuthority }
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Select(value => value!.TrimEnd('/'))
        .Distinct(StringComparer.OrdinalIgnoreCase);
}
