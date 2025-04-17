using Markdig;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UruruNotes;
using UruruNotes.Models;
using UruruNotes.Views;


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
        public new double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public string MarkdownText { get; set; }

        private bool _isLoading = false;

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
            try
            {
                var markdownText = new TextRange(
                    MarkdownRichTextBox.Document.ContentStart,
                    MarkdownRichTextBox.Document.ContentEnd
                ).Text;

                if (string.IsNullOrEmpty(markdownText)) return;

                // 1. Получаем обновленный HTML с динамическими стилями
                string htmlContent = ConvertMarkdownToHtml(markdownText);
                Debug.WriteLine($"HTML Content: {htmlContent}");

                // 2. Проверяем инициализацию WebView2
                if (!_webView2Initialized || MarkdownPreview == null) return;

                // 3. Принудительное обновление предпросмотра
                await MarkdownPreview.Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        // Гарантируем инициализацию
                        await MarkdownPreview.EnsureCoreWebView2Async();

                        // 4. Используем прямое вставление HTML
                        MarkdownPreview.NavigateToString(htmlContent);

                        // 5. Альтернативный вариант с полной перезагрузкой (раскомментировать при проблемах)
                        // MarkdownPreview.Reload();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WebView2 Error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Preview Update Error: {ex.Message}");
            }
        }

        // Вспомогательный метод для экранирования HTML
        private string ToJson(string str) =>
            Newtonsoft.Json.JsonConvert.SerializeObject(str);

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
                // Получаем текущий размер шрифта из глобальных настроек
                double fontSize = (double)Application.Current.Resources["GlobalFontSize"];

                // Создаем pipeline с расширениями
                var pipeline = new Markdig.MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseEmphasisExtras()
                    .Build();

                // Генерируем HTML с динамическими стилями
                return $@"<!DOCTYPE html>
        <html>
        <head>
            <meta charset=""UTF-8"">
            <style>
                body {{
                    font-family: 'Segoe UI', sans-serif;
                    font-size: {fontSize}px;
                    line-height: 1.6;
                    padding: 20px;
                    margin: 0;
                }}
                h1 {{ font-size: {fontSize * 1.5}px; }}
                h2 {{ font-size: {fontSize * 1.3}px; }}
                h3 {{ font-size: {fontSize * 1.1}px; }}
                code {{
                    font-family: Consolas, monospace;
                    font-size: {fontSize * 0.9}px;
                    background-color: #f0f0f0;
                    padding: 2px 4px;
                    border-radius: 3px;
                }}
                pre {{
                    background-color: #f8f8f8;
                    padding: 15px;
                    border-radius: 5px;
                    overflow-x: auto;
                }}
                mark {{
                    background-color: yellow;
                    padding: 0 2px;
                }}
            </style>
        </head>
        <body>
            {Markdig.Markdown.ToHtml(markdownText, pipeline)}
        </body>
        </html>";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка преобразования Markdown: {ex.Message}");
                return @"<html>
            <body>
                <p style='color: red;'>Ошибка преобразования Markdown</p>
            </body>
        </html>";
            }
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
            DisableDefaultShortcuts();

            // Подписка на события WebView2
            MarkdownPreview.CoreWebView2InitializationCompleted += MarkdownPreview_CoreWebView2InitializationCompleted;

            // Инициализация WebView2
            InitializeWebView2Async();

            // Таймер для отложенного обновления предпросмотра
            _previewTimer = new DispatcherTimer();
            _previewTimer.Interval = TimeSpan.FromMilliseconds(500);
            _previewTimer.Tick += PreviewTimer_Tick;

            // Загрузка файла
            if (file != null)
            {
                _file = file;
                LoadFileContent(file.FilePath);
            }

            // Синхронизация с глобальными настройками
            if (Application.Current.Resources.Contains("GlobalFontSize"))
            {
                UpdateFontSize((double)Application.Current.Resources["GlobalFontSize"]);
            }
            if (Application.Current.Resources.Contains("GlobalScale"))
            {
                ApplyScale((double)Application.Current.Resources["GlobalScale"]);
            }

            // Настройки документа
            MarkdownRichTextBox.Document.PageWidth = double.NaN;
            MarkdownRichTextBox.Document.PagePadding = new Thickness(0);

            // Обработчики событий
            KeyDown += MarkdownViewer_KeyDown;

            // Подписка на глобальные изменения шрифта
            App.FontSizeChanged += OnGlobalFontSizeChanged;

            Unloaded += (s, e) =>
            {
                // 1. Останавливаем таймер
                _previewTimer.Stop();

                // 2. Отписываемся от глобальных событий
                App.FontSizeChanged -= OnGlobalFontSizeChanged;
            };

            // Новый обработчик видимости
            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                {
                    // Обновляем предпросмотр при повторном открытии
                    _ = UpdatePreviewAsync();

                    // Синхронизируем настройки
                    UpdateFontSize((double)Application.Current.Resources["GlobalFontSize"]);
                    ApplyScale((double)Application.Current.Resources["GlobalScale"]);
                }
            };
        }

        private void OnGlobalFontSizeChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // 1. Обновляем размер шрифта редактора
                    double newSize = (double)Application.Current.Resources["GlobalFontSize"];
                    UpdateFontSize(newSize);

                    // 2. Принудительно обновляем предпросмотр
                    _previewTimer.Stop();
                    _previewTimer.Start(); // Запускаем таймер для немедленного обновления

                    // 3. Дополнительная синхронизация для WebView2
                    if (MarkdownPreview.CoreWebView2 != null)
                    {
                        _ = MarkdownPreview.ExecuteScriptAsync("document.location.reload()");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при обновлении шрифта: {ex.Message}");
                }
            });
        }

        protected void OnUnloaded(object sender, RoutedEventArgs e)
        {
            App.FontSizeChanged -= OnGlobalFontSizeChanged;
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
                StringBuilder textToSave = new StringBuilder();
                foreach (var block in MarkdownRichTextBox.Document.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        TextRange textRange = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
                        string paragraphText = textRange.Text.TrimEnd(); // Убираем лишние пробелы в конце строки

                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            textToSave.AppendLine(paragraphText);
                            textToSave.AppendLine(); // Оставляем ОДИН пустой ряд между абзацами
                        }
                    }
                }
                File.WriteAllText(filePath, textToSave.ToString().TrimEnd(), Encoding.UTF8); // Убираем лишние пробелы и пустые строки в конце
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
                MarkdownText = fileContent;
                MarkdownRichTextBox.Document.Blocks.Clear();

                var paragraphs = fileContent.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None);
                foreach (var para in paragraphs)
                {
                    if (!string.IsNullOrWhiteSpace(para)) // Пропускаем пустые строки
                    {
                        var paragraph = new Paragraph(new Run(para.Trim()))
                        {
                            FontSize = MarkdownRichTextBox.FontSize,
                            TextAlignment = TextAlignment.Left
                        };
                        MarkdownRichTextBox.Document.Blocks.Add(paragraph);
                    }
                }
            }
        }

        public void UpdateFontSize(double fontSize)
        {
            // Сохраняем текущий документ
            var originalDoc = MarkdownRichTextBox.Document;

            // 1. Обновляем Dependency Property
            FontSize = fontSize;

            // 2. Обновляем локальный ресурс (оставляем вашу текущую логику)
            if (this.Resources.Contains("NoteFontSize"))
            {
                this.Resources["NoteFontSize"] = fontSize;
            }
            else
            {
                this.Resources.Add("NoteFontSize", fontSize);
            }

            // 3. Принудительно обновляем RichTextBox
            MarkdownRichTextBox.FontSize = fontSize;

            // 4. Костыль для мгновенного обновления - перепривязка документа
            MarkdownRichTextBox.Document = new FlowDocument();
            MarkdownRichTextBox.Document = originalDoc;

            // 5. Ваш существующий цикл для параграфов (можно оставить как дополнительную меру)
            if (originalDoc is FlowDocument flowDocument)
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

        private void LoadTextIntoRichTextBox(string text, double fontSize)
        {
            MarkdownRichTextBox.Document.Blocks.Clear();
            var paragraphs = text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var para in paragraphs)
            {
                var paragraph = new Paragraph(new Run(para))
                {
                    FontSize = fontSize,
                    TextAlignment = TextAlignment.Left
                };
                MarkdownRichTextBox.Document.Blocks.Add(paragraph);
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
            => ApplyTextStyleToSelection("**");

        // Обработчик для "Курсивного" текста
        private void ItalicMenuItem_Click(object sender, RoutedEventArgs e) 
            => ApplyTextStyleToSelection("_");

        // Обработчик для "Зачеркнутого" текста
        private void StrikethroughMenuItem_Click(object sender, RoutedEventArgs e) 
            => ApplyTextStyleToSelection("~~");

        // Обработчик для "Выделенного" текста
        private void HighlightMenuItem_Click(object sender, RoutedEventArgs e) 
            => ApplyTextStyleToSelection("==");

        // Метод для применения стиля к выделенному тексту
        private void ApplyTextStyleToSelection(string markdownSyntax)
        {
            TextSelection selection = MarkdownRichTextBox.Selection;
            if (!selection.IsEmpty)
            {
                string selectedText = selection.Text.Trim();

                // Определяем, нужно ли добавлять или удалять форматирование
                bool isAlreadyFormatted = selectedText.StartsWith(markdownSyntax) &&
                                         selectedText.EndsWith(markdownSyntax);

                // Для выделенного текста (==текст==) нужно проверить двойные символы
                if (markdownSyntax == "==" && selectedText.Length >= 4)
                {
                    isAlreadyFormatted = selectedText.StartsWith("==") &&
                                        selectedText.EndsWith("==");
                }

                if (isAlreadyFormatted)
                {
                    // Удаляем форматирование
                    string unformattedText = selectedText.Substring(
                        markdownSyntax.Length,
                        selectedText.Length - 2 * markdownSyntax.Length
                    );
                    selection.Text = unformattedText;
                }
                else
                {
                    // Добавляем форматирование
                    selection.Text = markdownSyntax + selectedText + markdownSyntax;

                    // Перемещаем курсор после закрывающих символов
                    MarkdownRichTextBox.CaretPosition = MarkdownRichTextBox.CaretPosition.GetPositionAtOffset(
                        markdownSyntax.Length,
                        LogicalDirection.Forward
                    );
                }
            }
        }

        // Обработчик для преобразования клавиши Tab в абзац (4 пробела)
        private void MarkdownRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Обработка Tab
            if (e.Key == Key.Tab)
            {
                HandleTabKey();
                e.Handled = true;
                return;
            }

            // Проверяем Ctrl + ...
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.B:
                        ApplyTextStyleToSelection("**");
                        e.Handled = true;
                        break;
                    case Key.I:
                        ApplyTextStyleToSelection("_");
                        e.Handled = true;
                        break;
                    case Key.K: // Ctrl+K для зачеркнутого
                        ApplyTextStyleToSelection("~~");
                        e.Handled = true;
                        break;
                    case Key.H: // Ctrl+H для выделенного
                        ApplyTextStyleToSelection("==");
                        e.Handled = true;
                        break;
                    case Key.S:
                        SaveFile();
                        e.Handled = true;
                        break;
                }
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (_file != null)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.SelectFileInTree(_file);
            }
        }

        private void HandleTabKey()
        {
            TextSelection selection = MarkdownRichTextBox.Selection;
            if (selection.IsEmpty)
            {
                MarkdownRichTextBox.CaretPosition.InsertTextInRun("    ");
            }
            else
            {
                selection.Text = "    " + selection.Text;
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

        private void DisableDefaultShortcuts()
        {
            // Отключаем стандартные команды редактирования
            var commands = new[]
            {
        ApplicationCommands.Copy,
        ApplicationCommands.Cut,
        ApplicationCommands.Paste,
        ApplicationCommands.Undo,
        ApplicationCommands.Redo,
        EditingCommands.ToggleBold,
        EditingCommands.ToggleItalic,
        EditingCommands.ToggleUnderline,
        EditingCommands.IncreaseFontSize,
        EditingCommands.DecreaseFontSize
    };

            foreach (var command in commands)
            {
                var cb = new CommandBinding(command, (sender, e) => { e.Handled = true; });
                MarkdownRichTextBox.CommandBindings.Add(cb);
            }
        }

    }
}
