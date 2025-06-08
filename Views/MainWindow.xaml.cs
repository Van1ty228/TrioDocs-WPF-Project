using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TrioDocs.ViewModels;
using TrioDocs.ViewModels.DocumentViewModels;

namespace TrioDocs
{
    public partial class MainWindow : MetroWindow
    {
        private MainViewModel ViewModel => DataContext as MainViewModel;
        public MainWindow() { InitializeComponent(); }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (ViewModel != null && ViewModel.Documents.Any(doc => doc.IsDirty))
            {
                var result = MessageBox.Show("Сохранить изменения?", "TrioDocs", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes) ViewModel.SaveAllFilesCommand.Execute(null);
                else if (result == MessageBoxResult.Cancel) e.Cancel = true;
            }
        }

        private void NewDocument_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button) { button.ContextMenu.PlacementTarget = button; button.ContextMenu.IsOpen = true; }
        }
        private void OpenFile_Click(object sender, RoutedEventArgs e) => ViewModel?.OpenFileCommand.Execute(null);
        private void SaveFile_Click(object sender, RoutedEventArgs e) => ViewModel?.SaveFileCommand.Execute(null);
        private void AddRow_Click(object sender, RoutedEventArgs e) => (ViewModel?.SelectedDocument as ExcelDocumentViewModel)?.AddRowCommand.Execute(null);
        private void DeleteRow_Click(object sender, RoutedEventArgs e) => (ViewModel?.SelectedDocument as ExcelDocumentViewModel)?.DeleteRowCommand.Execute(null);

        #region Word Formatting Handlers
        private void ToggleBold_Click(object sender, RoutedEventArgs e)
        {
            if (GetActiveRichTextBox() is RichTextBox rtb) EditingCommands.ToggleBold.Execute(null, rtb);
        }
        private void ToggleItalic_Click(object sender, RoutedEventArgs e)
        {
            if (GetActiveRichTextBox() is RichTextBox rtb) EditingCommands.ToggleItalic.Execute(null, rtb);
        }
        private void ToggleUnderline_Click(object sender, RoutedEventArgs e)
        {
            if (GetActiveRichTextBox() is RichTextBox rtb) EditingCommands.ToggleUnderline.Execute(null, rtb);
        }
        private void FontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is FontFamily fontFamily)
                if (GetActiveRichTextBox() is RichTextBox rtb) rtb.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
        }
        private void FontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && double.TryParse(comboBox.Text, out double fontSize))
                if (GetActiveRichTextBox() is RichTextBox rtb) rtb.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
        }
        #endregion

        #region RichTextBox Handlers
        private void RichTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is RichTextBox rtb && rtb.DataContext is WordDocumentViewModel vm)
            {
                rtb.Document = vm.DocumentContent;
                if (DataContext is MainViewModel mainVm) mainVm.ActiveRichTextBox = rtb;
                UpdateRichTextBoxSelection(rtb);
            }
        }
        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is RichTextBox rtb && rtb.DataContext is WordDocumentViewModel vm) { vm.OnTextChanged(); vm.DocumentContent = rtb.Document; }
        }
        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RichTextBox rtb) UpdateRichTextBoxSelection(rtb);
        }
        private void UpdateRichTextBoxSelection(RichTextBox rtb)
        {
            if (!(rtb.DataContext is WordDocumentViewModel vm)) return;
            vm.IsSelectionBold = rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty) is FontWeight w && w == FontWeights.Bold;
            vm.IsSelectionItalic = rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty) is FontStyle s && s == FontStyles.Italic;
            vm.IsSelectionUnderline = rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection d && d.Count > 0;
            if (rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty) is FontFamily ff) vm.SelectedFontFamily = ff;
            if (rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty) is double fs) vm.SelectedFontSize = fs;
            vm.RaisePropertyChanged(nameof(vm.IsSelectionBold));
            vm.RaisePropertyChanged(nameof(vm.IsSelectionItalic));
            vm.RaisePropertyChanged(nameof(vm.IsSelectionUnderline));
            vm.RaisePropertyChanged(nameof(vm.SelectedFontFamily));
            vm.RaisePropertyChanged(nameof(vm.SelectedFontSize));
        }
        #endregion

        #region Excel Handlers
        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null || !(dataGrid.DataContext is ExcelDocumentViewModel vm)) return;
            if (dataGrid.Columns.Count == 0 && vm.SheetData != null)
            {
                var textBlockStyle = new Style(typeof(TextBlock));
                textBlockStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                var textBoxStyle = new Style(typeof(TextBox));
                textBoxStyle.Setters.Add(new Setter(TextBox.TextWrappingProperty, TextWrapping.Wrap));
                textBoxStyle.Setters.Add(new Setter(TextBox.AcceptsReturnProperty, true));
                foreach (DataColumn dataColumn in vm.SheetData.Columns)
                {
                    dataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = dataColumn.ColumnName,
                        Binding = new Binding(dataColumn.ColumnName),
                        Width = 100,
                        ElementStyle = textBlockStyle,
                        EditingElementStyle = textBoxStyle
                    });
                }
            }
        }
        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
        #endregion

        #region UI Helpers
        private RichTextBox GetActiveRichTextBox()
        {
            var tabItem = MainTabControl.ItemContainerGenerator.ContainerFromItem(ViewModel.SelectedDocument) as TabItem;
            return FindVisualChild<RichTextBox>(tabItem);
        }
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }
        #endregion
    }
}