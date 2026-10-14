using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using Newtonsoft.Json;

namespace ERP.Restaurant.Desktop
{
    public partial class LoginWindow : Window
    {
        private readonly HttpClient _http = new HttpClient { BaseAddress = new Uri("http://localhost:5000/") };

        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            TxtError.Text = "";
            try
            {
                var payload = new { email = TxtEmail.Text, password = TxtPassword.Password };
                var response = await _http.PostAsJsonAsync("api/Auth/login", payload);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(json);

                    App.Current.Properties["Token"] = (string)data.token;
                    App.Current.Properties["Usuario"] = (string)data.nombreCompleto;
                    App.Current.Properties["Rol"] = (string)data.rol;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    TxtError.Text = "Credenciales inválidas";
                }
            }
            catch (Exception ex)
            {
                TxtError.Text = $"Error: {ex.Message}";
            }
        }
    }
}