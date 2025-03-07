using System.Collections.ObjectModel;
using System.Windows.Data;

namespace UruruNotes.Models
{
    public class FileItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        // Добавляем свойство для подэлементов
        public ObservableCollection<FileItem> SubItems { get; set; } = new ObservableCollection<FileItem>();

        // Это свойство будет объединять файлы и их подэлементы
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
