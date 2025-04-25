using System.Collections.ObjectModel;
using System.Windows.Data;
using System.IO;

namespace UruruNotes.Models
{

    public class FileItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public ObservableCollection<FileItem> Files { get; set; } = new ObservableCollection<FileItem>();
        public ObservableCollection<FileItem> SubItems { get; set; } = new ObservableCollection<FileItem>();

        public FolderItem ParentFolder { get; set; }  // Ссылка на родительскую папку

        // Метод для удаления файла
        /*public void DeleteFile()
        {
            ParentFolder?.RemoveFile(this);  // Удаляем файл из родительской папки
            try
            {
                // Удаляем файл с диска
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, если не удалось удалить файл
                Console.WriteLine($"Ошибка при удалении файла: {ex.Message}");
            }
        }*/

        public CompositeCollection CompositeSubItems
        {
            get
            {
                var composite = new CompositeCollection
            {
                new CollectionContainer { Collection = SubItems }
            };
                return composite;
            }
        }
    }
}
