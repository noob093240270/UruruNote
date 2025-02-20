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
using System.IO;
using System.Xml.Serialization;
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
        

        public ObservableCollection<DayViewModel> Days { get; private set; }
        public string CurrentMonthYear => $"{_currentDate:MMMM yyyy}";

        private ObservableCollection<Note> _notes;

        public ObservableCollection<Note> Notes // Свойство Notes public
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged();
            }
        }

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
        private int _selectedHour;
        public int SelectedHour
        {
            get => _selectedHour;
            set
            {
                _selectedHour = value;
                OnPropertyChanged();
                UpdateReminderTime(); // Обновляем общее время
            }
        }

        private int _selectedMinute;
        public int SelectedMinute
        {
            get => _selectedMinute;
            set
            {
                _selectedMinute = value;
                OnPropertyChanged();
                UpdateReminderTime(); // Обновляем общее время
            }
        }

        private TimeSpan _selectedReminderTime;
        public TimeSpan SelectedReminderTime
        {
            get => _selectedReminderTime;
            set
            {
                _selectedReminderTime = value;
                OnPropertyChanged();
            }
        }

        // Метод для обновления общего времени
        private void UpdateReminderTime()
        {
            SelectedReminderTime = new TimeSpan(SelectedHour, SelectedMinute, 0);
        }

        public ObservableCollection<int> Hours { get; set; }
        public ObservableCollection<int> Minutes { get; set; }

        public CalendarViewModel()
        {
            _currentDate = DateTime.Today;

            Days = new ObservableCollection<DayViewModel>();
            PreviousMonthCommand = new RelayCommand(ShowPreviousMonth);
            NextMonthCommand = new RelayCommand(ShowNextMonth);
            _openTaskAreaCommand = new RelayCommand<DayViewModel>(OpenTaskArea);
            _saveTaskCommand = new RelayCommand(SaveTask);
            _notes = new ObservableCollection<Note>();
            LoadNotes();
            // Инициализация списков часов и минут
            Hours = new ObservableCollection<int>(Enumerable.Range(0, 24)); // Часы от 0 до 23
            Minutes = new ObservableCollection<int>(Enumerable.Range(0, 60)); // Минуты от 0 до 59

            // Установка значений по умолчанию
            SelectedHour = 8; // По умолчанию 8:00
            SelectedMinute = 0;
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

        private void OpenTaskArea(DayViewModel selectedDay)
        {
            if (selectedDay?.Date != null)
            {
                SelectedDate = selectedDay.Date;
                IsTaskPanelVisible = true; // Показываем панель задач
                
                // Загружаем данные для выбранного дня
                LoadNotesForDate(selectedDay.Date.Value);

                // Устанавливаем текст по умолчанию, если данных нет
                if (string.IsNullOrEmpty(NewTaskContent))
                {
                    NewTaskContent = $"Создание задачи на {selectedDay.Date.Value:dd MMMM yyyy}\n";
                }

                if (string.IsNullOrEmpty(NewTaskContentRemind))
                {
                    NewTaskContentRemind = $"Создание напоминания на {selectedDay.Date.Value:dd MMMM yyyy}\n";
                }
            }
        }

        private void LoadNotesForDate(DateTime date)
        {
            string filePath = GetNotesFilePath(date);
            if (File.Exists(filePath))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
                using (var reader = new StreamReader(filePath))
                {
                    var notesForDate = (ObservableCollection<Note>)serializer.Deserialize(reader);

                    // Обновляем текстовые поля
                    var note = notesForDate.FirstOrDefault(n => !n.IsReminder);
                    var reminder = notesForDate.FirstOrDefault(n => n.IsReminder);

                    NewTaskContent = note?.Content ?? $"Создание задачи на {date:dd MMMM yyyy}\n";
                    NewTaskContentRemind = reminder?.Content ?? $"Создание напоминания на {date:dd MMMM yyyy}\n";
                    // Устанавливаем часы и минуты
                    if (reminder != null)
                    {
                        SelectedHour = reminder.ReminderTime.Hours;
                        SelectedMinute = reminder.ReminderTime.Minutes;
                    }
                    else
                    {
                        SelectedHour = 8; // По умолчанию 8:00
                        SelectedMinute = 0;
                    }
                }
            }
            else
            {
                // Если файл не существует, очищаем поля
                NewTaskContent = $"Создание задачи на {date:dd MMMM yyyy}\n";
                NewTaskContentRemind = $"Создание напоминания на {date:dd MMMM yyyy}\n";
                SelectedHour = 8; // По умолчанию 8:00
                SelectedMinute = 0;
            }
        }

        private void SaveNotes()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "notes.xml");
            var serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, _notes);
            }
        }

        
        private void SaveTask()
        {
            // Логика сохранения задачи
            if (SelectedDate.HasValue && !string.IsNullOrEmpty(NewTaskContent) && !string.IsNullOrEmpty(NewTaskContentRemind))
            {
                var note = new Note
                {
                    Date = SelectedDate.Value,
                    Content = NewTaskContent,
                    IsReminder = false
                };
                

                var reminder = new Note
                {
                    Date = SelectedDate.Value,
                    Content = NewTaskContentRemind,
                    IsReminder = true,
                    ReminderTime = SelectedReminderTime // Сохраняем выбранное время
                };
                

                // Сохраняем данные для текущего дня
                SaveNotesForDate(SelectedDate.Value, note, reminder);

                MessageBox.Show("Данные успешно сохранены!");

            }
            else
            {
                MessageBox.Show("Ошибка: не все данные заполнены.");
            }
        }

        private void SaveNotesForDate(DateTime date, Note note, Note reminder)
        {
            string filePath = GetNotesFilePath(date);
            var notesForDate = new ObservableCollection<Note> { note, reminder };

            var serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, notesForDate);
            }
        }

        private string GetNotesFilePath(DateTime date)
        {
            string fileName = $"notes_{date:dd-MM-yyyy}.xml";
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        }

        private void LoadNotes()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "notes.xml");
            if (File.Exists(filePath))
            {
                var serializer = new XmlSerializer(typeof(ObservableCollection<Note>));
                using (var reader = new StreamReader(filePath))
                {
                    _notes = (ObservableCollection<Note>)serializer.Deserialize(reader);
                }
                                
            }
            else
            {
                _notes = new ObservableCollection<Note>(); // Инициализация, если файл не существует
            }
        }



    }
}
