using System.ComponentModel.DataAnnotations;

namespace ERP.Restaurant.Core.Entities;

public class Moneda
{
    [Key] // <-- Esto le dice a EF Core que 'Codigo' es la Primary Key
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Simbolo { get; set; } = "";
    public decimal? TipoCambioCompra { get; set; }
    public decimal? TipoCambioVenta { get; set; }
    public DateTime? ActualizadoEn { get; set; }
}