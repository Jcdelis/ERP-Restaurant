using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ERP.Restaurant.Desktop
{
    public partial class ReportesPage : Page
    {
        public ReportesPage()
        {
            InitializeComponent();
        }

        private void AbrirDashboard(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:5000/dashboard.html",
                UseShellExecute = true
            });
        }

        private void AbrirSwagger(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://localhost:5000/swagger",
                UseShellExecute = true
            });
        }
    }
}