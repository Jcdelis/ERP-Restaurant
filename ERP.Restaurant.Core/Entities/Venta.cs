using System.ComponentModel.DataAnnotations.Schema;
namespace ERP.Restaurant.Core.Entities;

public class Venta
{
    public int Id { get; set; }
    public string? NumeroComprobante { get; set; }
    public int? MesaId { get; set; }
    public int? MeseroId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "PEN";
    public decimal TipoCambio { get; set; } = 1;
    public string Estado { get; set; } = "ABIERTA";
    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
    public ICollection<VentaPago> Pagos { get; set; } = new List<VentaPago>();
}

public class VentaDetalle
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
    [NotMapped] public decimal Subtotal => (Cantidad * PrecioUnitario) - Descuento;
}

public class VentaPago
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;
    public int FormaPagoId { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "PEN";
    public decimal TipoCambio { get; set; } = 1;
    public string? Referencia { get; set; }
}

public class Mesa
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public string Estado { get; set; } = "LIBRE";
}

public class FormaPago
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}