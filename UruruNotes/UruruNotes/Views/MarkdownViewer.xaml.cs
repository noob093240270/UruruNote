using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using UruruNotes.Models;

namespace UruruNote.Views
{
    public partial class MarkdownViewer : UserControl
    {

        private FileItem _file;
        // Зависимое свойство для FontSize
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(
                nameof(FontSize),
                typeof(double),
                typeof(MarkdownViewer),
                new PropertyMetadata(15.0, OnFontSizeChanged));

        // Обычное свойство для доступа к зависимому свойству
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownViewer markdownViewer && e.NewValue is double fontSize)
            {
                markdownViewer.UpdateFontSize(fontSize);
            }
        }
        public void ApplyScale(double scale)
        {
            if (scale > 0)
            {
                this.LayoutTransform = new ScaleTransform(scale, scale);
            }
        }


        public MarkdownViewer(FileItem file = null)
        {
            InitializeComponent();
            if (file != null)
            {
                _file = file;
                LoadFileContent(file.FilePath);
            }
            if (Application.Current.Resources.Contains("GlobalFontSize"))
            {
                double globalFontSize = (double)Application.Current.Resources["GlobalFontSize"];
                UpdateFontSize(globalFontSize);
            }
            if (Application.Current.Resources.Contains("GlobalScale"))
            {
                double globalScale = (double)Application.Current.Resources["GlobalScale"];
                ApplyScale(globalScale);
            }

            // Устанавливаем начальные настройки FlowDocument
            MarkdownRichTextBox.Document.PageWidth = double.NaN;
            MarkdownRichTextBox.Document.PagePadding = new Thickness(0);

            this.KeyDown += MarkdownViewer_KeyDown;
        }

        private void MarkdownViewer_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата комбинация клавиш Ctrl + S
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Вызываем метод сохранения
                SaveFile();
                e.Handled = true; // Останавливаем дальнейшую обработку события
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            string filePath = _file.FilePath;  // Путь к файлу
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                // Получаем текст из RichTextBox
                TextRange range = new TextRange(MarkdownRichTextBox.Document.ContentStart, MarkdownRichTextBox.Document.ContentEnd);

                // Сохраняем текст в файл
                File.WriteAllText(filePath, range.Text, Encoding.UTF8);

                MessageBox.Show("Файл успешно сохранен.");
            }
            else
            {
                MessageBox.Show("Ошибка: файл не найден.");
            }
        }

        private void LoadFileContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                MarkdownRichTextBox.Document.Blocks.Clear();
                var paragraphs = fileContent.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var para in paragraphs)
                {
                    string singleLinePara = para.Replace("\r\n", " ").Replace("\n", " ").Trim();
                    var paragraph = new Paragraph(new Run(singleLinePara))
                    {
                        FontSize = MarkdownRichTextBox.FontSize,
                        TextAlignment = TextAlignment.Left
                    };
                    MarkdownRichTextBox.Document.Blocks.Add(paragraph);
                }
                MarkdownRichTextBox.Document.PageWidth = double.NaN;
                MarkdownRichTextBox.Document.PagePadding = new Thickness(0);
                Debug.WriteLine($"RichTextBox Width: {MarkdownRichTextBox.ActualWidth}");
            }
        }

        public void UpdateFontSize(double fontSize)
        {
            // Обновляем локальный ресурс
            if (this.Resources.Contains("NoteFontSize"))
            {
                this.Resources["NoteFontSize"] = fontSize;
            }
            else
            {
                this.Resources.Add("NoteFontSize", fontSize); // Добавляем новый ресурс
            }

            // Обновляем размер шрифта для RichTextBox
            MarkdownRichTextBox.FontSize = fontSize;

            // Обновляем размер шрифта для всего содержимого FlowDocument
            if (MarkdownRichTextBox.Document is FlowDocument flowDocument)
            {
                foreach (Block block in flowDocument.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        paragraph.FontSize = fontSize;
                    }
                }
            }
        }
        public void UpdateScale(double scale)
        {
            Scale = scale; 
        }
        // Зависимое свойство для Scale
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                nameof(Scale),
                typeof(double),
                typeof(MarkdownViewer),
                new PropertyMetadata(1.0, OnScaleChanged)); // По умолчанию масштаб 1.0 (100%)

        // Обычное свойство для доступа к зависимому свойству
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        // Метод вызывается при изменении значения Scale
        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownViewer markdownViewer && e.NewValue is double scale)
            {
                markdownViewer.ApplyScale(scale);
            }
        }
        // Обработчик для "Жирного" текста
        private void BoldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("**");
        }

        // Обработчик для "Курсивного" текста
        private void ItalicMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("_");
        }

        // Обработчик для "Зачеркнутого" текста
        private void StrikethroughMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("~~");
        }

        // Обработчик для "Выделенного" текста
        private void HighlightMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("==");
        }

        // Метод для применения стиля к выделенному тексту
        private void ApplyTextStyleToSelection(string markdownSyntax)
        {
            // Получаем выделенный текст
            TextSelection selection = MarkdownRichTextBox.Selection;

            if (!selection.IsEmpty)
            {
                // Применяем Markdown синтаксис
                string selectedText = selection.Text;
                string formattedText = markdownSyntax + selectedText + markdownSyntax;

                // Заменяем выделенный текст
                TextRange range = new TextRange(selection.Start, selection.End);
                range.Text = formattedText;
            }
        }

        // Обработчик для преобразования клавиши Tab в абзац (4 пробела)
        private void MarkdownRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                // Получаем текущую позицию курсора в RichTextBox
                TextSelection selection = MarkdownRichTextBox.Selection;

                if (selection.IsEmpty)
                {
                    // Если ничего не выделено, вставляем 4 пробела на месте курсора
                    var caretPosition = MarkdownRichTextBox.CaretPosition;

                    // Вставляем 4 пробела в текущее положение курсора
                    caretPosition.InsertTextInRun("    ");
                }
                else
                {
                    // Если текст выделен, добавляем 4 пробела в начало выделенного текста
                    selection.Text = "    " + selection.Text;
                }

                // Останавливаем обработку клавиши Tab
                e.Handled = true;
            }
        }
        private double _fontSize;

    }
}
