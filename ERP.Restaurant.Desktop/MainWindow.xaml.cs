using System;
using System.Windows;

namespace ERP.Restaurant.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TxtUsuario.Text = $"👤 {App.Current.Properties["Usuario"]}";
            TxtRol.Text = $"🔐 Rol: {App.Current.Properties["Rol"]}";

            // Cargar POS por defecto
            MainFrame.Navigate(new POSPage());
        }

        private void BtnPOS_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new POSPage());
        private void BtnCompras_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ComprasPage());
        private void BtnCRM_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CRMPage());
        private void BtnRecetas_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new RecetasPage());
        private void BtnReportes_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ReportesPage());
        private void BtnImpresoras_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ImpresorasPage());

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Properties.Clear();
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}