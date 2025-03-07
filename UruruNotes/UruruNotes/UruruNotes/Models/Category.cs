using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;


namespace UruruNote.Models
{
    public class Category : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private string _title;
        public string Title { 
            get { return _title; } 
            set
            {
                _title = value;
                OnPropertyChanged();
                
            }
        }

        public virtual ObservableCollection<TaskList> TaskLists { get; set; }

        [NotMapped]
        public bool IsInEditMode { get; set; }

        [NotMapped]
        public bool IsSelected { get; set; }

        [NotMapped]
        public bool IsExpanded { get; set; }

        public virtual User User { get; set; }

        public int UserId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) 
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
