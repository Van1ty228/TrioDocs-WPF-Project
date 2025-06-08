using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using TrioDocs.ViewModels.DocumentViewModels;

namespace TrioDocs.Services
{
    public class ExcelFileService : IFileService
    {
        static ExcelFileService()
        {
        }

        public DocumentViewModel OpenFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) throw new Exception("В книге Excel нет листов.");
                var dataTable = new DataTable(worksheet.Name);

                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    dataTable.Columns.Add(firstRowCell.Text);
                }
                for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                    var newRow = dataTable.NewRow();
                    foreach (var cell in row)
                    {
                        newRow[cell.Start.Column - 1] = cell.Text;
                    }
                    dataTable.Rows.Add(newRow);
                }

                var excelVm = new ExcelDocumentViewModel
                {
                    FilePath = filePath,
                    Title = Path.GetFileName(filePath),
                    SheetData = dataTable,
                    IsDirty = false
                };
                return excelVm;
            }
        }

        public void SaveFile(DocumentViewModel document)
        {
            if (!(document is ExcelDocumentViewModel excelDocVm)) return;
            if (string.IsNullOrEmpty(excelDocVm.FilePath))
            {
                SaveFileAs(excelDocVm);
            }
            else
            {
                SaveDataTableToExcel(excelDocVm.SheetData, excelDocVm.FilePath);
            }
        }

        private void SaveFileAs(ExcelDocumentViewModel excelDocVm)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Сохранить таблицу",
                FileName = excelDocVm.Header.Replace("Новая таблица", "Таблица1")
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                excelDocVm.FilePath = saveFileDialog.FileName;
                excelDocVm.Title = Path.GetFileName(saveFileDialog.FileName);
                SaveDataTableToExcel(excelDocVm.SheetData, excelDocVm.FilePath);
            }
        }

        private void SaveDataTableToExcel(DataTable dataTable, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            using (var package = new ExcelPackage(fileInfo))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("Лист1");
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);
                package.Save();
            }
        }
    }
}