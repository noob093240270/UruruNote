using System;
using System.Collections.ObjectModel;
using System.Windows;
using UruruNote.Models;
using UruruNotes.Models;

namespace UruruNotes.Views
{
    public partial class NewFolderWindow : Window
    {
        // Событие, которое передает название папки
        public event EventHandler<string> FolderCreated;

        public NewFolderWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем название папки из TextBox
            string folderName = FolderNameTextBox.Text.Trim();

            // Проверяем, что название не пустое
            if (!string.IsNullOrEmpty(folderName))
            {
                // Вызываем событие и передаем название папки
                FolderCreated?.Invoke(this, folderName);

                // Закрываем окно
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите название папки.");
            }
        }
    }
}
