using TrioDocs.ViewModels.DocumentViewModels;

namespace TrioDocs.Services
{
    public interface IFileService
    {
        DocumentViewModel OpenFile(string filePath);
        void SaveFile(DocumentViewModel document);
    }
}