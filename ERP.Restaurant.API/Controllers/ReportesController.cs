using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportesController(AppDbContext db) => _db = db;

    [HttpGet("rentabilidad-productos")]
    public async Task<IActionResult> RentabilidadProductos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.Today.AddMonths(-1);
        var fechaHasta = hasta ?? DateTime.Today;

        var productos = await _db.Productos.Where(p => p.Tipo == "PLATO").ToListAsync();
        var resultado = new List<object>();

        foreach (var p in productos)
        {
            var vendidos = await _db.VentaDetalles
                .Include(vd => vd.Venta)
                .Where(vd => vd.ProductoId == p.Id &&
                            vd.Venta.Estado == "PAGADA" &&
                            vd.Venta.Fecha >= fechaDesde && vd.Venta.Fecha <= fechaHasta)
                .SumAsync(x => (decimal?)x.Cantidad) ?? 0;

            var ingresos = await _db.VentaDetalles
                .Include(vd => vd.Venta)
                .Where(vd => vd.ProductoId == p.Id &&
                            vd.Venta.Estado == "PAGADA" &&
                            vd.Venta.Fecha >= fechaDesde && vd.Venta.Fecha <= fechaHasta)
                .SumAsync(x => (decimal?)x.Subtotal) ?? 0;

            var costoInsumos = await _db.Recetas
                .Where(r => r.PlatoId == p.Id)
                .SumAsync(r => (decimal?)(r.CantidadRequerida * r.CostoUnitario)) ?? 0;

            var costoTotal = costoInsumos * vendidos;
            var utilidad = ingresos - costoTotal;

            resultado.Add(new
            {
                Producto = p.Nombre,
                UnidadesVendidas = vendidos,
                Ingresos = Math.Round(ingresos, 2),
                CostoTotal = Math.Round(costoTotal, 2),
                Utilidad = Math.Round(utilidad, 2),
                Margen = ingresos > 0 ? Math.Round((utilidad / ingresos) * 100, 2) : 0
            });
        }

        return Ok(resultado.OrderByDescending(x => ((dynamic)x).Utilidad));
    }

    [HttpGet("eficiencia-meseros")]
    public async Task<IActionResult> EficienciaMeseros([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.Today.AddMonths(-1);
        var fechaHasta = hasta ?? DateTime.Today;

        var eficiencia = await _db.Ventas
            .Where(v => v.Estado == "PAGADA" && v.Fecha >= fechaDesde && v.Fecha <= fechaHasta && v.MeseroId != null)
            .GroupBy(v => v.MeseroId)
            .Select(g => new
            {
                MeseroId = g.Key,
                TotalVentas = g.Count(),
                MontoTotal = g.Sum(v => v.Total),
                TicketPromedio = g.Average(v => v.Total)
            })
            .OrderByDescending(x => x.MontoTotal)
            .ToListAsync();

        return Ok(eficiencia);
    }

    [HttpGet("horas-pico")]
    public async Task<IActionResult> HorasPico([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.Today.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.Today;

        var horas = await _db.Ventas
            .Where(v => v.Estado == "PAGADA" && v.Fecha >= fechaDesde && v.Fecha <= fechaHasta)
            .GroupBy(v => v.Fecha.Hour)
            .Select(g => new
            {
                Hora = g.Key,
                CantidadVentas = g.Count(),
                MontoTotal = g.Sum(v => v.Total)
            })
            .OrderBy(x => x.Hora)
            .ToListAsync();

        return Ok(horas);
    }

    [HttpGet("dashboard-ejecutivo")]
    public async Task<IActionResult> DashboardEjecutivo()
    {
        var hoy = DateTime.Today;
        var ayer = hoy.AddDays(-1);
        var mesActual = new DateTime(hoy.Year, hoy.Month, 1);

        var ventasHoy = await _db.Ventas.Where(v => v.Fecha.Date == hoy && v.Estado == "PAGADA").SumAsync(v => (decimal?)v.Total) ?? 0;
        var ventasAyer = await _db.Ventas.Where(v => v.Fecha.Date == ayer && v.Estado == "PAGADA").SumAsync(v => (decimal?)v.Total) ?? 0;
        var ventasMes = await _db.Ventas.Where(v => v.Fecha >= mesActual && v.Estado == "PAGADA").SumAsync(v => (decimal?)v.Total) ?? 0;
        var ticketPromedio = await _db.Ventas.Where(v => v.Fecha.Date == hoy && v.Estado == "PAGADA").AverageAsync(v => (decimal?)v.Total) ?? 0;
        var totalClientes = await _db.Clientes.CountAsync();
        var cuentasPorCobrar = await _db.CuentasPagar.Where(c => c.Estado == "PENDIENTE").SumAsync(c => (decimal?)c.Saldo) ?? 0;

        var topProductos = await _db.VentaDetalles
            .Include(vd => vd.Producto)
            .Include(vd => vd.Venta)
            .Where(vd => vd.Venta.Fecha.Date == hoy && vd.Venta.Estado == "PAGADA")
            .GroupBy(vd => vd.Producto.Nombre)
            .Select(g => new { Producto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .OrderByDescending(x => x.Cantidad)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            VentasHoy = Math.Round(ventasHoy, 2),
            VentasAyer = Math.Round(ventasAyer, 2),
            Variacion = ventasAyer > 0 ? Math.Round(((ventasHoy - ventasAyer) / ventasAyer) * 100, 2) : 0,
            VentasMes = Math.Round(ventasMes, 2),
            TicketPromedio = Math.Round(ticketPromedio, 2),
            TotalClientes = totalClientes,
            CuentasPorCobrar = Math.Round(cuentasPorCobrar, 2),
            TopProductosHoy = topProductos
        });
    }
}