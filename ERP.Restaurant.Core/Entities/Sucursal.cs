namespace ERP.Restaurant.Core.Entities;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Direccion { get; set; }
    public string CodigoUnico { get; set; } = "";
    public bool Activa { get; set; } = true;
}