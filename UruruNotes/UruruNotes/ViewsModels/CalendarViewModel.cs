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
using System.Xml;           // Из HEAD
using System.Diagnostics;   // Из HEAD
using System.IO;           // Из ветки
using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32.TaskScheduler;

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

        private ObservableCollection<Note> _notes;
        public ObservableCollection<Note> Notes
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
                    OnPropertyChanged();
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

        /*public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand ShowTaskPanelCommand { get; }


        // Команда для сохранения заметки
        private ICommand _saveNoteCommand;
        public ICommand SaveNoteCommand => _saveNoteCommand ??= new RelayCommand(SaveNote);

        // Команда для сохранения напоминания
        private ICommand _saveReminderCommand;
        public ICommand SaveReminderCommand => _saveReminderCommand ??= new RelayCommand(SaveReminder);

        private ICommand _openTaskAreaCommand;
        public ICommand OpenTaskAreaCommand;*/

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
        public ICommand ShowTaskPanelCommand { get; } // Добавлено
        public ICommand OpenTaskAreaCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand SaveReminderCommand { get; }

        private int _selectedHour;
        public int SelectedHour
        {
            get => _selectedHour;
            set
            {
                _selectedHour = value;
                OnPropertyChanged();
                UpdateReminderTime();
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
                UpdateReminderTime();
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
            Notes = new ObservableCollection<Note>();

            PreviousMonthCommand = new RelayCommand(ShowPreviousMonth);
            NextMonthCommand = new RelayCommand(ShowNextMonth);
            ShowTaskPanelCommand = new RelayCommand<DayViewModel>(ShowTaskPanel); // Добавлено
            OpenTaskAreaCommand = new RelayCommand<DayViewModel>(OpenTaskArea);
            SaveNoteCommand = new RelayCommand(SaveNote);
            SaveReminderCommand = new RelayCommand(SaveReminder);

            Hours = new ObservableCollection<int>(Enumerable.Range(0, 24));
            Minutes = new ObservableCollection<int>(Enumerable.Range(0, 60).Where(m => m % 5 == 0)); // Шаг 5 минут

            SelectedHour = 8;
            SelectedMinute = 0;

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
                    Date = null,
                    DisplayText = i.ToString(),
                    IsCurrentMonth = false
                });
            }

            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(_currentDate.Year, _currentDate.Month, i);
                var dayViewModel = new DayViewModel
                {
                    Date = date,
                    DisplayText = i.ToString(),
                    IsCurrentMonth = true
                };
                CheckForNoteAndReminder(dayViewModel);
                Days.Add(dayViewModel);
            }

            int remainingDays = 42 - Days.Count; // 6 строк по 7 дней
            for (int i = 1; i <= remainingDays; i++)
            {
                Days.Add(new DayViewModel
                {
                    Date = null,
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

                if (File.Exists(noteFilePath))
                {
                    dayViewModel.HasNote = true;
                }

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

            if (!Directory.Exists(baseFolder)) Directory.CreateDirectory(baseFolder);
            if (!Directory.Exists(notesFolder)) Directory.CreateDirectory(notesFolder);
            if (!Directory.Exists(remindersFolder)) Directory.CreateDirectory(remindersFolder);
        }

        private void LoadNotesForDate(DateTime date)
        {
            string noteFilePath = GetNotesFilePath(date, false);
            string reminderFilePath = GetNotesFilePath(date, true);

            if (File.Exists(noteFilePath))
            {
                NewTaskContent = File.ReadAllText(noteFilePath);
            }
            else
            {
                NewTaskContent = $"# Создание задачи на {date:dd MMMM yyyy}\n";
            }

            if (File.Exists(reminderFilePath))
            {
                string reminderContent = File.ReadAllText(reminderFilePath);
                NewTaskContentRemind = System.Text.RegularExpressions.Regex.Replace(
                    reminderContent, @"\*\*Время напоминания:\*\* \d{2}:\d{2}", "").Trim();

                var timeMatch = System.Text.RegularExpressions.Regex.Match(reminderContent, @"\*\*Время напоминания:\*\* (\d{2}:\d{2})");
                if (timeMatch.Success && TimeSpan.TryParse(timeMatch.Groups[1].Value, out var reminderTime))
                {
                    SelectedHour = reminderTime.Hours;
                    SelectedMinute = reminderTime.Minutes;
                }
            }
            else
            {
                NewTaskContentRemind = $"# Создание напоминания на {date:dd MMMM yyyy}\n";
                SelectedHour = 8;
                SelectedMinute = 0;
            }
        }

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
                SaveNoteForDate(SelectedDate.Value, note);
                MessageBox.Show("Заметка успешно сохранена!");
                UpdateCalendar();
            }
            else
            {
                MessageBox.Show("Ошибка: не все данные для заметки заполнены.");
            }
        }

        // Метод для показа уведомления
        /*public void ShowToastNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }*/

        // Метод для планирования уведомления
        /*private async System.Threading.Tasks.Task ScheduleReminderNotification(DateTime reminderTime, string message)
        {
            // Вычисляем, сколько времени осталось до напоминания
            var timeUntilReminder = reminderTime - DateTime.Now;

            // Если время ещё не наступило, ждём
            if (timeUntilReminder > TimeSpan.Zero)
            {
                await System.Threading.Tasks.Task.Delay(timeUntilReminder);

                // Показываем уведомление
                ShowToastNotification("Напоминание", message);
            }
        }
        */
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

                // Сохраняем напоминание в файл
                SaveReminderForDate(SelectedDate.Value, reminder);

                /* Планируем уведомление на указанное время
                DateTime reminderDateTime = SelectedDate.Value.Date + SelectedReminderTime;
                ScheduleReminderNotification(reminderDateTime, NewTaskContentRemind);

                MessageBox.Show("Напоминание успешно сохранено!");*/
                // Планируем уведомление через планировщик задач
                DateTime reminderDateTime = SelectedDate.Value.Date + SelectedReminderTime;
                ScheduleReminderTask(reminderDateTime, NewTaskContentRemind);
                MessageBox.Show("Напоминание успешно сохранено!");
                UpdateCalendar();
            }
            else
            {
                MessageBox.Show("Ошибка: не все данные для напоминания заполнены.");
            }
        }


        // Метод для показа уведомления
        private void ShowToastNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddArgument("action", "openReminder") // Аргумент для обработки
                .AddArgument("date", DateTime.Now.ToString("yyyy-MM-dd")) // Пример: передача даты
                .Show();
        }

        private void ScheduleReminderTask(DateTime reminderTime, string message)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "UruruNotes Reminder";
                    td.Triggers.Add(new TimeTrigger(reminderTime));

                    // Убедимся, что задача не повторяется
                    td.Settings.AllowDemandStart = false;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.AllowHardTerminate = true;

                    // Указываем путь к .exe файлу
                    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string exePath = appPath.Replace(".dll", ".exe"); // Заменяем .dll на .exe

                    td.Actions.Add(new ExecAction(exePath, $"\"{message}\"", null));

                    string taskName = $"UruruNotesReminder_{reminderTime:yyyyMMddHHmm}";
                    ts.RootFolder.RegisterTaskDefinition(taskName, td);

                    Debug.WriteLine($"Задача создана: {taskName}, Время: {reminderTime}, Путь: {exePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при создании задачи: {ex.Message}");
                MessageBox.Show($"Ошибка при создании задачи в планировщике: {ex.Message}");
            }
        }

        private void SaveNoteForDate(DateTime date, Note note)
        {
            string noteFilePath = GetNotesFilePath(date, false);
            File.WriteAllText(noteFilePath, note.Content);
        }

        private void SaveReminderForDate(DateTime date, Note reminder)
        {
            try
            {
                // Получаем путь к файлу напоминания
                string reminderFilePath = GetNotesFilePath(date, true);

                // Формируем содержимое файла
                string reminderContent = $"{reminder.Content}\n\n**Время напоминания:** {reminder.ReminderTime:hh\\:mm}";

                // Сохраняем содержимое в файл
                File.WriteAllText(reminderFilePath, reminderContent);

                Debug.WriteLine($"Напоминание сохранено: {reminderFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении напоминания: {ex.Message}");
                MessageBox.Show($"Ошибка при сохранении напоминания: {ex.Message}");
            }
        }

        private string GetNotesFilePath(DateTime date, bool isReminder)
        {
            // Базовый путь к папке с заметками
            string baseFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UruruNotes");

            // Подпапка для заметок или напоминаний
            string subFolder = isReminder ? "Reminders" : "Notes";

            // Имя файла
            string fileName = $"{(isReminder ? "reminder" : "note")}_{date:dd-MM-yyyy}.md";

            // Полный путь к файлу
            return Path.Combine(baseFolder, subFolder, fileName);
        }
    }
}