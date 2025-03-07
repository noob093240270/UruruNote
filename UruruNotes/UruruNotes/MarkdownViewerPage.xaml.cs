using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using UruruNotes.Models;

namespace UruruNotes.Views
{
    public partial class MarkdownViewerPage : Page
    {
        private readonly FileItem _file;

        public MarkdownViewerPage(FileItem file)
        {
            InitializeComponent();
            _file = file;

            // Загружаем содержимое файла в TextBox
            LoadMarkdownContent();
        }

        private void LoadMarkdownContent()
        {
            if (_file != null && File.Exists(_file.FilePath))
            {
                string content = File.ReadAllText(_file.FilePath, Encoding.UTF8);
                MarkdownTextBox.Text = content;
                MarkdownTextBox.TextWrapping = TextWrapping.Wrap;
            }
            else
            {
                MessageBox.Show("Файл не найден или не был инициализирован.");
            }
        }

        public void SaveMarkdownContent()
        {
            string content = MarkdownTextBox.Text;
            File.WriteAllText(_file.FilePath, content, Encoding.UTF8);
        }
    }
}
