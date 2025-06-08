using TrioDocs.Core;
namespace TrioDocs.ViewModels.DocumentViewModels
{
    public abstract class DocumentViewModel : BaseViewModel
    {
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        private bool _isDirty;
        public bool IsDirty { get => _isDirty; set { if (SetProperty(ref _isDirty, value)) OnPropertyChanged(nameof(Header)); } }
        public string Header => $"{Title}{(IsDirty ? "*" : "")}";
        public string FilePath { get; set; }
        public abstract string StatusInfo { get; }
        protected DocumentViewModel() { IsDirty = false; }
        public void RaisePropertyChanged(string propertyName) { OnPropertyChanged(propertyName); }
    }
}