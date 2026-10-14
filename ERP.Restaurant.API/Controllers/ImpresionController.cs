using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
using ERP.Restaurant.Core.Entities;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,CAJERO,MESERO,COCINA")]
public class ImpresionController : ControllerBase
{
    private readonly AppDbContext _db;
    public ImpresionController(AppDbContext db) => _db = db;

    [HttpGet("impresoras")]
    public async Task<IActionResult> GetImpresoras() => Ok(await _db.Impresoras.ToListAsync());

    [HttpPost("comanda")]
    public async Task<IActionResult> EnviarComanda([FromBody] ComandaRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.ImpresoraDestino) || req.ImpresoraDestino.Length > 100)
            return BadRequest("La impresora de destino no es válida");

        var venta = await _db.Ventas
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == req.VentaId);

        if (venta == null) return NotFound();

        var comanda = new ComandaCocina
        {
            VentaId = venta.Id,
            MesaId = venta.MesaId,
            Mesero = $"Mesero #{venta.MeseroId}",
            ImpresoraDestino = req.ImpresoraDestino,
            Estado = "PENDIENTE"
        };

        foreach (var detalle in venta.Detalles)
        {
            comanda.Detalles.Add(new ComandaDetalle
            {
                ProductoId = detalle.ProductoId,
                Cantidad = detalle.Cantidad,
                Observacion = req.Observaciones
            });
        }

        _db.ComandasCocina.Add(comanda);
        await _db.SaveChangesAsync();

        // Generar texto para imprimir (formato ESC/POS)
        var textoTicket = GenerarTextoComanda(comanda, venta);

        return Ok(new
        {
            comanda.Id,
            TextoParaImprimir = textoTicket,
            Mensaje = $"Comanda enviada a {req.ImpresoraDestino}"
        });
    }

    [HttpGet("comandas-pendientes")]
    public async Task<IActionResult> ComandasPendientes()
    {
        var comandas = await _db.ComandasCocina
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(c => c.Estado == "PENDIENTE" || c.Estado == "EN_PREPARACION")
            .OrderBy(c => c.Fecha)
            .ToListAsync();
        return Ok(comandas);
    }

    [HttpPost("comandas/{comandaId}/estado")]
    public async Task<IActionResult> CambiarEstado(int comandaId, [FromBody] EstadoRequest req)
    {
        var estadosPermitidos = new[] { "PENDIENTE", "EN_PREPARACION", "LISTA", "ENTREGADA", "CANCELADA" };
        if (!estadosPermitidos.Contains(req.Estado, StringComparer.OrdinalIgnoreCase))
            return BadRequest("Estado de comanda no válido");

        var comanda = await _db.ComandasCocina.FindAsync(comandaId);
        if (comanda == null) return NotFound();
        comanda.Estado = req.Estado;
        await _db.SaveChangesAsync();
        return Ok(comanda);
    }

    private string GenerarTextoComanda(ComandaCocina comanda, Venta venta)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("================================");
        sb.AppendLine("       COMANDA DE COCINA        ");
        sb.AppendLine("================================");
        sb.AppendLine($"Mesa: {comanda.MesaId}");
        sb.AppendLine($"Mesero: {comanda.Mesero}");
        sb.AppendLine($"Hora: {comanda.Fecha:HH:mm}");
        sb.AppendLine("--------------------------------");
        foreach (var d in comanda.Detalles)
        {
            sb.AppendLine($"{d.Cantidad} x {d.Producto.Nombre}");
            if (!string.IsNullOrEmpty(d.Observacion))
                sb.AppendLine($"   * {d.Observacion}");
        }
        sb.AppendLine("================================");
        return sb.ToString();
    }
}

public record ComandaRequest(int VentaId, string ImpresoraDestino, string? Observaciones);
public record EstadoRequest(string Estado);