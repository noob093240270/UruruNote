using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    }


}
