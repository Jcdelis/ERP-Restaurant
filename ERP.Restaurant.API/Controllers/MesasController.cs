using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Infrastructure.Data;
namespace ERP.Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,CAJERO,MESERO")]
public class MesasController : ControllerBase
{
    private readonly AppDbContext _db;
    public MesasController(AppDbContext db) => _db = db;
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _db.Mesas.ToListAsync());
}