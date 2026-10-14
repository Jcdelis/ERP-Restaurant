using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ERP.Restaurant.Desktop
{
    public partial class ImpresorasPage : Page
    {
        public ImpresorasPage()
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

            var resp = await http.GetAsync("api/Impresion/impresoras");
            if (resp.IsSuccessStatusCode)
                GridImpresoras.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(await resp.Content.ReadAsStringAsync());
        }
    }
}