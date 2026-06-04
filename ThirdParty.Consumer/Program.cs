using Microsoft.OpenApi;
using Platform.DotNetSdk.Extensions;
using ThirdParty.Consumer.Auth;
using ThirdParty.Consumer.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter an access token issued for platform-rest-api."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddScoped<IKeycloakTokenClient, KeycloakTokenClient>();
// 第三方应用选择“当前请求 token 透传”策略。
// 调用方先在 Swagger Authorize 中填 Bearer token，SDK 再把同一个 token 转发给 Platform.RestApi。
builder.Services.AddPlatformClient<CurrentRequestAccessTokenProvider>(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
