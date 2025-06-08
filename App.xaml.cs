using System.Windows;
using TrioDocs.Views;

namespace TrioDocs
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow();
            var loginView = new LoginView();
            if (loginView.ShowDialog() == true)
            {
                mainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }
    }
}