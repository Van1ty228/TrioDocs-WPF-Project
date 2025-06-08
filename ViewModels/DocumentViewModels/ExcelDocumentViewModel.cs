using CommunityToolkit.Mvvm.Input;
using System.Data;
using System.Windows.Input;

namespace TrioDocs.ViewModels.DocumentViewModels
{
    public class ExcelDocumentViewModel : DocumentViewModel
    {
        private DataTable _sheetData;
        public DataTable SheetData
        {
            get => _sheetData;
            set
            {
                if (_sheetData != null)
                {
                    _sheetData.RowChanged -= OnContentChanged;
                    _sheetData.ColumnChanged -= OnContentChanged;
                    _sheetData.TableNewRow -= OnContentChanged;
                    _sheetData.RowDeleted -= OnContentChanged;
                }
                SetProperty(ref _sheetData, value);
                if (_sheetData != null)
                {
                    _sheetData.RowChanged += OnContentChanged;
                    _sheetData.ColumnChanged += OnContentChanged;
                    _sheetData.TableNewRow += OnContentChanged;
                    _sheetData.RowDeleted += OnContentChanged;
                }
                OnPropertyChanged(nameof(StatusInfo));
            }
        }

        private DataRowView _selectedRow;
        public DataRowView SelectedRow { get => _selectedRow; set { if (SetProperty(ref _selectedRow, value)) (DeleteRowCommand as RelayCommand)?.NotifyCanExecuteChanged(); } }

        public ICommand AddRowCommand { get; }
        public ICommand DeleteRowCommand { get; }

        public override string StatusInfo
        {
            get
            {
                if (SheetData == null) return "Строк: 0 | Столбцов: 0";
                return $"Строк: {SheetData.Rows.Count} | Столбцов: {SheetData.Columns.Count}";
            }
        }

        public ExcelDocumentViewModel()
        {
            Title = "Новая таблица";
            SheetData = CreateEmptyDataTable();
            IsDirty = true;

            AddRowCommand = new RelayCommand(AddRow);
            DeleteRowCommand = new RelayCommand(DeleteRow, CanDeleteRow);
        }

        private void AddRow() { SheetData.Rows.Add(SheetData.NewRow()); }
        private void DeleteRow() { SelectedRow?.Row.Delete(); }
        private bool CanDeleteRow() => SelectedRow != null;
        private void OnContentChanged(object sender, System.EventArgs e) { IsDirty = true; OnPropertyChanged(nameof(StatusInfo)); }

        private DataTable CreateEmptyDataTable()
        {
            var dt = new DataTable();
            for (int i = 1; i <= 26; i++) dt.Columns.Add(GetExcelColumnName(i));
            for (int i = 0; i < 50; i++) dt.Rows.Add(dt.NewRow());
            return dt;
        }

        private string GetExcelColumnName(int columnNumber)
        {
            string name = "";
            while (columnNumber > 0)
            {
                int rem = (columnNumber - 1) % 26;
                name = (char)('A' + rem) + name;
                columnNumber = (columnNumber - rem) / 26;
            }
            return name;
        }
    }
}