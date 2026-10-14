using Microsoft.EntityFrameworkCore;
using ERP.Restaurant.Core.Entities;

namespace ERP.Restaurant.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Moneda> Monedas => Set<Moneda>();
    public DbSet<FormaPago> FormasPago => Set<FormaPago>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Zona> Zonas => Set<Zona>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentaDetalles => Set<VentaDetalle>();
    public DbSet<VentaPago> VentaPagos => Set<VentaPago>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraDetalle> OrdenesCompraDetalle => Set<OrdenCompraDetalle>();
    public DbSet<CuentaPagar> CuentasPagar => Set<CuentaPagar>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<HistorialPuntos> HistorialPuntos => Set<HistorialPuntos>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Impresora> Impresoras => Set<Impresora>();
    public DbSet<ComandaCocina> ComandasCocina => Set<ComandaCocina>();
    public DbSet<ComandaDetalle> ComandasDetalle => Set<ComandaDetalle>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Producto>().HasIndex(p => p.Codigo).IsUnique();
        modelBuilder.Entity<OrdenCompra>().HasIndex(o => o.Numero).IsUnique();

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }

        modelBuilder.Entity<Usuario>().ToTable(t => t.HasCheckConstraint("CK_Usuario_Rol", "[Rol] IN ('ADMIN','CAJERO','MESERO','COCINA')"));
        modelBuilder.Entity<Producto>().ToTable(t => t.HasCheckConstraint("CK_Producto_Importes", "[CostoPromedio] >= 0 AND [PrecioVenta] >= 0"));
        modelBuilder.Entity<VentaDetalle>().ToTable(t => t.HasCheckConstraint("CK_VentaDetalle_Valores", "[Cantidad] > 0 AND [PrecioUnitario] >= 0 AND [Descuento] >= 0"));
        modelBuilder.Entity<VentaPago>().ToTable(t => t.HasCheckConstraint("CK_VentaPago_Monto", "[Monto] > 0 AND [TipoCambio] > 0"));
    }
}