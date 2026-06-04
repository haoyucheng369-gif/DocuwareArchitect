using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["Authentication:Authority"]
    ?? throw new InvalidOperationException("Authentication:Authority is required");
var audience = builder.Configuration["Authentication:Audience"]
    ?? throw new InvalidOperationException("Authentication:Audience is required");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = false;
        // REST API 只信任 Keycloak 颁发给 platform-rest-api 的 JWT。
        // 关闭默认 claim mapping 后，roles claim 会按 Keycloak 原始名称参与授权判断。
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidAudience = audience,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
    });

builder.Services.AddAuthorization(options =>
{
    // 用户链路：普通文档允许普通用户和管理员访问；保密文档只允许管理员访问。
    options.AddPolicy("PlatformUser", policy =>
    {
        policy.RequireRole("platform-user", "platform-admin");
    });

    options.AddPolicy("PlatformAdmin", policy =>
    {
        policy.RequireRole("platform-admin");
    });

    // 应用链路：第三方后台集成使用 client credentials，不代表某个用户。
    options.AddPolicy("PlatformIntegration", policy =>
    {
        policy.RequireRole("platform-integration");
    });
});
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter a Keycloak access token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
