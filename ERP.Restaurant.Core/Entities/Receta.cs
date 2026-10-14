using System.ComponentModel.DataAnnotations.Schema;
namespace ERP.Restaurant.Core.Entities;

public class Receta
{
    public int Id { get; set; }
    public int PlatoId { get; set; }
    public Producto Plato { get; set; } = null!;
    public int InsumoId { get; set; }
    public Producto Insumo { get; set; } = null!;
    public decimal CantidadRequerida { get; set; }
    public string Unidad { get; set; } = "UNIDAD";
    public decimal CostoUnitario { get; set; }
    [NotMapped] public decimal CostoTotal => CantidadRequerida * CostoUnitario;
}