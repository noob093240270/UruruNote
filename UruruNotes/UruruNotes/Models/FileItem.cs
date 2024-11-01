using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNotes.Models
{
    public class FileItem
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        // Добавляем свойство для подэлементов
        public ObservableCollection<FileItem> SubItems { get; set; } = new ObservableCollection<FileItem>();

    }

}