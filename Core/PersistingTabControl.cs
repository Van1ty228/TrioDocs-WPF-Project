using System.Windows;
using System.Windows.Controls;

namespace TrioDocs.Core
{
    public class PersistingTabControl : TabControl
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PersistingTabItem();
        }
    }

    public class PersistingTabItem : TabItem
    {
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            // Эта логика предотвращает очистку Content при потере фокуса вкладкой
        }
    }
}