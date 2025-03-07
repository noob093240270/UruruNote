using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UruruNote.Models
{
    public class Task : INotifyPropertyChanged
    {

        public int Id { get; set; }
        public string _content;
        public string Content
        {  
            get {  return _content; }
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public DateTime ComleteDate { get; set; }
        private bool _isCompleted;
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set { _isCompleted = value; OnPropertyChanged();}
        }


        public virtual TaskList TaskList { get; set; }

        public int IaskListId { get; set; }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}