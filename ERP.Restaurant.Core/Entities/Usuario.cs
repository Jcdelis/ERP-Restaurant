using System.ComponentModel.DataAnnotations;

namespace ERP.Restaurant.Core.Entities;

public class Usuario
{
    [Key]
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Rol { get; set; } = "CAJERO"; // ADMIN, MESERO, CAJERO, COCINA
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcceso { get; set; }
}