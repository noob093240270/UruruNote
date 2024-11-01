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

                MarkdownTextBox.Text = File.ReadAllText(filePath);

        }
    }


}
