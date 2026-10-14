namespace ERP.Restaurant.Core.Entities;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, string NombreCompleto, string Email, string Rol, DateTime Expiracion);

public record RegistrarRequest(string NombreCompleto, string Email, string Password, string Rol);