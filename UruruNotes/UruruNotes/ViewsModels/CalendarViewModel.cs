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
using System.IO;
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
        

        // Команда для сохранения заметки
        private ICommand _saveNoteCommand;
        public ICommand SaveNoteCommand => _saveNoteCommand ??= new RelayCommand(SaveNote);

        // Команда для сохранения напоминания
        private ICommand _saveReminderCommand;
        public ICommand SaveReminderCommand => _saveReminderCommand ??= new RelayCommand(SaveReminder);

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
            EnsureFoldersExist();

            Days = new ObservableCollection<DayViewModel>();
            PreviousMonthCommand = new RelayCommand(ShowPreviousMonth);
            NextMonthCommand = new RelayCommand(ShowNextMonth);
            _openTaskAreaCommand = new RelayCommand<DayViewModel>(OpenTaskArea);
            _notes = new ObservableCollection<Note>();
            

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
                var date = new DateTime(_currentDate.Year, _currentDate.Month, i);
                var dayViewModel = new DayViewModel
                {
                    Date = date,
                    DisplayText = i.ToString(),
                    IsCurrentMonth = true
                };

                // Проверяем, есть ли заметка или напоминание для этого дня
                CheckForNoteAndReminder(dayViewModel);

                Days.Add(dayViewModel);
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


        private void CheckForNoteAndReminder(DayViewModel dayViewModel)
        {
            if (dayViewModel.Date.HasValue)
            {
                string noteFilePath = GetNotesFilePath(dayViewModel.Date.Value, false);
                string reminderFilePath = GetNotesFilePath(dayViewModel.Date.Value, true);

                // Проверяем наличие заметки
                if (File.Exists(noteFilePath))
                {
                    dayViewModel.HasNote = true;
                }

                // Проверяем наличие напоминания
                if (File.Exists(reminderFilePath))
                {
                    string reminderContent = File.ReadAllText(reminderFilePath);
                    var timeMatch = System.Text.RegularExpressions.Regex.Match(reminderContent, @"\*\*Время напоминания:\*\* (\d{2}:\d{2})");
                    if (timeMatch.Success)
                    {
                        dayViewModel.ReminderTime = timeMatch.Groups[1].Value;
                    }
                }
            }
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

        private void EnsureFoldersExist()
        {
            string baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UruruNotes");
            string notesFolder = Path.Combine(baseFolder, "Notes");
            string remindersFolder = Path.Combine(baseFolder, "Reminders");

            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            if (!Directory.Exists(notesFolder))
            {
                Directory.CreateDirectory(notesFolder);
            }

            if (!Directory.Exists(remindersFolder))
            {
                Directory.CreateDirectory(remindersFolder);
            }
        }

        private void LoadNotesForDate(DateTime date)
        {
            string noteFilePath = GetNotesFilePath(date, false);
            string reminderFilePath = GetNotesFilePath(date, true);

            // Загружаем заметку
            if (File.Exists(noteFilePath))
            {
                NewTaskContent = File.ReadAllText(noteFilePath);
            }
            else
            {
                NewTaskContent = $"# Создание задачи на {date:dd MMMM yyyy}\n";
            }

            // Загружаем напоминание
            if (File.Exists(reminderFilePath))
            {
                string reminderContent = File.ReadAllText(reminderFilePath);

                // Удаляем строку с временем напоминания из текста, который отображается в TextBox
                NewTaskContentRemind = System.Text.RegularExpressions.Regex.Replace(
                    reminderContent,
                    @"\*\*Время напоминания:\*\* \d{2}:\d{2}",
                    "").Trim();

                // Парсим время напоминания (если оно есть)
                var timeMatch = System.Text.RegularExpressions.Regex.Match(reminderContent, @"\*\*Время напоминания:\*\* (\d{2}:\d{2})");
                if (timeMatch.Success)
                {
                    if (TimeSpan.TryParse(timeMatch.Groups[1].Value, out var reminderTime))
                    {
                        SelectedHour = reminderTime.Hours;
                        SelectedMinute = reminderTime.Minutes;
                    }
                }
            }
            else
            {
                NewTaskContentRemind = $"# Создание напоминания на {date:dd MMMM yyyy}\n";
                SelectedHour = 8;
                SelectedMinute = 0;
            }
        }


        // Метод для сохранения заметки
        private void SaveNote()
        {
            if (SelectedDate.HasValue && !string.IsNullOrEmpty(NewTaskContent))
            {
                var note = new Note
                {
                    Date = SelectedDate.Value,
                    Content = NewTaskContent,
                    IsReminder = false
                };

                // Сохраняем заметку
                SaveNoteForDate(SelectedDate.Value, note);

                MessageBox.Show("Заметка успешно сохранена!");

                // Обновляем календарь
                UpdateCalendar();
            }
            else
            {
                MessageBox.Show("Ошибка: не все данные для заметки заполнены.");
            }
        }

        // Метод для сохранения напоминания
        private void SaveReminder()
        {
            if (SelectedDate.HasValue && !string.IsNullOrEmpty(NewTaskContentRemind))
            {
                var reminder = new Note
                {
                    Date = SelectedDate.Value,
                    Content = NewTaskContentRemind,
                    IsReminder = true,
                    ReminderTime = SelectedReminderTime
                };

                // Сохраняем напоминание
                SaveReminderForDate(SelectedDate.Value, reminder);

                MessageBox.Show("Напоминание успешно сохранено!");

                // Обновляем календарь
                UpdateCalendar();
            }
            else
            {
                MessageBox.Show("Ошибка: не все данные для напоминания заполнены.");
            }
        }

        // Метод для сохранения заметки
        private void SaveNoteForDate(DateTime date, Note note)
        {
            string noteFilePath = GetNotesFilePath(date, false);
            File.WriteAllText(noteFilePath, note.Content);
        }

        // Метод для сохранения напоминания
        private void SaveReminderForDate(DateTime date, Note reminder)
        {
            string reminderFilePath = GetNotesFilePath(date, true);
            string reminderContent = $"{reminder.Content}\n\n**Время напоминания:** {reminder.ReminderTime:hh\\:mm}";
            File.WriteAllText(reminderFilePath, reminderContent);
        }

        private void SaveNotesForDate(DateTime date, Note note, Note reminder)
        {
            // Сохраняем заметку
            string noteFilePath = GetNotesFilePath(date, false);
            File.WriteAllText(noteFilePath, note.Content);

            // Сохраняем напоминание
            string reminderFilePath = GetNotesFilePath(date, true);
            string reminderContent = $"{reminder.Content}\n\n**Время напоминания:** {reminder.ReminderTime:hh\\:mm}";
            File.WriteAllText(reminderFilePath, reminderContent);
        }

        private string GetNotesFilePath(DateTime date, bool isReminder)
        {
            string baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UruruNotes");
            string subFolder = isReminder ? "Reminders" : "Notes";
            string fileName = $"{(isReminder ? "reminder" : "note")}_{date:dd-MM-yyyy}.md";
            return Path.Combine(baseFolder, subFolder, fileName);
        }

        



    }
}
