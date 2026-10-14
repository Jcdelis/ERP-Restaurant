using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ERP.Restaurant.Core.Entities;
using ERP.Restaurant.Infrastructure.Data;

namespace ERP.Restaurant.Infrastructure.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var usuario = await _db.Set<Usuario>()
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower() && u.Activo);

        // Ahora la verificación funcionará porque el hash es determinista
        if (usuario == null || !VerifyPassword(request.Password, usuario.PasswordHash))
            return null;

        if (!usuario.PasswordHash.StartsWith("PBKDF2$", StringComparison.Ordinal))
            usuario.PasswordHash = HashPassword(request.Password);

        usuario.UltimoAcceso = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var token = GenerateJwtToken(usuario);

        return new LoginResponse(
            token,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.Rol,
            DateTime.UtcNow.AddHours(8)
        );
    }

    public async Task<Usuario?> RegistrarAsync(Usuario usuario, string passwordPlano)
    {
        if (await _db.Set<Usuario>().AnyAsync(u => u.Email == usuario.Email))
            return null;

        usuario.PasswordHash = HashPassword(passwordPlano);
        _db.Set<Usuario>().Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Role, usuario.Rol),
            new Claim("UserId", usuario.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ✅ CORRECCIÓN: Usa SHA256 estándar que coincide con el script SQL
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600_000, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    // ✅ CORRECCIÓN: Comparación segura e insensible a mayúsculas/minúsculas
    public static bool VerifyPassword(string password, string hash)
    {
        if (hash.StartsWith("PBKDF2$", StringComparison.Ordinal))
        {
            var parts = hash.Split('$');
            if (parts.Length != 3) return false;
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 600_000, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }

        using var sha256 = SHA256.Create();
        var legacy = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(legacy), Encoding.UTF8.GetBytes(hash.ToUpperInvariant()));
    }
}