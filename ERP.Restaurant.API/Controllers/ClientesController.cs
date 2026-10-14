using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
using ERP.Restaurant.Core.Entities;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,CAJERO")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ClientesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Clientes.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] Cliente c)
    {
        _db.Clientes.Add(c);
        await _db.SaveChangesAsync();
        return Ok(c);
    }

    [HttpPost("{clienteId}/suma-puntos")]
    public async Task<IActionResult> SumarPuntos(int clienteId, [FromBody] PuntosRequest req)
    {
        if (req.Puntos <= 0 || req.MontoCompra < 0)
            return BadRequest("Los puntos deben ser positivos y el monto no puede ser negativo");

        var cliente = await _db.Clientes.FindAsync(clienteId);
        if (cliente == null) return NotFound();

        cliente.Puntos += req.Puntos;
        cliente.TotalCompras += req.MontoCompra;
        cliente.Visitas++;

        // Actualizar nivel según total de compras
        cliente.Nivel = cliente.TotalCompras switch
        {
            >= 5000 => "DIAMANTE",
            >= 2000 => "ORO",
            >= 500 => "PLATA",
            _ => "BRONCE"
        };

        _db.HistorialPuntos.Add(new HistorialPuntos
        {
            ClienteId = clienteId,
            Puntos = req.Puntos,
            Tipo = "GANADO",
            Motivo = $"Compra de S/ {req.MontoCompra:F2}"
        });

        await _db.SaveChangesAsync();
        return Ok(new { cliente.Puntos, cliente.Nivel, cliente.TotalCompras });
    }

    [HttpPost("{clienteId}/canjear-puntos")]
    public async Task<IActionResult> CanjearPuntos(int clienteId, [FromBody] PuntosRequest req)
    {
        if (req.Puntos <= 0)
            return BadRequest("Los puntos a canjear deben ser positivos");

        var cliente = await _db.Clientes.FindAsync(clienteId);
        if (cliente == null) return NotFound();
        if (cliente.Puntos < req.Puntos) return BadRequest("Puntos insuficientes");

        cliente.Puntos -= req.Puntos;
        _db.HistorialPuntos.Add(new HistorialPuntos
        {
            ClienteId = clienteId,
            Puntos = -req.Puntos,
            Tipo = "CANJEADO",
            Motivo = req.Motivo ?? "Canje de puntos"
        });

        await _db.SaveChangesAsync();
        return Ok(new { cliente.Puntos });
    }

    [HttpGet("cumpleanos-hoy")]
    public async Task<IActionResult> CumpleanosHoy()
    {
        var hoy = DateTime.Today;
        var cumpleañeros = await _db.Clientes
            .Where(c => c.FechaCumpleanos.HasValue &&
                        c.FechaCumpleanos.Value.Month == hoy.Month &&
                        c.FechaCumpleanos.Value.Day == hoy.Day)
            .ToListAsync();
        return Ok(cumpleañeros);
    }

    [HttpGet("top-clientes")]
    public async Task<IActionResult> TopClientes([FromQuery] int top = 10)
    {
        top = Math.Clamp(top, 1, 100);
        var topClientes = await _db.Clientes
            .OrderByDescending(c => c.TotalCompras)
            .Take(top)
            .ToListAsync();
        return Ok(topClientes);
    }

    [HttpGet("promociones-activas")]
    public async Task<IActionResult> PromocionesActivas()
    {
        var hoy = DateTime.UtcNow;
        var promos = await _db.Promociones
            .Where(p => p.Activa && p.FechaInicio <= hoy && p.FechaFin >= hoy)
            .ToListAsync();
        return Ok(promos);
    }
}

public record PuntosRequest(int Puntos, decimal MontoCompra = 0, string? Motivo = null);