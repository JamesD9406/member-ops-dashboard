using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MemberOpsAPI.Data;
using MemberOpsAPI.DTOs;

namespace MemberOpsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var staff = _context.Staff.FirstOrDefault(s => s.Username == request.Username);

        if (staff == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // TODO: In production, use BCrypt.Net to verify hashed passwords
        // For now, comparing plain text (DEVELOPMENT ONLY!)
        if (staff.PasswordHash != request.Password)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Generate JWT token
        var token = GenerateJwtToken(staff.Username, staff.Role);
        var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");

        var response = new LoginResponse
        {
            Token = token,
            Username = staff.Username,
            DisplayName = staff.DisplayName,
            Role = staff.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes)
        };

        return Ok(response);
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "MemberOpsAPI";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "MemberOpsClient";
        var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
