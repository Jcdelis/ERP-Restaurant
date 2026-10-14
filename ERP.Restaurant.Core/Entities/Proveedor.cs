using System.ComponentModel.DataAnnotations.Schema;
namespace ERP.Restaurant.Core.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string RazonSocial { get; set; } = "";
    public string RUC { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<OrdenCompra> Ordenes { get; set; } = new List<OrdenCompra>();
}

public class OrdenCompra
{
    public int Id { get; set; }
    public string Numero { get; set; } = "";
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public ICollection<OrdenCompraDetalle> Detalles { get; set; } = new List<OrdenCompraDetalle>();
    public ICollection<CuentaPagar> CuentasPagar { get; set; } = new List<CuentaPagar>();
}

public class OrdenCompraDetalle
{
    public int Id { get; set; }
    public int OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    [NotMapped] public decimal Subtotal => Cantidad * CostoUnitario;
}

public class CuentaPagar
{
    public int Id { get; set; }
    public int OrdenCompraId { get; set; }
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public decimal MontoOriginal { get; set; }
    public decimal MontoPagado { get; set; }
    [NotMapped] public decimal Saldo => MontoOriginal - MontoPagado;
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
}