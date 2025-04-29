﻿using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using UruruNotes.Models;
using Markdig;
using Markdown.Xaml;
using Markdig.Wpf;
using System.Windows.Controls;
using System.Windows.Documents;
using Aspose.Html;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Threading;

using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;


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

        public string MarkdownText { get; set; }

        private bool _isLoading = false;

        // Срабатывает при изменении текста в RichTextBox
        // Срабатывает при изменении текста в RichTextBox
        // Срабатывает при изменении текста в RichTextBox
        // Срабатывает при изменении текста в RichTextBox
        private string currentFilePath;

        private DispatcherTimer _previewTimer;
        private bool _webView2Initialized = false;

        private async void PreviewTimer_Tick(object sender, EventArgs e)
        {
            if (_previewTimer != null) // Добавляем проверку на null
            {
                _previewTimer.Stop();
            }
            await UpdatePreviewAsync();
        }

        private void LoadTestHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTestHtml();
        }

        private void LoadTestHtml()
        {
            // Создаем простой HTML текст
            string testHtml = @"
                <html>
                <head>
                    <style>
                        body {
                            font-family: sans-serif;
                            font-size: 16px;
                            line-height: 1.6;
                            padding: 20px;
                        }
                        h1 {
                            font-size: 24px;
                            font-weight: bold;
                        }
                    </style>
                </head>
                <body>
                    <h1>Это тестовый заголовок</h1>
                    <p>Это тестовый абзац. Здесь может быть любой текст.</p>
                </body>
                </html>
            ";

            // Загружаем HTML текст в WebView2
            MarkdownPreview.NavigateToString(testHtml);
        }

        private async Task UpdatePreviewAsync()
        {
            var markdownText = new TextRange(MarkdownRichTextBox.Document.ContentStart, MarkdownRichTextBox.Document.ContentEnd).Text;

            if (!string.IsNullOrEmpty(markdownText))
            {
                try
                {
                    string htmlContent = ConvertMarkdownToHtml(markdownText); // Первая переменная

                    if (_webView2Initialized && MarkdownPreview != null)
                    {
                        htmlContent = ConvertMarkdownToHtml(markdownText); // Используем существующую переменную
                        Debug.WriteLine($"HTML Content: {htmlContent}");
                        await MarkdownPreview.Dispatcher.InvokeAsync(() => MarkdownPreview.NavigateToString(htmlContent));
                    }
                }
                catch (Exception ex)
                {
                    // Обработка ошибок
                    Debug.WriteLine($"Ошибка обновления предпросмотра: {ex.Message}");
                }
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалог выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Markdown Files (*.md)|*.md|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Загружаем файл в режим редактирования
                LoadMarkdownFile(filePath);
            }
        }

        private void LoadMarkdownFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    currentFilePath = filePath;
                    string markdownText = File.ReadAllText(filePath);
                    MarkdownRichTextBox.Document.Blocks.Clear();
                    MarkdownRichTextBox.AppendText(markdownText);
                    string htmlContent = ConvertMarkdownToHtml(markdownText);
                    if (MarkdownPreview != null)
                    {
                        MarkdownPreview.NavigateToString(htmlContent);
                    }
                }
                else
                {
                    MessageBox.Show("Файл не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}");
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex == 1) // Вкладка "Предпросмотр"
            {
                Debug.WriteLine("Переключились на вкладку 'Предпросмотр'");
                UpdatePreview(); // Обновляем предпросмотр
            }
        }

        private void MarkdownPreview_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                UpdatePreview(); // Обновляем предпросмотр после инициализации
            }
            else
            {
                // Обработка ошибок инициализации
                MessageBox.Show($"Ошибка инициализации WebView2: {e.InitializationException.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdatePreview()
        {
            try
            {
                // Получаем текст из RichTextBox
                TextRange textRange = new TextRange(MarkdownRichTextBox.Document.ContentStart, MarkdownRichTextBox.Document.ContentEnd);
                string markdownText = textRange.Text;

                // Преобразуем Markdown в HTML
                string htmlContent = ConvertMarkdownToHtml(markdownText);

                // Обновляем WebView2 в потоке пользовательского интерфейса
                await MarkdownPreview.Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        // Проверяем, инициализирован ли CoreWebView2
                        if (MarkdownPreview.CoreWebView2 == null)
                        {
                            Debug.WriteLine("CoreWebView2 не инициализирован, инициализируем");
                            await MarkdownPreview.EnsureCoreWebView2Async();
                        }

                        Debug.WriteLine("Загружаем HTML в WebView2");
                        MarkdownPreview.NavigateToString(htmlContent);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка при загрузке HTML в WebView2: {ex.Message}");
                        MessageBox.Show($"Ошибка при загрузке HTML в WebView2: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });

                Debug.WriteLine("Предпросмотр обновлен");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при обновлении предпросмотра: {ex.Message}");
                MessageBox.Show($"Ошибка при обновлении предпросмотра: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для преобразования Markdown в HTML
        private string ConvertMarkdownToHtml(string markdownText)
        {
            try
            {
                var htmlContent = Markdig.Markdown.ToHtml(markdownText);
                htmlContent = $"<head><meta charset=\"UTF-8\"></head>{htmlContent}"; // Добавляем метатег кодировки
                return htmlContent;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка преобразования Markdown: {ex.Message}");
                return string.Empty;
            }
        }




        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownViewer markdownViewer && e.NewValue is double fontSize)
            {
                markdownViewer.UpdateFontSize(fontSize);
            }
        }


        private async Task InitializeWebView2Async()
        {
            try
            {
                await MarkdownPreview.EnsureCoreWebView2Async();
                _webView2Initialized = true;
            }
            catch (Exception ex)
            {
                // Обработка ошибок инициализации WebView2
                MessageBox.Show($"Ошибка инициализации WebView2: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public MarkdownViewer(FileItem file = null)
        {
            InitializeComponent();
            MarkdownPreview.CoreWebView2InitializationCompleted += MarkdownPreview_CoreWebView2InitializationCompleted;

            InitializeWebView2Async();
            _previewTimer = new DispatcherTimer();
            _previewTimer.Interval = TimeSpan.FromMilliseconds(500); // 500 мс задержка
            _previewTimer.Tick += PreviewTimer_Tick;

            if (file != null)
            {
                _file = file;
                LoadFileContent(file.FilePath);
<<<<<<< Updated upstream
            }
            if (Application.Current.Resources.Contains("GlobalFontSize"))
            {
                double globalFontSize = (double)Application.Current.Resources["GlobalFontSize"];
                UpdateFontSize(globalFontSize);
            }

            // Загружаем файл, если он передан
            if (file != null)
            {
                LoadFileContent(file.FilePath);
            }

            // Подписываемся на событие KeyDown
=======

            }

>>>>>>> Stashed changes
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
            string filePath = _file.FilePath;
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                string textToSave = "";
                foreach (var block in MarkdownRichTextBox.Document.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        TextRange textRange = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
                        textToSave += textRange.Text + "\r\n\r\n"; // Добавляем двойные переносы строк
                    }
                }
                File.WriteAllText(filePath, textToSave, Encoding.UTF8);
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
<<<<<<< Updated upstream

                // Очищаем RichTextBox перед загрузкой нового текста
=======
                MarkdownText = fileContent;
>>>>>>> Stashed changes
                MarkdownRichTextBox.Document.Blocks.Clear();

                // Загружаем текст в RichTextBox
                TextRange range = new TextRange(MarkdownRichTextBox.Document.ContentStart, MarkdownRichTextBox.Document.ContentEnd);
                range.Text = fileContent;

                // Применяем размер шрифта к содержимому
                if (MarkdownRichTextBox.Document is FlowDocument flowDocument)
                {
<<<<<<< Updated upstream
                    foreach (Block block in flowDocument.Blocks)
=======
                    string singleLinePara = para.Replace("\r\n", Environment.NewLine).Replace("\n", Environment.NewLine).Trim();
                    var paragraph = new Paragraph(new Run(singleLinePara))
>>>>>>> Stashed changes
                    {
                        if (block is Paragraph paragraph)
                        { 
                            paragraph.FontSize = MarkdownRichTextBox.FontSize;
                        }
                    }
                }
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
                for (int i = flowDocument.Blocks.Count - 1; i >= 0; i--)
                {
                    if (flowDocument.Blocks.ElementAt(i) is Paragraph paragraph)
                    {
                        paragraph.FontSize = fontSize;
                    }
                }
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

        private void MarkdownRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_previewTimer != null) // Добавляем проверку на null
            {
                _previewTimer.Stop();
                _previewTimer.Start();
            }

        }

        private double _fontSize;

    }
}
