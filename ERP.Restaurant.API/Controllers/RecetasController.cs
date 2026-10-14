using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
using ERP.Restaurant.Core.Entities;

namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class RecetasController : ControllerBase
{
    private readonly AppDbContext _db;
    public RecetasController(AppDbContext db) => _db = db;

    [HttpGet("costo-plato/{platoId}")]
    public async Task<IActionResult> CalcularCostoPlato(int platoId)
    {
        var receta = await _db.Recetas
            .Include(r => r.Insumo)
            .Include(r => r.Plato)
            .Where(r => r.PlatoId == platoId)
            .ToListAsync();

        if (!receta.Any())
            return NotFound("No hay receta definida para este plato");

        var costoTotal = receta.Sum(r => r.CantidadRequerida * r.CostoUnitario);
        var precioVenta = receta.First().Plato.PrecioVenta;
        var margen = precioVenta - costoTotal;
        var margenPct = precioVenta > 0 ? (margen / precioVenta) * 100 : 0;

        return Ok(new
        {
            PlatoId = platoId,
            Plato = receta.First().Plato.Nombre,
            PrecioVenta = precioVenta,
            CostoTotal = Math.Round(costoTotal, 2),
            MargenBruto = Math.Round(margen, 2),
            MargenPorcentaje = Math.Round(margenPct, 2),
            Insumos = receta.Select(r => new
            {
                r.Insumo.Nombre,
                r.CantidadRequerida,
                r.Unidad,
                r.CostoUnitario,
                CostoTotal = Math.Round(r.CantidadRequerida * r.CostoUnitario, 2)
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearReceta([FromBody] CrearRecetaRequest req)
    {
        if (req.CantidadRequerida <= 0 || req.CostoUnitario < 0 || string.IsNullOrWhiteSpace(req.Unidad))
            return BadRequest("La cantidad, unidad y coste de la receta no son válidos");

        var receta = new Receta
        {
            PlatoId = req.PlatoId,
            InsumoId = req.InsumoId,
            CantidadRequerida = req.CantidadRequerida,
            Unidad = req.Unidad,
            CostoUnitario = req.CostoUnitario
        };
        _db.Recetas.Add(receta);
        await _db.SaveChangesAsync();
        return Ok(receta);
    }

    [HttpGet("todos-los-costos")]
    public async Task<IActionResult> TodosLosCostos()
    {
        var platos = await _db.Productos.Where(p => p.Tipo == "PLATO").ToListAsync();
        var resultado = new List<object>();

        foreach (var plato in platos)
        {
            var insumos = await _db.Recetas
                .Where(r => r.PlatoId == plato.Id)
                .ToListAsync();

            var costoTotal = insumos.Sum(r => r.CantidadRequerida * r.CostoUnitario);
            var margen = plato.PrecioVenta - costoTotal;
            var margenPct = plato.PrecioVenta > 0 ? (margen / plato.PrecioVenta) * 100 : 0;

            resultado.Add(new
            {
                Plato = plato.Nombre,
                PrecioVenta = plato.PrecioVenta,
                CostoTotal = Math.Round(costoTotal, 2),
                Margen = Math.Round(margen, 2),
                MargenPct = Math.Round(margenPct, 2)
            });
        }
        return Ok(resultado);
    }
}

public record CrearRecetaRequest(int PlatoId, int InsumoId, decimal CantidadRequerida, string Unidad, decimal CostoUnitario);