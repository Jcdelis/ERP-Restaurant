namespace ERP.Restaurant.Core.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = "";
    public string? RUC_NIT { get; set; }
    public string? Direccion { get; set; }
    public string MonedaBase { get; set; } = "PEN";
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}