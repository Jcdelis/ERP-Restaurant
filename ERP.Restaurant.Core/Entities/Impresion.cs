namespace ERP.Restaurant.Core.Entities;

public class Impresora
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string NombreSistema { get; set; } = ""; // Nombre real en Windows
    public string Tipo { get; set; } = "TICKET"; // TICKET, COCINA, BARRA
    public string? Zona { get; set; }
    public bool Activa { get; set; } = true;
}

public class ComandaCocina
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int? MesaId { get; set; }
    public string? Mesero { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "PENDIENTE"; // PENDIENTE, EN_PREPARACION, LISTO, ENTREGADO
    public string? ImpresoraDestino { get; set; }
    public ICollection<ComandaDetalle> Detalles { get; set; } = new List<ComandaDetalle>();
}

public class ComandaDetalle
{
    public int Id { get; set; }
    public int ComandaId { get; set; }
    public ComandaCocina Comanda { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public string? Observacion { get; set; }
    public bool Listo { get; set; } = false;
}