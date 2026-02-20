using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models.Dto;

namespace AiOdonto.Api.Tests.Controllers;

public class AuthTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF Core registrations for AppDbContext
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericTypeDefinition().FullName?
                         .Contains("IDbContextOptionsConfiguration") == true))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    public async Task SeedRolesAsync()
    {
        using var scope = Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Student", "Faculty", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

public class AuthControllerIntegrationTests : IClassFixture<AuthTestFactory>
{
    private readonly AuthTestFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(AuthTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.SeedRolesAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Register_WithValidData_Returns200()
    {
        var request = new RegisterRequest
        {
            Email = "test@fouba.edu.ar",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.True(result!.Success);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var request = new LoginRequest
        {
            Email = "nonexistent@fouba.edu.ar",
            Password = "WrongPassword!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
