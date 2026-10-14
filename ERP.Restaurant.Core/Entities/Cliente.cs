namespace ERP.Restaurant.Core.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Documento { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime? FechaCumpleanos { get; set; }
    public int Puntos { get; set; } = 0;
    public string Nivel { get; set; } = "BRONCE"; // BRONCE, PLATA, ORO, DIAMANTE
    public decimal TotalCompras { get; set; } = 0;
    public int Visitas { get; set; } = 0;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public ICollection<HistorialPuntos> Historial { get; set; } = new List<HistorialPuntos>();
}

public class HistorialPuntos
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int Puntos { get; set; }
    public string Tipo { get; set; } = ""; // GANADO, CANJEADO, VENCIDO
    public string? Motivo { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}

public class Promocion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = ""; // PORCENTAJE, MONTO_FIJO, PRODUCTO_GRATIS
    public decimal Valor { get; set; }
    public decimal? MinimoCompra { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activa { get; set; } = true;
}