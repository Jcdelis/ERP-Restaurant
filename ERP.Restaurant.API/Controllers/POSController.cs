using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
using ERP.Restaurant.Core.Entities;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,CAJERO,MESERO")]
public class POSController : ControllerBase
{
    private readonly AppDbContext _db;
    public POSController(AppDbContext db) => _db = db;

    [HttpPost("ventas")]
    public async Task<IActionResult> CrearVenta([FromBody] CrearVentaRequest req)
    {
        if (req.TipoCambio is <= 0 || req.MesaId is <= 0 || req.MeseroId is <= 0)
            return BadRequest("Mesa, mesero y tipo de cambio deben ser válidos");

        var venta = new Venta
        {
            NumeroComprobante = $"VTA-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
            MesaId = req.MesaId,
            MeseroId = req.MeseroId,
            Moneda = req.Moneda ?? "PEN",
            TipoCambio = req.TipoCambio ?? 1,
            Estado = "ABIERTA"
        };
        _db.Ventas.Add(venta);
        await _db.SaveChangesAsync();
        return Ok(new { venta.Id, venta.Estado });
    }

    [HttpPost("ventas/{ventaId}/productos")]
    public async Task<IActionResult> AgregarProducto(int ventaId, [FromBody] AgregarProductoRequest req)
    {
        if (req.Cantidad <= 0 || req.Descuento is < 0)
            return BadRequest("La cantidad debe ser positiva y el descuento no puede ser negativo");

        var venta = await _db.Ventas.FindAsync(ventaId);
        if (venta == null || venta.Estado != "ABIERTA") return NotFound("Venta no encontrada o cerrada");
        var producto = await _db.Productos.FindAsync(req.ProductoId);
        if (producto == null) return NotFound("Producto no encontrado");

        var detalle = new VentaDetalle
        {
            VentaId = ventaId,
            ProductoId = req.ProductoId,
            Cantidad = req.Cantidad,
            PrecioUnitario = producto.PrecioVenta,
            Descuento = req.Descuento ?? 0
        };
        _db.VentaDetalles.Add(detalle);
        await _db.SaveChangesAsync();
        return Ok(new { detalle.Id, detalle.Cantidad, detalle.PrecioUnitario });
    }

    [HttpPost("ventas/{ventaId}/pagar")]
    public async Task<IActionResult> PagarVenta(int ventaId, [FromBody] PagarVentaRequest req)
    {
        if (req.Pagos is null || req.Pagos.Count == 0 || req.Pagos.Any(p => p.Monto <= 0 || p.TipoCambio <= 0))
            return BadRequest("Debe existir al menos un pago válido");

        var venta = await _db.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null || venta.Estado != "ABIERTA") return BadRequest("Venta no válida");

        var totalFinal = venta.Detalles.Sum(d => d.Subtotal);
        venta.Total = totalFinal;
        venta.Subtotal = Math.Round(totalFinal / 1.18m, 2);
        venta.Impuestos = Math.Round(venta.Total - venta.Subtotal, 2);

        decimal totalPagado = 0;
        var formaPagoIds = req.Pagos.Select(p => p.FormaPagoId).Distinct().ToList();
        if (await _db.FormasPago.CountAsync(f => formaPagoIds.Contains(f.Id)) != formaPagoIds.Count)
            return BadRequest("Una o más formas de pago no existen");

        foreach (var pago in req.Pagos)
        {
            _db.VentaPagos.Add(new VentaPago
            {
                VentaId = ventaId,
                FormaPagoId = pago.FormaPagoId,
                Monto = pago.Monto,
                Moneda = pago.Moneda,
                TipoCambio = pago.TipoCambio,
                Referencia = pago.Referencia
            });
            totalPagado += pago.Monto * pago.TipoCambio;
        }

        if (totalPagado < venta.Total - 0.01m)
            return BadRequest($"Falta saldo. Total: S/ {venta.Total:F2}, Pagado: S/ {totalPagado:F2}");

        venta.Estado = "PAGADA";
        await _db.SaveChangesAsync();
        return Ok(new { venta.Id, venta.Total, venta.Estado, Vuelto = Math.Round(totalPagado - venta.Total, 2) });
    }
}

public record CrearVentaRequest(int? MesaId, int? MeseroId, string? Moneda, decimal? TipoCambio);
public record AgregarProductoRequest(int ProductoId, decimal Cantidad, decimal? Descuento);
public record PagarVentaRequest(List<PagoRequest> Pagos);
public record PagoRequest(int FormaPagoId, decimal Monto, string Moneda, decimal TipoCambio, string? Referencia);