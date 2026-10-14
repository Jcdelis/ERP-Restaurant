using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductosController(AppDbContext db) => _db = db;
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? tipo)
    {
        var query = _db.Productos.AsQueryable();
        if (!string.IsNullOrEmpty(tipo)) query = query.Where(p => p.Tipo == tipo);
        return Ok(await query.ToListAsync());
    }
}