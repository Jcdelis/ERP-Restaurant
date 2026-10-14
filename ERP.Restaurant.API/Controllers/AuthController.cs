using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ERP.Restaurant.Core.Entities;
using ERP.Restaurant.Infrastructure.Services;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
            return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(result);
    }

    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.NombreCompleto) ||
            string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
            request.Password.Length < 12)
            return BadRequest(new { message = "Nombre, email y contraseña son obligatorios" });

        var usuario = new Usuario
        {
            NombreCompleto = request.NombreCompleto.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Rol = "CAJERO"
        };
        var result = await _authService.RegistrarAsync(usuario, request.Password);
        if (result == null)
            return BadRequest(new { message = "El email ya está registrado o los datos no son válidos" });
        return Ok(new { message = "Usuario registrado", usuario.Id });
    }
}

public record RegistrarRequest(string NombreCompleto, string Email, string Password, string Rol);