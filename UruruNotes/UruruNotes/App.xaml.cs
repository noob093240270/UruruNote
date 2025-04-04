﻿using System.Configuration;
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

    }
}


