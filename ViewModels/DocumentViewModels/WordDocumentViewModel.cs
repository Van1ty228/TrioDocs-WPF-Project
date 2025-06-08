using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;

namespace TrioDocs.ViewModels.DocumentViewModels
{
    public class WordDocumentViewModel : DocumentViewModel
    {
        private FlowDocument _documentContent;
        public FlowDocument DocumentContent { get => _documentContent; set => SetProperty(ref _documentContent, value); }

        public List<FontFamily> FontFamilies { get; }
        public List<double> FontSizes { get; }
        public FontFamily SelectedFontFamily { get; set; }
        public double SelectedFontSize { get; set; }
        public bool IsSelectionBold { get; set; }
        public bool IsSelectionItalic { get; set; }
        public bool IsSelectionUnderline { get; set; }

        public override string StatusInfo
        {
            get
            {
                if (DocumentContent == null) return "Символов: 0 | Слов: 0";
                var text = new TextRange(DocumentContent.ContentStart, DocumentContent.ContentEnd).Text;
                int charCount = text.Trim().Length;
                int wordCount = text.Split(new[] { ' ', '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                return $"Символов: {charCount} | Слов: {wordCount}";
            }
        }

        public WordDocumentViewModel()
        {
            Title = "Новый документ";
            DocumentContent = new FlowDocument(new Paragraph(new Run("")));
            IsDirty = true;

            FontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToList();
            FontSizes = new List<double> { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 72 };
            SelectedFontFamily = new FontFamily("Calibri");
            SelectedFontSize = 12;
        }

        public void OnTextChanged()
        {
            IsDirty = true;
            RaisePropertyChanged(nameof(StatusInfo));
        }
    }
}