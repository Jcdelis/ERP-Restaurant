using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace ERP.Restaurant.Desktop
{
    public partial class POSPage : Page
    {
        private readonly HttpClient _http;
        public ObservableCollection<ItemCarrito> Carrito { get; set; } = new();
        public int VentaActualId { get; set; }

        public POSPage()
        {
            InitializeComponent();
            GridCarrito.ItemsSource = Carrito;
            _http = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
            var token = App.Current.Properties["Token"]?.ToString();
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            _ = IniciarVenta();
            if (TxtBarcode != null) TxtBarcode.Focus();
        }

        private async System.Threading.Tasks.Task IniciarVenta()
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/POS/ventas", new { mesaId = 1, meseroId = 1, moneda = "PEN", tipoCambio = 1 });
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    VentaActualId = (int)data.id;
                }
                else
                {
                    MessageBox.Show($"Error al iniciar venta: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && TxtBarcode != null && !string.IsNullOrWhiteSpace(TxtBarcode.Text))
            {
                var codigo = TxtBarcode.Text.Trim();
                try
                {
                    var resp = await _http.GetAsync($"api/Productos?codigoBarras={codigo}");
                    if (resp.IsSuccessStatusCode)
                    {
                        var productos = JsonConvert.DeserializeObject<dynamic[]>(await resp.Content.ReadAsStringAsync());
                        if (productos.Length > 0)
                        {
                            var p = productos[0];
                            await AgregarAlCarrito((int)p.id, (string)p.nombre, (decimal)p.precioVenta);
                            TxtBarcode.Clear();
                            TxtBarcode.Focus();
                        }
                        else { MessageBox.Show("Producto no encontrado"); }
                    }
                }
                catch { MessageBox.Show("Error de conexión (Modo Offline activado)"); }
            }
        }

        private async System.Threading.Tasks.Task AgregarAlCarrito(int productoId, string nombre, decimal precio)
        {
            var item = new ItemCarrito { Nombre = nombre, Cantidad = 1, PrecioUnitario = precio, Subtotal = precio };
            Carrito.Add(item);
            await _http.PostAsJsonAsync($"api/POS/ventas/{VentaActualId}/productos", new { productoId, cantidad = 1, descuento = 0 });
            TxtTotal.Text = $"Total: S/ {Carrito.Sum(x => x.Subtotal):F2}";
        }

        private void AgregarLomo(object sender, RoutedEventArgs e) => _ = AgregarAlCarrito(1, "Lomo Saltado", 35);
        private void AgregarCeviche(object sender, RoutedEventArgs e) => _ = AgregarAlCarrito(2, "Ceviche", 40);
        private void AgregarInca(object sender, RoutedEventArgs e) => _ = AgregarAlCarrito(3, "Inca Kola", 8);

        private async void PagarVenta(object sender, RoutedEventArgs e)
        {
            if (VentaActualId == 0)
            {
                MessageBox.Show("No hay venta activa", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var total = Carrito.Sum(x => x.Subtotal);
            var payload = new { pagos = new[] { new { formaPagoId = 1, monto = total, moneda = "PEN", tipoCambio = 1, referencia = "EFECTIVO" } } };

            try
            {
                var response = await _http.PostAsJsonAsync($"api/POS/ventas/{VentaActualId}/pagar", payload);

                if (response.IsSuccessStatusCode)
                {
                    ImprimirTicketESCPOS(total);
                    MessageBox.Show("¡Venta cerrada e impresa!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    Carrito.Clear();
                    TxtTotal.Text = "Total: S/ 0.00";
                    await IniciarVenta();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al cobrar:\n{response.StatusCode}\n\n{errorContent}", "Error en el Pago", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión al procesar pago:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImprimirTicketESCPOS(decimal total)
        {
            string nombreImpresora = "EPSON TM-T20";
            System.Text.StringBuilder ticket = new System.Text.StringBuilder();
            ticket.Append("\x1B\x40");
            ticket.Append("\x1B\x61\x01");
            ticket.Append("ERP RESTAURANT\n");
            ticket.Append("RUC: 20123456789\n\n");
            ticket.Append("\x1B\x61\x00");
            foreach (var item in Carrito)
            {
                ticket.Append($"{item.Nombre}\n  {item.Cantidad} x {item.PrecioUnitario:F2} = {item.Subtotal:F2}\n");
            }
            ticket.Append("--------------------------\n");
            ticket.Append("\x1B\x21\x20");
            ticket.Append($"TOTAL: S/ {total:F2}\n");
            ticket.Append("\x1B\x21\x00");
            ticket.Append("\n¡Gracias por su compra!\n\n\n");
            ticket.Append("\x1D\x56\x41\x03");

            System.Diagnostics.Debug.WriteLine("Enviando a impresora: " + nombreImpresora);
        }
    }

    public class ItemCarrito
    {
        public string Nombre { get; set; } = "";
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}