using MahApps.Metro.Controls;
using System.Reflection;
using System.Windows;
namespace TrioDocs.Views
{
    public partial class AboutView : MetroWindow
    {
        public AboutView()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            LoadAssemblyInformation();
        }
        private void LoadAssemblyInformation()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var titleAttribute = (AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
            TitleTextBlock.Text = titleAttribute.Title;
            this.Title = $"О программе {titleAttribute.Title}";
            VersionTextBlock.Text = $"Версия {assembly.GetName().Version}";
            var copyrightAttribute = (AssemblyCopyrightAttribute)assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
            CopyrightTextBlock.Text = copyrightAttribute.Copyright;
            var descriptionAttribute = (AssemblyDescriptionAttribute)assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0];
            DescriptionTextBlock.Text = descriptionAttribute.Description;
        }
    }
}