using ControlzEx.Theming;
using System.Windows;

namespace TrioDocs.Core
{
    public static class ThemeManagerHelper
    {
        public static void ChangeTheme(string themeName)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, $"{themeName}.Blue");
        }
    }
}