using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TrioDocs.Core;
using TrioDocs.Properties;
using TrioDocs.Services;
using TrioDocs.ViewModels.DocumentViewModels;
using TrioDocs.Views;

namespace TrioDocs.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const int MaxRecentFiles = 5;

        private readonly IFileService _wordFileService;
        private readonly IFileService _excelFileService;

        public ObservableCollection<DocumentViewModel> Documents { get; }
        private DocumentViewModel _selectedDocument;
        public DocumentViewModel SelectedDocument { get => _selectedDocument; set => SetProperty(ref _selectedDocument, value); }
        public RichTextBox ActiveRichTextBox { get; set; }

        public ObservableCollection<string> RecentFiles { get; }

        public ICommand NewWordDocumentCommand { get; }
        public ICommand NewExcelDocumentCommand { get; }
        public ICommand OpenFileCommand { get; }
        public IRelayCommand SaveFileCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand SaveAllFilesCommand { get; }
        public ICommand SetLightThemeCommand { get; }
        public ICommand SetDarkThemeCommand { get; }
        public ICommand ShowAboutDialogCommand { get; }
        public ICommand OpenRecentFileCommand { get; }

        public MainViewModel()
        {
            _wordFileService = new WordFileService();
            _excelFileService = new ExcelFileService();
            Documents = new ObservableCollection<DocumentViewModel>();
            RecentFiles = new ObservableCollection<string>();

            NewWordDocumentCommand = new RelayCommand(CreateNewWordDocument);
            NewExcelDocumentCommand = new RelayCommand(CreateNewExcelDocument);
            OpenFileCommand = new RelayCommand(OpenFile);
            SaveFileCommand = new RelayCommand(SaveFile, CanSaveFile);
            CloseDocumentCommand = new RelayCommand<DocumentViewModel>(CloseDocument);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
            SaveAllFilesCommand = new RelayCommand(SaveAllFiles);
            SetLightThemeCommand = new RelayCommand(() => ThemeManagerHelper.ChangeTheme("Light"));
            SetDarkThemeCommand = new RelayCommand(() => ThemeManagerHelper.ChangeTheme("Dark"));
            ShowAboutDialogCommand = new RelayCommand(ShowAboutDialog);
            OpenRecentFileCommand = new RelayCommand<string>(OpenSpecificFile);

            this.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(SelectedDocument))
                {
                    (SaveFileCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            };

            LoadRecentFiles();
        }

        #region Методы команд
        private void ShowAboutDialog()
        {
            var aboutView = new AboutView();
            aboutView.ShowDialog();
        }

        private void LoadRecentFiles()
        {
            if (Settings.Default.RecentFiles == null)
                Settings.Default.RecentFiles = new System.Collections.Specialized.StringCollection();

            RecentFiles.Clear();
            foreach (var file in Settings.Default.RecentFiles)
                RecentFiles.Add(file);
        }

        private void AddToRecentFiles(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            RecentFiles.Remove(filePath);
            RecentFiles.Insert(0, filePath);
            if (RecentFiles.Count > MaxRecentFiles)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            Settings.Default.RecentFiles.Clear();
            foreach (var file in RecentFiles)
                Settings.Default.RecentFiles.Add(file);
            Settings.Default.Save();
        }

        private void OpenSpecificFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Файл не найден:\n{filePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                RecentFiles.Remove(filePath);
                Settings.Default.RecentFiles.Remove(filePath);
                Settings.Default.Save();
                return;
            }

            DocumentViewModel documentVm = null;
            try
            {
                if (Path.GetExtension(filePath).ToLower() == ".docx") documentVm = _wordFileService.OpenFile(filePath);
                else if (Path.GetExtension(filePath).ToLower() == ".xlsx") documentVm = _excelFileService.OpenFile(filePath);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка при открытии файла:\n{ex.Message}"); return; }

            if (documentVm != null)
            {
                Documents.Add(documentVm);
                SelectedDocument = documentVm;
                AddToRecentFiles(filePath);
            }
        }

        private void SaveAllFiles()
        {
            foreach (var doc in Documents.Where(d => d.IsDirty))
                SaveFileForDocument(doc);
        }

        private void SaveFileForDocument(DocumentViewModel doc)
        {
            if (doc is WordDocumentViewModel) _wordFileService.SaveFile(doc);
            else if (doc is ExcelDocumentViewModel) _excelFileService.SaveFile(doc);

            if (!string.IsNullOrEmpty(doc.FilePath))
            {
                doc.Title = Path.GetFileName(doc.FilePath);
                AddToRecentFiles(doc.FilePath);
            }

            doc.IsDirty = false;
        }

        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog { Filter = "Поддерживаемые документы|*.docx;*.xlsx|Word Documents (*.docx)|*.docx|Excel Workbooks (*.xlsx)|*.xlsx", Title = "Открыть документ" };
            if (openFileDialog.ShowDialog() == true)
                OpenSpecificFile(openFileDialog.FileName);
        }

        private void SaveFile()
        {
            if (SelectedDocument != null)
                SaveFileForDocument(SelectedDocument);
        }

        private bool CanSaveFile() => SelectedDocument != null;

        private void CreateNewWordDocument()
        {
            var newDoc = new WordDocumentViewModel();
            Documents.Add(newDoc);
            SelectedDocument = newDoc;
        }

        private void CreateNewExcelDocument()
        {
            var newDoc = new ExcelDocumentViewModel();
            Documents.Add(newDoc);
            SelectedDocument = newDoc;
        }

        private void CloseDocument(DocumentViewModel document)
        {
            if (document == null) return;
            if (!document.IsDirty) { Documents.Remove(document); return; }
            var result = MessageBox.Show($"Вы хотите сохранить изменения в файле \"{document.Header}\"?", "TrioDocs - Сохранение", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                SaveFileForDocument(document);
                if (!document.IsDirty) Documents.Remove(document);
            }
            else if (result == MessageBoxResult.No)
            {
                Documents.Remove(document);
            }
        }
        #endregion
    }
}