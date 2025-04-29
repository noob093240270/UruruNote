using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace UruruNotes.Models
{
    public class FolderItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public ObservableCollection<FolderItem> SubFolders { get; set; } = new ObservableCollection<FolderItem>();
        public ObservableCollection<FileItem> Files { get; set; } = new ObservableCollection<FileItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // Удаление файла из коллекции
        public void RemoveFile(FileItem fileItem)
        {
            if (fileItem != null && Files.Contains(fileItem))
            {
                Files.Remove(fileItem);

                // Обновление всех связанных свойств
                OnPropertyChanged(nameof(Files));
                OnPropertyChanged(nameof(CompositeSubItems)); // ⚠️ это критично
            }
        }




        public bool IsFolderNameUnique(string folderName)
        {
            return !SubFolders.Any(f => f.FileName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsFolderNameUniqueInRoot(string folderName)
        {
            return !Files.Any(f => f.FileName.Equals(folderName, StringComparison.OrdinalIgnoreCase)) &&
                   !SubFolders.Any(f => f.FileName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        }


        //public ObservableCollection<object> CompositeSubItems { get; set; } = new ObservableCollection<object>();


        public CompositeCollection CompositeSubItems
        {
            get
            {
                var composite = new CompositeCollection
            {
                new CollectionContainer { Collection = SubFolders },
                new CollectionContainer { Collection = Files }
            };
                return composite;
            }
        }

        public void RebuildComposite()
        {
            CompositeSubItems.Clear();
            foreach (var folder in SubFolders)
                CompositeSubItems.Add(folder);

            foreach (var file in Files)
                CompositeSubItems.Add(file);
        }

    }


}
