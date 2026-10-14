using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Restaurant.API.Tests;

public class TestFixture : IDisposable
{
    private const string JwtKey = "01234567890123456789012345678901";
    private const string JwtIssuer = "ERP-Restaurant";
    private const string JwtAudience = "ERP-Restaurant-API";

    public WebApplicationFactory<Program> Factory { get; }

    public TestFixture()
    {
        // Set environment variable for Jwt:Key before creating the host
        Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);
        Environment.SetEnvironmentVariable("Jwt__Issuer", JwtIssuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", JwtAudience);

        Factory = new WebApplicationFactory<Program>();
    }

    /// <summary>
    /// Generates a valid JWT token for testing authenticated endpoints.
    /// </summary>
    public string GenerateTestToken(string role = "ADMIN", string userName = "test@example.com")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, role),
            new Claim("UserId", "1")
        };

        var token = new JwtSecurityToken(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void Dispose()
    {
        try { Factory.Dispose(); } catch { }
        Environment.SetEnvironmentVariable("Jwt__Key", null);
        Environment.SetEnvironmentVariable("Jwt__Issuer", null);
        Environment.SetEnvironmentVariable("Jwt__Audience", null);
    }
}
