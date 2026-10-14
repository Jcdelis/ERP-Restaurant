namespace ERP.Restaurant.Core.Entities;

public class Zona
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public ICollection<Mesa> Mesas { get; set; } = new List<Mesa>();
}