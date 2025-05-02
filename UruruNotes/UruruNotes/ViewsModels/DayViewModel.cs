using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace UruruNotes.ViewsModels
{
    public class DayViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DateTime? _date;
        public DateTime? Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        private string _displayText;
        public string DisplayText
        {
            get => _displayText;
            set
            {
                _displayText = value;
                OnPropertyChanged();
            }
        }

        private bool _isCurrentMonth;
        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set
            {
                _isCurrentMonth = value;
                OnPropertyChanged();
            }
        }

        // Новое свойство для отображения красной точки
        private bool _hasNote;
        public bool HasNote
        {
            get => _hasNote;
            set
            {
                _hasNote = value;
                OnPropertyChanged();
            }
        }


        private string _nearestReminder;
        public string NearestReminder
        {
            get => _nearestReminder;
            set
            {
                _nearestReminder = value;
                OnPropertyChanged();
            }
        }
    }
}
