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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic; // Для IEnumerable<T>

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

        // enum для типов вкладок
        public enum ViewType { Notes, Reminders }

        
        private bool _isEditing;

        /// <summary>
        /// флаг редактирования
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(EditorButtonText)); // Уведомляем UI об изменении текста кнопки
                OnPropertyChanged(nameof(EditorTitle));
            }
        }

        public ObservableCollection<DayViewModel> Days { get; } = new ObservableCollection<DayViewModel>();
        public string CurrentMonthYear => $"{_currentDate:MMMM yyyy}";


        private ObservableCollection<NoteItem> _notes;
        public ObservableCollection<NoteItem> Notes { get; set; } = new ObservableCollection<NoteItem>();

        private ObservableCollection<ReminderItem> _reminders;
        public ObservableCollection<ReminderItem> Reminders { get; set; }

        private NoteItem _selectedNote;
        public NoteItem SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged();

                if (value != null)
                {
                    NewTaskContent = value.Content;
                    SelectedDate = value.Date;
                }
            }
        }

        private ReminderItem _selectedReminder;
        public ReminderItem SelectedReminder
        {
            get => _selectedReminder;
            set
            {
                _selectedReminder = value;
                OnPropertyChanged();

                if (value != null)
                {
                    // Автозаполнение полей редактирования
                    NewTaskContentRemind = value.Content;
                    SelectedDate = value.Date;
                    SelectedReminderTime = value.Time;
                }
            }
        }

        private void LoadNote(NoteItem note)
        {
            NewTaskContent = note.Content;
            SelectedDate = note.Date;
        }

        private void LoadReminder(ReminderItem reminder)
        {
            NewTaskContentRemind = reminder.Content;
            SelectedDate = reminder.Date;
            SelectedReminderTime = reminder.Time;
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

        /// Свойство для отслеживания активной вкладки
        private ViewType _currentView = ViewType.Notes;
        public ViewType CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        public ICommand SwitchViewCommand { get; }

        /// <summary>
        /// Метод для переключения вкладок
        /// </summary>
        /// <param name="viewType"></param>
        private void SwitchView(string viewType)
        {
            CurrentView = (ViewType)Enum.Parse(typeof(ViewType), viewType);
            IsEditing = false;

            // Если дата выбрана, обновляем данные для новой вкладки
            if (SelectedDate.HasValue)
            {
                bool hasNotes = Notes.Any(n => n.Date.Date == SelectedDate.Value.Date);
                bool hasReminders = Reminders.Any(r => r.Date.Date == SelectedDate.Value.Date);

                if (CurrentView == ViewType.Notes)
                {
                    LoadNoteForDate(SelectedDate.Value);
                    if (!hasNotes)
                    {
                        NewTaskContent = $"Создание задачи на {SelectedDate.Value:dd MMMM yyyy}\n";
                    }
                }
                else
                {
                    LoadReminderForDate(SelectedDate.Value);
                    if (!hasReminders)
                    {
                        NewTaskContentRemind = $"Создание напоминания на {SelectedDate.Value:dd MMMM yyyy}\n";
                    }
                }
            }

            OnPropertyChanged(nameof(NewTaskContent));
            OnPropertyChanged(nameof(NewTaskContentRemind));
            OnPropertyChanged(nameof(EditorButtonText));
            OnPropertyChanged(nameof(EditorTitle));
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
                    OnPropertyChanged();
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

        private bool _isEditorVisible;
        public bool IsEditorVisible
        {
            get => _isEditorVisible;
            set
            {
                _isEditorVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCalendarVisible));
            }
        }

        public bool IsCalendarVisible => !_isEditorVisible;

        public ICommand CreateNewNoteCommand { get; }
        public ICommand BackToCalendarCommand { get; }

        public ICommand SelectNoteCommand { get; }
        public ICommand SelectReminderCommand { get; }

        private void BackToCalendarCommandExecute()
        {
            IsEditorVisible = false;
            IsEditing = false;
            SelectedNote = null;
            SelectedReminder = null;
            NewTaskContent = string.Empty;
            NewTaskContentRemind = string.Empty;
            OnPropertyChanged(nameof(EditorButtonText));
            OnPropertyChanged(nameof(EditorTitle)); 
        }

        /// <summary>
        /// Команда для сохранения в зависимости от текущего вида
        /// </summary>
        public ICommand SaveCommand { get; }

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
        public ICommand ShowTaskPanelCommand { get; }
        public ICommand OpenTaskAreaCommand { get; }
        public ICommand SaveNoteCommand { get; private set; }
        public ICommand SaveReminderCommand { get; private set; }

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

            Days = new ObservableCollection<DayViewModel>();
            Notes = new ObservableCollection<NoteItem>();
            Reminders = new ObservableCollection<ReminderItem>();

            // 2. Инициализация команд после загрузки данных
            PreviousMonthCommand = new RelayCommand(ShowPreviousMonth);
            NextMonthCommand = new RelayCommand(ShowNextMonth);
            ShowTaskPanelCommand = new RelayCommand<DayViewModel>(ShowTaskPanel);
            OpenTaskAreaCommand = new RelayCommand<DayViewModel>(OpenTaskArea);

            // 3. Новые реализации команд
            SaveNoteCommand = new RelayCommand(SaveNote);
            SaveReminderCommand = new RelayCommand(SaveReminder);

            CreateNewNoteCommand = newCreateNewNoteCommand();
            BackToCalendarCommand = new RelayCommand(BackToCalendarCommandExecute);
            SwitchViewCommand = new RelayCommand<string>(SwitchView);
            SaveCommand = new RelayCommand(() =>
            {
                if (CurrentView == ViewType.Notes) SaveNote();
                else SaveReminder();
            });

            Hours = new ObservableCollection<int>(Enumerable.Range(0, 24));
            Minutes = new ObservableCollection<int>(Enumerable.Range(0, 60).Where(m => m % 5 == 0));

            SelectedHour = 8;
            SelectedMinute = 0;

            SelectNoteCommand = new RelayCommand<NoteItem>(note =>
            {
                SelectedNote = note;
                CurrentView = ViewType.Notes;
                IsEditing = true;
                OnPropertyChanged(nameof(EditorTitle)); // Уведомление об изменении
            });

            SelectReminderCommand = new RelayCommand<ReminderItem>(reminder =>
            {
                SelectedReminder = reminder;
                CurrentView = ViewType.Reminders;
                IsEditing = true;
                OnPropertyChanged(nameof(EditorTitle));
            });

            EnsureFoldersExist();
            LoadNotesAndReminders();


            UpdateCalendar();
            UpdateScaledProperties();
        }

        public RelayCommand newCreateNewNoteCommand()
        {
            return new RelayCommand(() =>
            {
                IsEditorVisible = true;
                IsEditing = false;
                SelectedNote = null;
                SelectedReminder = null;

                // Если дата не выбрана, используем текущую
                if (!SelectedDate.HasValue)
                {
                    SelectedDate = DateTime.Today;
                }

                // Устанавливаем текст по умолчанию
                if (CurrentView == ViewType.Notes)
                {
                    NewTaskContent = $"Создание задачи на {SelectedDate.Value:dd MMMM yyyy}\n";
                }
                else
                {
                    NewTaskContentRemind = $"Создание напоминания на {SelectedDate.Value:dd MMMM yyyy}\n";
                    SelectedHour = 8;
                    SelectedMinute = 0;
                }

                OnPropertyChanged(nameof(NewTaskContent));
                OnPropertyChanged(nameof(NewTaskContentRemind));
            });
        }

        private void UpdateScaledProperties()
        {
            ButtonWidth = Math.Max(50 * Scale, 30);
            ButtonHeight = Math.Max(50 * Scale, 20);
            MarginTop = new Thickness(10 * Scale, 10 * Scale, 10 * Scale, 10 * Scale);
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
            if (CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
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

                // Получаем все напоминания для этого дня
                var dayReminders = Reminders
                    .Where(r => r.Date.Date == date.Date)
                    .OrderBy(r => r.Time)
                    .ToList();

                // Находим ближайшее не прошедшее время
                var currentTime = DateTime.Now.TimeOfDay;
                var nearest = dayReminders
                    .FirstOrDefault(r => r.Time > currentTime)?.Time
                    ?? dayReminders.FirstOrDefault()?.Time;

                dayViewModel.NearestReminder = nearest?.ToString(@"hh\:mm") ?? "";
                dayViewModel.HasNote = dayReminders.Any() || Notes.Any(n => n.Date.Date == date.Date);
                OnPropertyChanged(nameof(Days));
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

        //private void CheckForNoteAndReminder(DayViewModel dayViewModel)
        //{
        //    if (!dayViewModel.Date.HasValue) return;

        //    // Убрать условие r.Time > DateTime.Now.TimeOfDay
        //    var reminders = Reminders
        //        .Where(r => r.Date.Date == dayViewModel.Date.Value.Date)
        //        .OrderBy(r => r.Time)
        //        .ToList();

        //    dayViewModel.HasNote = reminders.Any()
        //        || Notes.Any(n => n.Date.Date == dayViewModel.Date.Value.Date); // Учитываем заметки

        //    dayViewModel.ReminderTime = reminders.Any()
        //        ? reminders.First().Time.ToString(@"hh\:mm")
        //        : string.Empty;
        //}

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

        public string EditorTitle
        {
            get
            {
                if (IsEditing)
                {
                    return CurrentView == ViewType.Notes
                        ? "Редактирование заметки"
                        : "Редактирование напоминания";
                }
                else
                {
                    return CurrentView == ViewType.Notes
                        ? "Создать заметку"
                        : "Создать напоминание";
                }
            }
        }

        private void OpenTaskArea(DayViewModel selectedDay)
        {
            if (selectedDay?.Date == null) return;

            SelectedDate = selectedDay.Date; // Фиксируем выбранную дату
            IsTaskPanelVisible = true;
            IsEditorVisible = true;

            // Проверяем наличие записей для активной вкладки
            bool hasNotes = Notes.Any(n => n.Date.Date == SelectedDate.Value.Date);
            bool hasReminders = Reminders.Any(r => r.Date.Date == SelectedDate.Value.Date);

            IsEditing = (CurrentView == ViewType.Notes && hasNotes)
                      || (CurrentView == ViewType.Reminders && hasReminders);

            // Обновляем данные для текущей вкладки
            if (CurrentView == ViewType.Notes)
            {
                LoadNoteForDate(SelectedDate.Value);
                if (!IsEditing)
                {
                    NewTaskContent = $"Создание задачи на {SelectedDate.Value:dd MMMM yyyy}\n";
                }
            }
            else
            {
                LoadReminderForDate(SelectedDate.Value);
                if (!IsEditing)
                {
                    NewTaskContentRemind = $"Создание напоминания на {SelectedDate.Value:dd MMMM yyyy}\n";
                }
            }

            // Принудительное обновление UI
            OnPropertyChanged(nameof(NewTaskContent));
            OnPropertyChanged(nameof(NewTaskContentRemind));
            OnPropertyChanged(nameof(EditorTitle));
        }

        private void EnsureFoldersExist()
        {
            var basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes"
            );

            // Создаем все необходимые папки одной операцией
            Directory.CreateDirectory(Path.Combine(basePath, "Notes"));
            Directory.CreateDirectory(Path.Combine(basePath, "Reminders"));
        }

        private void LoadNoteForDate(DateTime date)
        {
            var note = Notes.FirstOrDefault(n => n.Date.Date == date.Date);
            if (note != null)
            {
                NewTaskContent = note.Content;
                IsEditing = true;
            }
            else
            {
                IsEditing = false;
            }
        }

        private void LoadReminderForDate(DateTime date)
        {
            try
            {
                // Ищем все файлы напоминаний для выбранной даты
                var reminders = Reminders
                    .Where(r => r.Date.Date == date.Date)
                    .OrderBy(r => r.Time)
                    .ToList();

                if (reminders.Any())
                {
                    // Берём первое напоминание (или реализуйте выбор через UI)
                    var reminder = reminders.First();
                    NewTaskContentRemind = reminder.Content;
                    SelectedHour = reminder.Time.Hours;
                    SelectedMinute = reminder.Time.Minutes;
                    IsEditing = true;
                }
                else
                {
                    NewTaskContentRemind = $"Создание напоминания на {date:dd MMMM yyyy}\n";
                    SelectedHour = 8;
                    SelectedMinute = 0;
                    IsEditing = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки напоминания: {ex.Message}");
                NewTaskContentRemind = string.Empty;
                SelectedHour = 8;
                SelectedMinute = 0;
            }
        }

        public string EditorButtonText => IsEditing ? "Сохранить изменения" : "Создать";

        private void SaveNote()
        {
            try
            {
                if (!ValidateNoteData()) return;

                if (!SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату!");
                    return;
                }

                if (IsEditing && SelectedNote != null)
                {
                    // Редактируем существующую заметку
                    SelectedNote.Content = NewTaskContent;
                    SaveToFile(SelectedNote);
                    MessageBox.Show("Изменения сохранены!");
                }
                else
                {
                    // Создаём новую заметку
                    var note = new NoteItem
                    {
                        Date = SelectedDate.Value,
                        Title = GetTitleFromContent(NewTaskContent),
                        Content = NewTaskContent
                    };
                    Notes.Add(note);
                    SaveToFile(note);
                    SelectedNote = note; // Привязываем новую заметку
                    IsEditing = true; // Устанавливаем флаг редактирования
                    MessageBox.Show("Новая заметка создана!");
                }

                UpdateCalendar();
                OnPropertyChanged(nameof(EditorButtonText)); // Обновляем кнопку
                OnPropertyChanged(nameof(EditorTitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Сброс флага при закрытии формы
        /// </summary>
        private void CancelEditing()
        {
            IsEditing = false;
            NewTaskContent = string.Empty;
            SelectedDate = null;
            OnPropertyChanged(nameof(EditorButtonText));
        }

        private bool ValidateNoteData()
        {
            if (!SelectedDate.HasValue)
            {
                MessageBox.Show("Не выбрана дата!");
                return false;
            }
            if (CurrentView == ViewType.Notes && string.IsNullOrWhiteSpace(NewTaskContent))
            {
                MessageBox.Show("Содержимое не может быть пустым!");
                return false;
            }
            return true;
        }

        private void UpdateExistingNote()
        {
            var oldNote = SelectedNote;
            var newNote = new NoteItem
            {
                Id = oldNote.Id,
                Date = SelectedDate.Value,
                Title = GetTitleFromContent(NewTaskContent),
                Content = NewTaskContent
            };

            // Заменяем элемент в коллекции
            int index = Notes.IndexOf(oldNote);
            Notes[index] = newNote;

            SaveToFile(newNote);
        }

        private void CreateNewNote()
        {
            if (!SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату!");
                return;
            }

            var note = new NoteItem
            {
                Date = SelectedDate.Value,
                Title = GetTitleFromContent(NewTaskContent),
                Content = NewTaskContent
            };

            Notes.Add(note);
            SaveToFile(note);

            // Уведомить UI об изменении коллекции
            OnPropertyChanged(nameof(Notes));
            UpdateCalendar(); // Обновить календарь
        }

        /// <summary>
        /// Метод сброса формы
        /// </summary>
        private void ResetForm()
        {
            SelectedNote = null;
            SelectedDate = null;
        }



        // Метод для сохранения напоминания
        private void SaveReminder()
        {
            try
            {
                if (!ValidateReminderData()) return;

                if (!SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату!");
                    return;
                }

                if (SelectedReminder != null)
                {
                    UpdateExistingReminder();
                }
                else
                {
                    var reminder = new ReminderItem
                    {
                        Date = SelectedDate.Value,
                        Title = GetTitleFromContent(NewTaskContentRemind),
                        Content = NewTaskContentRemind,
                        Time = SelectedReminderTime
                    };
                    Reminders.Add(reminder);
                    SaveToFile(reminder);
                    ScheduleReminderTask(reminder.Date + reminder.Time, reminder.Content);
                    SelectedReminder = reminder; // Привязываем новое напоминание
                    IsEditing = true; // Переводим в режим редактирования
                }

                UpdateCalendar();
                OnPropertyChanged(nameof(EditorButtonText));
                OnPropertyChanged(nameof(EditorTitle));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateReminderData()
        {
            if (!SelectedDate.HasValue)
            {
                MessageBox.Show("Не выбрана дата!");
                return false;
            }
            if (CurrentView == ViewType.Reminders && string.IsNullOrWhiteSpace(NewTaskContentRemind))
            {
                MessageBox.Show("Содержимое не может быть пустым!");
                return false;
            }
            return true;
        }

        private void UpdateExistingReminder()
        {
            SelectedReminder.Title = GetTitleFromContent(NewTaskContentRemind);
            SelectedReminder.Content = NewTaskContentRemind;
            SelectedReminder.Date = SelectedDate.Value;
            SelectedReminder.Time = SelectedReminderTime;
            SaveToFile(SelectedReminder);
        }

        private void CreateNewReminder()
        {
            var reminder = new ReminderItem
            {
                Date = SelectedDate.Value,
                Title = GetTitleFromContent(NewTaskContentRemind),
                Content = NewTaskContentRemind,
                Time = SelectedReminderTime
            };
            Reminders.Add(reminder);
            SaveToFile(reminder);
            ScheduleReminderTask(reminder.Date + reminder.Time, reminder.Content);
        }

        private void ResetReminderForm()
        {
            NewTaskContentRemind = string.Empty;
            SelectedReminder = null;
            SelectedDate = null;
        }



        private void LoadNotesAndReminders()
        {
            Notes.Clear();
            Reminders.Clear();

            string notesDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes",
                "Notes"
            );
            string remindersDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes",
                "Reminders"
            );

            // Загрузка заметок
            foreach (var file in Directory.GetFiles(notesDir, "note_*.md"))
            {
                NoteItem note = LoadNoteFromFile(file);
                if (note != null && !Notes.Any(n => n.Id == note.Id))
                {
                    Notes.Add(note);
                }
            }

            // Загрузка напоминаний
            foreach (var file in Directory.GetFiles(remindersDir, "reminder_*.md"))
            {
                ReminderItem reminder = LoadReminderFromFile(file);
                if (reminder != null && !Reminders.Any(r => r.Id == reminder.Id))
                {
                    Reminders.Add(reminder);
                }
            }

            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(Reminders));
            UpdateCalendar();
        }

        private NoteItem LoadNoteFromFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                return new NoteItem
                {
                    Id = ExtractGuidFromFileName(filePath),
                    Date = ExtractDateFromFileName(filePath),
                    Title = GetTitleFromContent(content),
                    Content = content
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки заметки: {ex.Message}");
                return null;
            }
        }

        private ReminderItem LoadReminderFromFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                TimeSpan time = ExtractTimeFromContent(content);
                return new ReminderItem
                {
                    Id = ExtractGuidFromFileName(filePath),
                    Date = ExtractDateFromFileName(filePath),
                    Title = GetTitleFromContent(content),
                    Content = content,
                    Time = time
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки напоминания: {ex.Message}");
                return null;
            }
        }



        

        private List<NoteItem> LoadNotesForDate(DateTime date)
        {
            var notes = new List<NoteItem>();
            var notesDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes",
                "Notes"
            );

            foreach (var file in Directory.GetFiles(notesDir, $"note_{date:yyyy-MM-dd}_*.md"))
            {
                var id = ExtractGuidFromFileName(file);
                var content = File.ReadAllText(file);
                notes.Add(new NoteItem
                {
                    Id = id,
                    Date = date,
                    Title = GetTitleFromContent(content),
                    Content = content
                });
            }

            return notes;
        }

        // Загрузка напоминаний
        private List<ReminderItem> LoadRemindersForDate(DateTime date)
        {
            var reminders = new List<ReminderItem>();
            var remindersDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes",
                "Reminders"
            );

            foreach (var file in Directory.GetFiles(remindersDir, $"reminder_{date:yyyy-MM-dd}_*.md"))
            {
                var content = File.ReadAllText(file);
                var time = ExtractTimeFromContent(content);
                if (time == TimeSpan.Zero)
                {
                    Debug.WriteLine($"Ошибка чтения времени из файла: {file}");
                    continue;
                }

                reminders.Add(new ReminderItem
                {
                    Id = ExtractGuidFromFileName(file),
                    Date = date,
                    Title = GetTitleFromContent(content),
                    Content = content,
                    Time = time
                });
            }

            return reminders;
        }

        // Метод для извлечения GUID из имени файла
        private Guid ExtractGuidFromFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var parts = fileName.Split('_');

            if (parts.Length >= 3 && Guid.TryParse(parts[2], out Guid guid))
            {
                return guid;
            }
            else
            {
                Debug.WriteLine($"Ошибка извлечения GUID: {fileName}");
                return Guid.NewGuid(); // Создаем новый GUID для избежания конфликтов
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

        private void SaveNoteForDate(DateTime date, NoteItem note)
        {
            string noteFilePath = GetNotesFilePath(date, false);
            File.WriteAllText(noteFilePath, note.Content);
        }

        //private void SaveReminderForDate(DateTime date, NoteItem reminder)
        //{
        //    try
        //    {
        //        // Получаем путь к файлу напоминания
        //        string reminderFilePath = GetNotesFilePath(date, true);

        //        // Формируем содержимое файла
        //        string reminderContent = $"{reminder.Content}\n\n**Время напоминания:** {reminder.ReminderTime:hh\\:mm}";

        //        // Сохраняем содержимое в файл
        //        File.WriteAllText(reminderFilePath, reminderContent);

        //        Debug.WriteLine($"Напоминание сохранено: {reminderFilePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Ошибка при сохранении напоминания: {ex.Message}");
        //        MessageBox.Show($"Ошибка при сохрааавнении напоминания: {ex.Message}");
        //    }
        //}

        private void SaveToFile(NoteItem item)
        {
            string content;
            string path;

            if (item is ReminderItem reminder)
            {
                content = $"{reminder.Content}\n**Время:** {reminder.Time:hh\\:mm}";
                path = GetReminderPath(reminder.Date, reminder.Id);
            }
            else
            {
                content = item.Content;
                path = GetNotePath(item.Date, item.Id);
            }

            File.WriteAllText(path, content, Encoding.UTF8);
        }

        private string GetNotePath(DateTime date, Guid id)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes",
                "Notes",
                $"note_{date:yyyy-MM-dd}_{id}.md"
            );
        }

        private string GetReminderPath(DateTime date, Guid id)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes",
                "Reminders",
                $"reminder_{date:yyyy-MM-dd}_{id}.md"
            );
        }

        private void LoadFromFiles()
        {
            var basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "UruruNotes"
            );

            LoadItems(
                Path.Combine(basePath, "Notes"),
                "note_*.md",
                (path, content) => new NoteItem
                {
                    Date = ExtractDateFromFileName(path),
                    Title = GetTitleFromContent(content),
                    Content = content
                },
                Notes
            );

            LoadItems(
                Path.Combine(basePath, "Reminders"),
                "reminder_*.md",
                (path, content) => new ReminderItem
                {
                    Date = ExtractDateFromFileName(path),
                    Title = GetTitleFromContent(content),
                    Content = content,
                    Time = ExtractTimeFromContent(content)
                },
                Reminders
            );
        }

        private void LoadItems<T>(string directory, string searchPattern,
    Func<string, string, T> itemFactory, ObservableCollection<T> collection)
    where T : class
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                collection.Clear();
                foreach (var file in Directory.EnumerateFiles(directory, searchPattern))
                {
                    var content = File.ReadAllText(file);
                    var item = itemFactory(file, content);
                    if (item != null) collection.Add(item);
                }
            });
        }

        private DateTime ExtractDateFromFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var parts = fileName.Split('_');

            // Старый формат: [тип]_[дата]
            if (parts.Length == 2)
            {
                return DateTime.ParseExact(parts[1], "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }

            // Новый формат: [тип]_[дата]_[GUID]
            if (parts.Length >= 3)
            {
                return DateTime.ParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            throw new FormatException($"Неизвестный формат файла: {fileName}");
        }

        private string GetTitleFromContent(string content)
        {
            return content.Split('\n').FirstOrDefault()?.TrimStart('#', ' ') ?? "Без названия";
        }

        private TimeSpan ExtractTimeFromContent(string content)
        {
            var timeMatch = Regex.Match(content, @"\*\*Время:\*\* (\d{2}:\d{2})");
            if (timeMatch.Success)
            {
                return TimeSpan.ParseExact(timeMatch.Groups[1].Value, "hh\\:mm", CultureInfo.InvariantCulture);
            }
            return TimeSpan.Zero;
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


    public class NoteItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _content;
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ReminderItem : NoteItem
    {
        private TimeSpan _time;

        public TimeSpan Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }
    }
}
