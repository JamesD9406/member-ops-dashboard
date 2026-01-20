using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MemberOpsAPI.Controllers;
using MemberOpsAPI.Data;
using MemberOpsAPI.DTOs;
using MemberOpsAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MemberOpsAPI.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthController _controller;
    private readonly IConfiguration _configuration;

    public AuthControllerTests()
    {
        // Set up in-memory database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Set up configuration
        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:Key", "ThisIsASecretKeyForTestingPurposesOnly123456" },
            { "Jwt:Issuer", "MemberOpsAPI" },
            { "Jwt:Audience", "MemberOpsClient" },
            { "Jwt:ExpiresInMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _controller = new AuthController(_context, _configuration);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var staff = new Staff
        {
            Id = 1,
            Username = "testadmin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            DisplayName = "Test Admin",
            Email = "admin@test.com",
            Role = "Admin"
        };

        _context.Staff.Add(staff);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var request = new LoginRequest
        {
            Username = "testadmin",
            Password = "Admin123!"
        };

        var result = _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);

        Assert.NotNull(response.Token);
        Assert.Equal("testadmin", response.Username);
        Assert.Equal("Test Admin", response.DisplayName);
        Assert.Equal("Admin", response.Role);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Admin123!"
        };

        var result = _controller.Login(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public void Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "testadmin",
            Password = "WrongPassword!"
        };

        var result = _controller.Login(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    [Fact]
    public void Login_ValidCredentials_JwtContainsCorrectClaims()
    {
        var request = new LoginRequest
        {
            Username = "testadmin",
            Password = "Admin123!"
        };

        var result = _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);

        // Decode and verify JWT claims
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.Token);

        var nameClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

        Assert.NotNull(nameClaim);
        Assert.Equal("testadmin", nameClaim.Value);
        Assert.NotNull(roleClaim);
        Assert.Equal("Admin", roleClaim.Value);
    }

    [Fact]
    public void Login_ValidCredentials_JwtHasCorrectIssuerAndAudience()
    {
        var request = new LoginRequest
        {
            Username = "testadmin",
            Password = "Admin123!"
        };

        var result = _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.Token);

        Assert.Equal("MemberOpsAPI", token.Issuer);
        Assert.Contains("MemberOpsClient", token.Audiences);
    }

    [Theory]
    [InlineData("Agent")]
    [InlineData("Supervisor")]
    [InlineData("Admin")]
    public void Login_DifferentRoles_JwtContainsCorrectRole(string role)
    {
        var staff = new Staff
        {
            Username = $"role_{role.ToLower()}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            DisplayName = $"Test {role}",
            Email = $"{role.ToLower()}@test.com",
            Role = role
        };
        _context.Staff.Add(staff);
        _context.SaveChanges();

        var request = new LoginRequest
        {
            Username = $"role_{role.ToLower()}",
            Password = "Test123!"
        };

        var result = _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.Token);

        var roleClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.NotNull(roleClaim);
        Assert.Equal(role, roleClaim.Value);
    }
}
