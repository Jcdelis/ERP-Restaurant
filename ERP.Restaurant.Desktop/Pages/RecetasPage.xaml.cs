using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ERP.Restaurant.Desktop
{
    public partial class RecetasPage : Page
    {
        public RecetasPage()
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

            var resp = await http.GetAsync("api/Recetas/todos-los-costos");
            if (resp.IsSuccessStatusCode)
                GridRecetas.ItemsSource = JsonConvert.DeserializeObject<System.Collections.IEnumerable>(await resp.Content.ReadAsStringAsync());
        }
    }
}