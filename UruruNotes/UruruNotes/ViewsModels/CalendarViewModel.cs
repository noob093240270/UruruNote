using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UruruNotes.Models;
using UruruNotes.Views;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Diagnostics;
using System.Windows;


namespace UruruNotes.ViewsModels
{
    public class CalendarViewModel : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




        private DateTime _currentDate;
        private bool _isTaskPanelVisible;
        private double _scale = 1.0;


        public ObservableCollection<DayViewModel> Days { get; private set; }
        public string CurrentMonthYear => $"{_currentDate:MMMM yyyy}";

        public bool IsTaskPanelVisible
        {
            get => _isTaskPanelVisible;
            set
            {
                _isTaskPanelVisible = value;
                OnPropertyChanged();
            }
        }

        private string _newTaskContent;

        public string NewTaskContent
        {
            get => _newTaskContent;
            set
            {
                if (_newTaskContent != value)
                {
                    _newTaskContent = value;
                    OnPropertyChanged(nameof(NewTaskContent));
                }
                
            }
        }

        private string _newTaskContentRemind;

        public string NewTaskContentRemind
        {
            get => _newTaskContentRemind;
            set
            {
                if (_newTaskContentRemind != value)
                {
                    _newTaskContentRemind = value;
                    OnPropertyChanged(nameof(NewTaskContentRemind));
                }
            }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }
        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    Debug.WriteLine($"CalendarViewModel.Scale изменён на: {_scale}");
                    UpdateScaledProperties();
                    OnPropertyChanged();
                }
            }
        }
        // Динамические свойства для размеров
        private double _buttonWidth = 50;
        public double ButtonWidth
        {
            get => _buttonWidth;
            set
            {
                _buttonWidth = value;
                OnPropertyChanged();
            }
        }

        private double _buttonHeight = 30;
        public double ButtonHeight
        {
            get => _buttonHeight;
            set
            {
                _buttonHeight = value;
                OnPropertyChanged();
            }
        }

        private Thickness _marginTop = new Thickness(10);
        public Thickness MarginTop
        {
            get => _marginTop;
            set
            {
                _marginTop = value;
                OnPropertyChanged();
            }
        }

        private Thickness _marginMiddle = new Thickness(20, 0, 20, 0);
        public Thickness MarginMiddle
        {
            get => _marginMiddle;
            set
            {
                _marginMiddle = value;
                OnPropertyChanged();
            }
        }

        private Thickness _marginDays = new Thickness(10);
        public Thickness MarginDays
        {
            get => _marginDays;
            set
            {
                _marginDays = value;
                OnPropertyChanged();
            }
        }

        private Thickness _marginButtons = new Thickness(5);
        public Thickness MarginButtons
        {
            get => _marginButtons;
            set
            {
                _marginButtons = value;
                OnPropertyChanged();
            }
        }

        private Thickness _marginNotes = new Thickness(10);
        public Thickness MarginNotes
        {
            get => _marginNotes;
            set
            {
                _marginNotes = value;
                OnPropertyChanged();
            }
        }

        private double _titleFontSize = 20;
        public double TitleFontSize
        {
            get => _titleFontSize;
            set
            {
                _titleFontSize = value;
                OnPropertyChanged();
            }
        }

        private double _dayFontSize = 12;
        public double DayFontSize
        {
            get => _dayFontSize;
            set
            {
                _dayFontSize = value;
                OnPropertyChanged();
            }
        }

        private double _noteTitleFontSize = 20;
        public double NoteTitleFontSize
        {
            get => _noteTitleFontSize;
            set
            {
                _noteTitleFontSize = value;
                OnPropertyChanged();
            }
        }

        private double _noteFontSize = 14;
        public double NoteFontSize
        {
            get => _noteFontSize;
            set
            {
                _noteFontSize = value;
                OnPropertyChanged();
            }
        }

        private double _textBoxHeight = 100;
        public double TextBoxHeight
        {
            get => _textBoxHeight;
            set
            {
                _textBoxHeight = value;
                OnPropertyChanged();
            }
        }
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand ShowTaskPanelCommand { get; }
        

        private ICommand _saveTaskCommand;
        public ICommand SaveTaskCommand => _saveTaskCommand ??= new RelayCommand(SaveTask);

        private ICommand _openTaskAreaCommand;
        public ICommand OpenTaskAreaCommand
        {
            get
            {
                return _openTaskAreaCommand ??= new RelayCommand<DayViewModel>(OpenTaskArea);
            }
        }
        public CalendarViewModel()
        {
            _currentDate = DateTime.Today;

            Days = new ObservableCollection<DayViewModel>();
            PreviousMonthCommand = new RelayCommand(ShowPreviousMonth);
            NextMonthCommand = new RelayCommand(ShowNextMonth);
            _openTaskAreaCommand = new RelayCommand<DayViewModel>(OpenTaskArea);
            _saveTaskCommand = new RelayCommand(SaveTask);

            UpdateCalendar();
            UpdateScaledProperties();
        }
        private void UpdateScaledProperties()
        {
            ButtonWidth = Math.Max(50 * Scale, 30); 
            ButtonHeight = Math.Max(30 * Scale, 20); 
            MarginTop = new Thickness(10 * Scale);
            MarginMiddle = new Thickness(20 * Scale, 0, 20 * Scale, 0);
            MarginDays = new Thickness(10 * Scale);
            MarginButtons = new Thickness(5 * Scale);
            MarginNotes = new Thickness(10 * Scale);
            TitleFontSize = Math.Max(20 * Scale, 12); 
            DayFontSize = Math.Max(12 * Scale, 8); 
            NoteTitleFontSize = Math.Max(20 * Scale, 12); 
            NoteFontSize = Math.Max(14 * Scale, 10); 
            TextBoxHeight = Math.Max(100 * Scale, 50); 
        }
        private void UpdateCalendar()
        {
            Days.Clear();

            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
            int startDayOffset = (int)firstDayOfMonth.DayOfWeek;
            if (System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
            {
                startDayOffset = (startDayOffset == 0) ? 6 : startDayOffset - 1;
            }
            var previousMonth = firstDayOfMonth.AddMonths(-1);
            int daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);

            for (int i = daysInPreviousMonth - startDayOffset + 1; i <= daysInPreviousMonth; i++)
            {
                Days.Add(new DayViewModel
                {
                    Date = null, // Дни предыдущего месяца
                    DisplayText = i.ToString(),
                    IsCurrentMonth = false
                });
            }

            // Заполнение текущего месяца
            for (int i = 1; i <= daysInMonth; i++)
            {
                Days.Add(new DayViewModel
                {
                    Date = new DateTime(_currentDate.Year, _currentDate.Month, i),
                    DisplayText = i.ToString(),
                    IsCurrentMonth = true
                });
            }

            // Заполнение дней следующего месяца
            int remainingDays = 35 - Days.Count; // Для полного отображения (6 строк по 7 дней)
            for (int i = 1; i <= remainingDays; i++)
            {
                Days.Add(new DayViewModel
                {
                    Date = null, // Дни следующего месяца
                    DisplayText = i.ToString(),
                    IsCurrentMonth = false
                });
            }

            OnPropertyChanged(nameof(CurrentMonthYear));
        }

        private void ShowPreviousMonth()
        {
            _currentDate = _currentDate.AddMonths(-1);
            UpdateCalendar();
        }

        private void ShowNextMonth()
        {
            _currentDate = _currentDate.AddMonths(1);
            UpdateCalendar();
        }

        private void ShowTaskPanel(DayViewModel selectedDay)
        {
            if (selectedDay?.Date != null)
            {
                SelectedDate = selectedDay.Date;
                IsTaskPanelVisible = true;
            }
        }

        private void SaveTask()
        {
            // Логика сохранения задачи
            string taskContent = NewTaskContent;
            string taskContentRemind = NewTaskContentRemind;
        }



        private void OpenTaskArea(DayViewModel selectedDay)
        {
            if (selectedDay?.Date != null)
            {
                SelectedDate = selectedDay.Date;
                IsTaskPanelVisible = true; // Показываем панель задач
                NewTaskContent = $"Создание задачи на {selectedDay.Date.Value:dd MMMM yyyy}\n";
                NewTaskContentRemind = $"Создание напоминания на {selectedDay.Date.Value:dd MMMM yyyy}\n";
            }
        }

    }
}
