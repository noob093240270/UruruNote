using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using UruruNote.ViewsModels;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32.TaskScheduler;
using System;
using UruruNotes;
using UruruNotes.Views;
using System.Diagnostics;

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
        /// Событие запуска приложения
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
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
        }

        private void OpenReminderDetails(DateTime date)
        {
            // Открываем окно с деталями напоминания как модальное
            var reminderWindow = new ReminderDetailsWindow(date);
            reminderWindow.ShowDialog(); // Используем ShowDialog() вместо Show()
        }

        private void ShowToastNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddArgument("action", "openReminder") // Добавляем аргумент для обработки
                .Show();
        }

    }
}


