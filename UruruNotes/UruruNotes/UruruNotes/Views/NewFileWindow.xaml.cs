using System;
using System.IO;
using System.Windows;
using UruruNote.Models;

namespace UruruNotes.Views
{


    public partial class NewFileWindow : Window
    {
        public string FileName => FileNameTextBox.Text;

        // Событие для уведомления об успешном создании файла
        public event Action<string> FileCreated;


        public NewFileWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                MessageBox.Show("Пожалуйста, введите имя файла.");
                return;
            }

            var markdownService = new MarkdownFileService();
            string newFileName = FileName.Trim() + ".md";

            // Формируем полный путь с учетом папки MyFolders
            string filePath = Path.Combine(markdownService.RootDirectory, newFileName);

            if (markdownService.IsMarkdownFileExists(filePath))
            {
                MessageBox.Show("Файл с таким именем уже существует.");
                return;
            }

            try
            {
                // Создаем файл в папке MyFolders
                markdownService.CreateMarkdownFile(filePath);

                // Вызываем событие для уведомления о создании файла
                FileCreated?.Invoke(filePath);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}

