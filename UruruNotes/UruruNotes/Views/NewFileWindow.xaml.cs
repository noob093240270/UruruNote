using System;
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

            if (markdownService.IsMarkdownFileExists(newFileName))
            {
                MessageBox.Show("Файл с таким именем уже существует.");
                return;
            }

            try
            {
                string filePath = markdownService.CreateMarkdownFile(newFileName);

                // Вызываем событие для уведомления о создании файла
                FileCreated?.Invoke(filePath);

                MessageBox.Show("Файл успешно создан: " + filePath);
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