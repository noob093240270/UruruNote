using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNotes.Models
{
    public class FolderItem : FileItem
    {
        public ObservableCollection<FileItem> SubItems { get; set; } = new ObservableCollection<FileItem>();
    }
}
