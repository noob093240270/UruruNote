using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;

namespace UruruNotes.Views
{
    /// <summary>
    /// Логика взаимодействия для ReminderDetailsWindow.xaml
    /// </summary>
    public partial class ReminderDetailsWindow : Window
    {
        private DateTime _reminderDate;

        public string ReminderDate => _reminderDate.ToString("dd MMMM yyyy"); // Привязка для заголовка

        public ReminderDetailsWindow(DateTime date)
        {
            InitializeComponent();
            _reminderDate = date;

            // Устанавливаем DataContext для привязки
            DataContext = this;

            // Загрузи данные напоминания
            LoadReminderDetails(date);
        }

        private void LoadReminderDetails(DateTime date)
        {
            try
            {
                string reminderFilePath = GetNotesFilePath(date, true);
                if (File.Exists(reminderFilePath))
                {
                    string reminderContent = File.ReadAllText(reminderFilePath);
                    ReminderTextBlock.Document.Blocks.Clear();
                    ReminderTextBlock.Document.Blocks.Add(new Paragraph(new Run(reminderContent)));
                }
                else
                {
                    ReminderTextBlock.Document.Blocks.Clear();
                    ReminderTextBlock.Document.Blocks.Add(new Paragraph(new Run("Напоминание не найдено.")));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке напоминания: {ex.Message}");
            }
        }

        private void DeleteReminder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string reminderFilePath = GetNotesFilePath(_reminderDate, true);
                if (File.Exists(reminderFilePath))
                {
                    File.Delete(reminderFilePath);
                    MessageBox.Show("Напоминание удалено.");
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении напоминания: {ex.Message}");
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            // Удаляем задачу из планировщика задач
            DeleteScheduledTask(_reminderDate);

            Close();
        }

        private void DeleteScheduledTask(DateTime date)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    // Формируем имя задачи
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

        private string GetNotesFilePath(DateTime date, bool isReminder)
        {
            string baseFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UruruNotes");
            string subFolder = isReminder ? "Reminders" : "Notes";
            string fileName = $"{(isReminder ? "reminder" : "note")}_{date:dd-MM-yyyy}.md";
            return System.IO.Path.Combine(baseFolder, subFolder, fileName);
        }
    }
}
