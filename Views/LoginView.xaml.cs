using MahApps.Metro.Controls;
using System.Windows;
using TrioDocs.ViewModels;
namespace TrioDocs.Views
{
    public partial class LoginView : MetroWindow
    {
        private LoginViewModel ViewModel => DataContext as LoginViewModel;
        public LoginView() { InitializeComponent(); }
        private void Login_Click(object sender, RoutedEventArgs e) => ViewModel?.LoginCommand.Execute(this);
        private void Register_Click(object sender, RoutedEventArgs e) => ViewModel?.RegisterCommand.Execute(this);
    }
}