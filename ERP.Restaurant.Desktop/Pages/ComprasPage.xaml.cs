using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ERP.Restaurant.Desktop
{
    public partial class ComprasPage : Page
    {
        public ComprasPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await CargarDatos();
        }

        private async System.Threading.Tasks.Task CargarDatos()
        {
            var http = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };
            var token = App.Current.Properties["Token"]?.ToString();
            if (!string.IsNullOrEmpty(token))
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var provResp = await http.GetAsync("api/Compras/proveedores");
            if (provResp.IsSuccessStatusCode)
            {
                var json = await provResp.Content.ReadAsStringAsync();
                GridProveedores.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(json);
            }

            var cuentasResp = await http.GetAsync("api/Compras/cuentas-pagar");
            if (cuentasResp.IsSuccessStatusCode)
            {
                var json = await cuentasResp.Content.ReadAsStringAsync();
                GridCuentas.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(json);
            }
        }
    }
}