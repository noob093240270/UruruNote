using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using UruruNote.ViewsModels;
using Microsoft.Toolkit.Uwp.Notifications;
//using Microsoft.Win32.TaskScheduler;
using System;
using UruruNotes;
using UruruNotes.Views;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;



namespace UruruNotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Метод для обновления глобального размера шрифта
        /// </summary>
        /// <param name="fontSize">Размер шрифта</param>
        public static event EventHandler FontSizeChanged;
        public static void UpdateGlobalFontSize(double fontSize)
        {
            if (Application.Current.Resources.Contains("GlobalFontSize"))
            {
                Application.Current.Resources["GlobalFontSize"] = fontSize;
            }
            else
            {
                Application.Current.Resources.Add("GlobalFontSize", fontSize);
            }
        }

        /// <summary>
        /// Метод для обновления глобального масштаба
        /// </summary>
        /// <param name="scale"></param>
        public static void UpdateGlobalScale(double scale)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var scaleTransform = new ScaleTransform(scale, scale);
                window.LayoutTransform = scaleTransform;
            }
        }
        /// <summary>
        /// Метод для применения темы
        /// </summary>
        /// <param name="isDarkMode">Флаг тёмной темы</param>
        public static void ApplyTheme(bool isDarkMode)
        {
            try
            {
                // Создаём новый словарь для темы
                var themeDictionary = new ResourceDictionary
                {
                    Source = new Uri($"/Themes/{(isDarkMode ? "DarkTheme.xaml" : "LightTheme.xaml")}", UriKind.Relative)
                };

                // Удаляем текущую тему из ресурсов приложения
                var currentTheme = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null &&
                        (d.Source.OriginalString == "/Themes/LightTheme.xaml" ||
                         d.Source.OriginalString == "/Themes/DarkTheme.xaml"));
                if (currentTheme != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(currentTheme);
                }

                // Создаём словарь для глобальных ресурсов
                var globalResources = new ResourceDictionary();
                globalResources.Add("GlobalFont", new FontFamily("Segoe UI"));
                globalResources.Add("GlobalFontSize", 15.0);
                globalResources.Add("NoteFontSize", 15.0);
                globalResources.Add("GlobalScale", 1.0);

                // Стиль для иконок (MaterialDesignThemes.Wpf.PackIcon)
                var iconStyle = new Style(typeof(MaterialDesignThemes.Wpf.PackIcon))
                {
                    Setters =
                    {
                        new Setter(MaterialDesignThemes.Wpf.PackIcon.ForegroundProperty, new DynamicResourceExtension("IconColor"))
                    }
                };
                globalResources.Add(typeof(MaterialDesignThemes.Wpf.PackIcon), iconStyle); // Исправлено: используем typeof вместо StyleKey

                // Очищаем текущие ресурсы приложения и добавляем новые
                Application.Current.Resources.MergedDictionaries.Clear();
                Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
                Application.Current.Resources.MergedDictionaries.Add(globalResources);

                // Обновляем ресурсы для всех открытых окон
                foreach (Window window in Application.Current.Windows)
                {
                    window.Resources.MergedDictionaries.Clear();
                    window.Resources.MergedDictionaries.Add(themeDictionary);
                    window.Resources.MergedDictionaries.Add(globalResources);
                }

                CurrentTheme = isDarkMode;
                Debug.WriteLine($"Применена тема: {(isDarkMode ? "Тёмная" : "Светлая")}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при применении темы: {ex.Message}");
                Debug.WriteLine($"Стек вызовов: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// Событие запуска приложения
        /// </summary>
        /// <summary>
        /// Событие запуска приложения
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                // Логирование аргументов
                Debug.WriteLine($"Аргументы командной строки: {string.Join(", ", e.Args)}");

                // Обработка аргументов командной строки
                if (e.Args.Length > 0)
                {
                    // Если аргумент содержит "ToastActivated", это нажатие на уведомление
                    if (e.Args[0] == "-ToastActivated")
                    {
                        Debug.WriteLine("Нажатие на уведомление: открываем окно с деталями напоминания");

                        // Открываем окно с деталями напоминания
                        OpenReminderDetails(DateTime.Now); // Здесь можно передать конкретную дату, если она есть в аргументах
                        return; // Не завершаем приложение
                    }

                    // Если это просто уведомление, показываем его
                    string message = e.Args[0];
                    Debug.WriteLine($"Показ уведомления: {message}");
                    ShowToastNotification("Напоминание", message);

                    // Не завершаем приложение, если это не нажатие на уведомление
                    return;
                }

                // Загрузка сохраненных настроек
                var settings = SettingsManager.LoadSettings();
                UpdateGlobalFontSize(settings.FontSize);
                UpdateGlobalScale(settings.Scale);
                ApplyTheme(settings.DarkMode);

                // Создаём и показываем главное окно
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Критическая ошибка при запуске приложения: {ex.Message}");
                Debug.WriteLine($"Стек вызовов: {ex.StackTrace}");
                MessageBox.Show($"Произошла ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        public static bool IsReminderWindowOpen { get; set; } = false;

        private void OpenReminderDetails(DateTime date)
        {
            // Удаляем задачу из планировщика задач
            DeleteScheduledTask(date);

            // Устанавливаем флаг, что окно открыто
            IsReminderWindowOpen = true;

            // Открываем окно
            var reminderWindow = new ReminderDetailsWindow(date);
            reminderWindow.Closed += (s, e) => IsReminderWindowOpen = false; // Сбрасываем флаг при закрытии окна
            reminderWindow.ShowDialog();
        }

        private void DeleteScheduledTask(DateTime date)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    // Формируем имя задачи (исправлено имя и формат даты)
                    string taskName = $"UruruNotesReminder_{date:yyyyMMddHHmm}";

                    // Пытаемся найти задачу и удалить её
                    Microsoft.Win32.TaskScheduler.Task task = ts.GetTask(taskName);
                    if (task != null)
                    {
                        ts.RootFolder.DeleteTask(taskName);
                        Debug.WriteLine($"Задача удалена: {taskName}");
                    }
                    else
                    {
                        Debug.WriteLine($"Задача не найдена: {taskName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при удалении задачи: {ex.Message}");
            }
        }

        private void ShowToastNotification(string title, string message)
        {
            // Проверяем, открыто ли окно "Детали напоминания"
            if (!IsReminderWindowOpen)
            {
                // Показываем уведомление
                new ToastContentBuilder()
                    .AddText(title)
                    .AddText(message)
                    .AddArgument("action", "openReminder")
                    .Show();
            }
            else
            {
                Debug.WriteLine("Окно 'Детали напоминания' уже открыто, уведомление не показывается.");
            }
        }
        public static bool CurrentTheme { get; set; } = false;
    }
}


