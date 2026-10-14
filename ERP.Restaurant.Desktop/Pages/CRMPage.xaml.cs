using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ERP.Restaurant.Desktop
{
    public partial class CRMPage : Page
    {
        public CRMPage()
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

            var clientesResp = await http.GetAsync("api/Clientes");
            if (clientesResp.IsSuccessStatusCode)
                GridClientes.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(await clientesResp.Content.ReadAsStringAsync());

            var topResp = await http.GetAsync("api/Clientes/top-clientes");
            if (topResp.IsSuccessStatusCode)
                GridTop.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(await topResp.Content.ReadAsStringAsync());

            var cumpleResp = await http.GetAsync("api/Clientes/cumpleanos-hoy");
            if (cumpleResp.IsSuccessStatusCode)
                GridCumple.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(await cumpleResp.Content.ReadAsStringAsync());
        }
    }
}