using Microsoft.Win32;
using System.IO;
using System.Windows.Documents;
using TrioDocs.ViewModels.DocumentViewModels;
using Xceed.Words.NET;

namespace TrioDocs.Services
{
    public class WordFileService : IFileService
    {
        public DocumentViewModel OpenFile(string filePath)
        {
            using (var doc = DocX.Load(filePath))
            {
                var flowDoc = new FlowDocument();
                var paragraph = new Paragraph(new Run(doc.Text));
                flowDoc.Blocks.Add(paragraph);

                var wordVm = new WordDocumentViewModel
                {
                    FilePath = filePath,
                    Title = Path.GetFileName(filePath),
                    DocumentContent = flowDoc,
                    IsDirty = false
                };
                return wordVm;
            }
        }

        public void SaveFile(DocumentViewModel document)
        {
            if (!(document is WordDocumentViewModel wordDocVm)) return;
            if (string.IsNullOrEmpty(wordDocVm.FilePath))
            {
                SaveFileAs(wordDocVm);
            }
            else
            {
                CreateDocXFromViewModel(wordDocVm.DocumentContent, wordDocVm.FilePath);
            }
        }

        private void SaveFileAs(WordDocumentViewModel wordDocVm)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Documents (*.docx)|*.docx",
                Title = "Сохранить документ",
                FileName = wordDocVm.Header.Replace("Новый документ", "Документ1")
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                wordDocVm.FilePath = saveFileDialog.FileName;
                wordDocVm.Title = Path.GetFileName(saveFileDialog.FileName);
                CreateDocXFromViewModel(wordDocVm.DocumentContent, wordDocVm.FilePath);
            }
        }

        private void CreateDocXFromViewModel(FlowDocument flowDoc, string filePath)
        {
            string textToSave = new TextRange(flowDoc.ContentStart, flowDoc.ContentEnd).Text;
            using (var doc = DocX.Create(filePath))
            {
                doc.InsertParagraph(textToSave);
                doc.Save();
            }
        }
    }
}