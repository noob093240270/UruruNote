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
                _newTaskContent = value;
                OnPropertyChanged();
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
        }

        private void OpenTaskArea(DayViewModel selectedDay)
        {
            if (selectedDay?.Date != null)
            {
                SelectedDate = selectedDay.Date;
                IsTaskPanelVisible = true; // Показываем панель задач
                NewTaskContent = $"Создание задачи на {selectedDay.Date.Value:dd MMMM yyyy}";
            }
        }

    }
}
