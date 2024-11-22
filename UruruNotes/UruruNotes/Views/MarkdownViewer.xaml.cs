using System.IO;
using System.Windows;

namespace UruruNote.Views
{
    public partial class MarkdownViewer : Window
    {
        public string FilePath { get; }

        public MarkdownViewer(string filePath)
        {
            InitializeComponent();
            FilePath = filePath; // Сохраняем путь к файлу
            LoadFileContent(filePath);
        }

        private void LoadFileContent(string filePath)
        {
            try
            {
                // Читаем содержимое файла и отображаем его в TextBox
                MarkdownTextBox.Text = File.ReadAllText(filePath); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
            }
        }
    }
}
