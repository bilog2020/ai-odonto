using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using AiOdonto.Api.Services;
using AiOdonto.Api.Models.Dto;

namespace AiOdonto.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!",
            ["Jwt:Issuer"] = "AiOdonto.Test",
            ["Jwt:Audience"] = "AiOdonto.Test",
            ["Jwt:ExpirationMinutes"] = "60"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _authService = new AuthService(_userManagerMock.Object, _configuration);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string> { "Student" });

        var request = new RegisterRequest
        {
            Email = "student@fouba.edu.ar",
            Password = "SecurePass123!",
            FullName = "María López"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsFailure()
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already exists" }));

        var request = new RegisterRequest
        {
            Email = "existing@fouba.edu.ar",
            Password = "SecurePass123!",
            FullName = "Test User"
        };

        var result = await _authService.RegisterAsync(request);

        Assert.False(result.Success);
        Assert.Contains("Email already exists", result.Errors);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var user = new IdentityUser { Email = "student@fouba.edu.ar", UserName = "student@fouba.edu.ar" };
        _userManagerMock
            .Setup(x => x.FindByEmailAsync("student@fouba.edu.ar"))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "SecurePass123!"))
            .ReturnsAsync(true);
        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Student" });

        var request = new LoginRequest
        {
            Email = "student@fouba.edu.ar",
            Password = "SecurePass123!"
        };

        var result = await _authService.LoginAsync(request);

        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.NotEmpty(result.Token);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsFailure()
    {
        var user = new IdentityUser { Email = "student@fouba.edu.ar", UserName = "student@fouba.edu.ar" };
        _userManagerMock
            .Setup(x => x.FindByEmailAsync("student@fouba.edu.ar"))
            .ReturnsAsync(user);
        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, "WrongPassword!"))
            .ReturnsAsync(false);

        var request = new LoginRequest
        {
            Email = "student@fouba.edu.ar",
            Password = "WrongPassword!"
        };

        var result = await _authService.LoginAsync(request);

        Assert.False(result.Success);
    }
}
