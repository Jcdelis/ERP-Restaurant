namespace ERP.Restaurant.Core.Entities;

public class ColaSincronizacion
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public string TipoOperacion { get; set; } = "";
    public string DatosJson { get; set; } = "";
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaProcesamiento { get; set; }
}