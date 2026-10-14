using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
using ERP.Restaurant.Core.Entities;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,CAJERO")]
public class ComprasController : ControllerBase
{
    private readonly AppDbContext _db;
    public ComprasController(AppDbContext db) => _db = db;

    [HttpGet("proveedores")]
    public async Task<IActionResult> GetProveedores() => Ok(await _db.Proveedores.ToListAsync());

    [HttpPost("proveedores")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CrearProveedor([FromBody] Proveedor p)
    {
        _db.Proveedores.Add(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }

    [HttpPost("ordenes")]
    public async Task<IActionResult> CrearOrden([FromBody] CrearOrdenRequest req)
    {
        if (req.ProveedorId <= 0 || req.DiasCredito < 0 || req.Detalles is null || req.Detalles.Count == 0 ||
            req.Detalles.Any(d => d.ProductoId <= 0 || d.Cantidad <= 0 || d.CostoUnitario < 0))
            return BadRequest("La orden y sus detalles no son válidos");

        if (!await _db.Proveedores.AnyAsync(p => p.Id == req.ProveedorId))
            return BadRequest("El proveedor no existe");

        var orden = new OrdenCompra
        {
            Numero = $"OC-{DateTime.Now:yyyyMMddHHmmss}",
            ProveedorId = req.ProveedorId,
            Fecha = DateTime.UtcNow,
            Estado = "PENDIENTE"
        };

        decimal subtotal = 0;
        foreach (var item in req.Detalles)
        {
            var detalle = new OrdenCompraDetalle
            {
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                CostoUnitario = item.CostoUnitario
            };
            orden.Detalles.Add(detalle);
            subtotal += detalle.Subtotal;
        }

        orden.Subtotal = subtotal;
        orden.Impuestos = subtotal * 0.18m;
        orden.Total = orden.Subtotal + orden.Impuestos;

        _db.OrdenesCompra.Add(orden);

        var cuenta = new CuentaPagar
        {
            OrdenCompra = orden,
            MontoOriginal = orden.Total,
            FechaVencimiento = DateTime.UtcNow.AddDays(req.DiasCredito)
        };
        _db.CuentasPagar.Add(cuenta);

        await _db.SaveChangesAsync();
        return Ok(orden);
    }

    [HttpPost("ordenes/{ordenId}/recibir")]
    public async Task<IActionResult> RecibirOrden(int ordenId)
    {
        var orden = await _db.OrdenesCompra
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.Id == ordenId);

        if (orden == null) return NotFound();
        if (orden.Estado != "PENDIENTE") return BadRequest("La orden ya fue recibida o cancelada");

        foreach (var detalle in orden.Detalles)
        {
            var producto = await _db.Productos.FindAsync(detalle.ProductoId);
            if (producto != null)
            {
                producto.StockActual += detalle.Cantidad;
                producto.CostoPromedio = detalle.CostoUnitario;
            }
        }

        orden.Estado = "RECIBIDA";
        await _db.SaveChangesAsync();
        return Ok(new { Mensaje = "Orden recibida y stock actualizado" });
    }

    [HttpGet("cuentas-pagar")]
    public async Task<IActionResult> GetCuentasPagar()
    {
        var cuentas = await _db.CuentasPagar
            .Include(c => c.OrdenCompra)
                .ThenInclude(o => o.Proveedor)
            .Where(c => c.Estado == "PENDIENTE")
            .ToListAsync();

        return Ok(cuentas.Select(c => new
        {
            c.Id,
            Proveedor = c.OrdenCompra.Proveedor.RazonSocial,
            NumeroOrden = c.OrdenCompra.Numero,
            c.MontoOriginal,
            c.MontoPagado,
            c.Saldo,
            c.FechaVencimiento,
            DiasAtraso = c.FechaVencimiento < DateTime.UtcNow ? (DateTime.UtcNow - c.FechaVencimiento).Days : 0
        }));
    }

    [HttpPost("cuentas-pagar/{cuentaId}/pagar")]
    public async Task<IActionResult> PagarCuenta(int cuentaId, [FromBody] PagoCuentaRequest req)
    {
        if (req.Monto <= 0) return BadRequest("El monto debe ser positivo");

        var cuenta = await _db.CuentasPagar.FindAsync(cuentaId);
        if (cuenta == null) return NotFound();
        if (cuenta.Estado == "PAGADA") return BadRequest("La cuenta ya está pagada");
        if (req.Monto > cuenta.Saldo) return BadRequest("El pago supera el saldo pendiente");

        cuenta.MontoPagado += req.Monto;
        if (cuenta.Saldo <= 0) cuenta.Estado = "PAGADA";
        await _db.SaveChangesAsync();
        return Ok(cuenta);
    }
}

public record CrearOrdenRequest(int ProveedorId, int DiasCredito, List<DetalleOrdenRequest> Detalles);
public record DetalleOrdenRequest(int ProductoId, decimal Cantidad, decimal CostoUnitario);
public record PagoCuentaRequest(decimal Monto);