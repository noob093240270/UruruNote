using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UruruNotes.Models
{
    public class FolderItem
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }
        public ObservableCollection<FolderItem> SubItems { get; set; } = new ObservableCollection<FolderItem>();
    }


}
