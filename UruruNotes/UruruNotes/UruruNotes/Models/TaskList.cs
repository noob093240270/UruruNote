using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace UruruNote.Models
{
    public class TaskList : INotifyPropertyChanged
    {

        public int Id { get; set; }
        public string Title { get; set; }


        //Указывает на режим, в котором находится список задач в представлении
        [NotMapped] // не сохраняется в бд
        public bool IsInEditMode { get; set; }


        [NotMapped]
        public bool IsSelected { get; set; }

        public virtual IList<Task> Tasks { get; set; }

        public virtual Category Category { get; set; }

        public int? CategoryId { get; set; }



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
