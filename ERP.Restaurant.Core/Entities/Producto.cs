namespace ERP.Restaurant.Core.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public decimal CostoPromedio { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal StockActual { get; set; }
}