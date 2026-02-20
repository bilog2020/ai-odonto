using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Serilog;
using AiOdonto.Api.Data;
using AiOdonto.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// Voyage AI Embedding Service
builder.Services.AddHttpClient<IEmbeddingService, VoyageEmbeddingService>(client =>
{
    client.BaseAddress = new Uri("https://api.voyageai.com/");
    client.DefaultRequestHeaders.Add("Authorization",
        $"Bearer {builder.Configuration["VoyageAi:ApiKey"]}");
});

builder.Services.AddSingleton<TextChunker>();
builder.Services.AddScoped<DocumentIngestionService>();

// Semantic Kernel with Claude
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: "claude-sonnet-4-20250514",
    apiKey: builder.Configuration["Anthropic:ApiKey"] ?? string.Empty,
    endpoint: new Uri("https://api.anthropic.com/v1/")
);
var kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);
builder.Services.AddScoped<ILlmService, ClaudeLlmService>();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AuditService>();

// Controllers
builder.Services.AddControllers();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Student", "Faculty", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
