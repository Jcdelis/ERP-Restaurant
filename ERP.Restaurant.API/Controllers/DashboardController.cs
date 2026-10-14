using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,CAJERO")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet("kpi-general")]
    public async Task<IActionResult> GetKPIs()
    {
        var hoy = DateTime.Today;

        // Ventas del día
        var ventasDia = await _db.Ventas
            .Where(v => v.Fecha.Date == hoy && v.Estado == "PAGADA")
            .SumAsync(v => (decimal?)v.Total) ?? 0;

        // Ticket promedio
        var ticketPromedio = await _db.Ventas
            .Where(v => v.Fecha.Date == hoy && v.Estado == "PAGADA")
            .AverageAsync(v => (decimal?)v.Total) ?? 0;

        // Productos más vendidos (Top 5)
        var topProductos = await _db.VentaDetalles
            .Include(d => d.Producto)
            .Include(d => d.Venta) // <-- Ahora sí funciona porque agregamos la propiedad
            .Where(d => d.Venta.Fecha.Date == hoy && d.Venta.Estado == "PAGADA")
            .GroupBy(d => d.Producto.Nombre)
            .Select(g => new { Producto = g.Key, CantidadTotal = g.Sum(x => x.Cantidad) })
            .OrderByDescending(x => x.CantidadTotal)
            .Take(5)
            .ToListAsync();

        return Ok(new
        {
            VentasDia = ventasDia,
            TicketPromedio = Math.Round(ticketPromedio, 2),
            TopProductos = topProductos
        });
    }
}