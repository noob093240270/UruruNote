using Markdig;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UruruNote.ViewsModels;
using UruruNotes;
using UruruNotes.Models;
using UruruNotes.Views;



namespace UruruNote.Views
{
    public partial class MarkdownViewer : UserControl, INotifyPropertyChanged
    {
        // Реализуем INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

                    // Загружаем текст
                    string markdownText = File.ReadAllText(filePath);

                    // Чистим весь документ
                    MarkdownRichTextBox.Document.Blocks.Clear();

                    // Создаём FlowDocument без лишних отступов
                    FlowDocument doc = new FlowDocument();
                    doc.PagePadding = new Thickness(0); // убираем внешние отступы документа
                    doc.Blocks.Clear();

                    // Разбиваем текст на строки и добавляем по абзацу
                    foreach (string line in markdownText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
                    {
                        var para = new Paragraph(new Run(line))
                        {
                            Margin = new Thickness(0), // убираем отступы у абзаца
                            Padding = new Thickness(0),
                            FontSize = MarkdownRichTextBox.FontSize,
                            TextAlignment = TextAlignment.Left
                        };
                        doc.Blocks.Add(para);
                    }

                    MarkdownRichTextBox.Document = doc;

                    // Обновляем HTML-просмотр
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


        private double _editingScrollViewerVerticalOffset = 0;

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Header.ToString() == "Предпросмотр")
                {
                    // Сохраняем позицию скролла (возможно, для WebView2, если он скроллируемый)
                    // Здесь логика может отличаться в зависимости от того, как скроллируется WebView2
                }
                else if (selectedTab.Header.ToString() == "Редактирование")
                {
                    // Получаем RichTextBox
                    RichTextBox textBox = FindVisualChild<RichTextBox>(selectedTab);
                    if (textBox != null)
                    {
                        // Получаем ScrollViewer внутри RichTextBox
                        ScrollViewer editingScrollViewer = FindVisualChild<ScrollViewer>(textBox);
                        if (editingScrollViewer != null)
                        {
                            // Восстанавливаем вертикальную позицию скролла
                            SetVerticalOffset(editingScrollViewer, _editingScrollViewerVerticalOffset);
                            editingScrollViewer.ScrollToHorizontalOffset(0);
                        }
                    }
                }
            }
        }

        private double GetVerticalOffset(ScrollViewer scrollViewer)
        {
            if (scrollViewer != null)
            {
                return scrollViewer.VerticalOffset;
            }
            return 0;
        }

        private void SetVerticalOffset(ScrollViewer scrollViewer, double offset)
        {
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(offset);
            }
        }

        // Вспомогательный метод для поиска Visual Child определенного типа
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    return (T)child;
                }
                T grandchild = FindVisualChild<T>(child);
                if (grandchild != null)
                {
                    return grandchild;
                }
            }
            return null;
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
                if (string.IsNullOrEmpty(markdownText))
                {
                    return "<head><meta charset=\"UTF-8\"><style>mark { background-color: yellow; }</style></head><body></body>";
                }

                // Заменяем одиночные переводы строк на двойные перед преобразованием в HTML
                string processedMarkdown = markdownText.Replace("\r\n", "\r\n\r\n").Replace("\n", "\n\n");

                // Создаем pipeline с расширениями

                var pipeline = new Markdig.MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseEmphasisExtras()
                    .Build();

                var htmlContent = Markdig.Markdown.ToHtml(processedMarkdown, pipeline);
                return $"<head><meta charset=\"UTF-8\"><style>mark {{ background-color: yellow; }}</style></head><body>{htmlContent}</body>";

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка преобразования Markdown: {ex.Message}");
                return $"<head><meta charset=\"UTF-8\"><style>mark {{ background-color: yellow; }}</style></head><body><p>Ошибка преобразования: {ex.Message}</p></body>";
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

        // Пример ViewModel или кода UserControl
        // Коллекция доступных размеров шрифта
        private ObservableCollection<int> _fontSizeOptions;
        public ObservableCollection<int> FontSizeOptions
        {
            get
            {
                if (_fontSizeOptions == null)
                {
                    _fontSizeOptions = new ObservableCollection<int> { 10, 12, 14, 16, 18, 20, 22, 24, 28, 30, 35 };
                }
                return _fontSizeOptions;
            }
        }

        // Текущий выбранный размер шрифта
        private int _selectedFontSize;

        public int SelectedFontSize
        {
            get { return _selectedFontSize; }
            set
            {
                if (_selectedFontSize != value)
                {
                    _selectedFontSize = value;
                    UpdateFontSize(value); // Обновляем размер шрифта
                    // Получаем MainViewModel из DataContext
                    if (DataContext is MainViewModel mainViewModel)
                    {
                        SettingsManager.SaveSettings(_selectedFontSize, SettingsManager.LoadScale(), mainViewModel.IsNotificationsEnabled);
                    }
                    else
                    {
                        // Если DataContext не MainViewModel, используем значение по умолчанию
                        SettingsManager.SaveSettings(_selectedFontSize, SettingsManager.LoadScale(), false);
                    }
                    OnPropertyChanged(nameof(SelectedFontSize)); // Уведомляем об изменении
                }
            }
        }



        private void ApplyFontSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(FontSizeComboBox.Text, out double fontSize))
            {
                MarkdownRichTextBox.FontSize = fontSize;
            }
            else
            {
                MessageBox.Show("Введите корректный размер шрифта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private bool _isUpdatingFontSize = false;

        // Обработчик для текстового ввода в поле выбора размера шрифта
        private void FontSizeComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры при ручном вводе шрифта
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            string currentText = comboBox.Text + e.Text;
            if (currentText.Length > 2) // Ограничение по длине ввода
            {
                e.Handled = true;
            }



        }


        // Обработка ввода размера шрифта при нажатии Enter
        private void ProcessFontSizeInput(ComboBox comboBox)
        {
            string currentText = comboBox.Text;

            // Проверяем, является ли ввод числом
            if (!int.TryParse(currentText, out int size))
            {
                ShowErrorMessage("Пожалуйста, введите числовое значение.");
                comboBox.Text = "";
                return;
            }

            // Проверяем диапазон (от 10 до 35)
            if (size < 10 || size > 35)
            {
                ShowErrorMessage("Размер шрифта должен быть в диапазоне от 10 до 35.");
                comboBox.Text = "";
            }
            else
            {
                // Если значение корректное, применяем его
                SelectedFontSize = size;
            }
        }










        // Отображение сообщения об ошибке
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            // ⬇️ 1. Загружаем сохранённый шрифт
            int savedFontSize = SettingsManager.LoadFontSize();
            SelectedFontSize = savedFontSize;

            // ⬇️ 2. Применяем его к редактору
            UpdateFontSize(savedFontSize);

            // ⬇️ 3. Устанавливаем значение в ComboBox
            FontSizeComboBox.Text = savedFontSize.ToString();

            // Синхронизация с глобальными настройками (если они есть)
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

            this.KeyDown += MarkdownViewer_KeyDown;

            // Обработчики событий
            KeyDown += MarkdownViewer_KeyDown;

            SelectedFontSize = SettingsManager.LoadFontSize();
            App.FontSizeChanged += OnGlobalFontSizeChanged;

            Unloaded += (s, e) =>
            {
                _previewTimer.Stop();
                App.FontSizeChanged -= OnGlobalFontSizeChanged;
            };

            IsVisibleChanged += (s, e) =>
            {
                if (IsVisible)
                {
                    _ = UpdatePreviewAsync();
                    UpdateFontSize((double)Application.Current.Resources["GlobalFontSize"]);
                    ApplyScale((double)Application.Current.Resources["GlobalScale"]);
                }
            };
        }






        // Обработчик события изменения выбранного шрифта
        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Content.ToString(), out int newFontSize))
                {
                    SelectedFontSize = newFontSize; // Устанавливаем новый размер шрифта
                }
            }


            UpdateFontSize(SelectedFontSize);
        }

        // Обработчик для клавиши Enter в ComboBox
        private void FontSizeComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(FontSizeComboBox.Text, out int newFontSize))
                {
                    SelectedFontSize = newFontSize; // Устанавливаем новый размер шрифта
                    UpdateFontSize(SelectedFontSize);
                }
            }
        }

        // Обновление размера шрифта на элементе управления (например, RichTextBox)
        private int? _previousFontSize;
        private bool _isInitializedF = false;
        private MarkdownViewer _markdownViewer;
        private MainViewModel _mainViewModel;
        private void UpdateFontSize(int newSize)
        {
            MarkdownRichTextBox.FontSize = newSize;
            if (_isUpdatingFontSize) return;

            try
            {
                _isUpdatingFontSize = true;

                if (_previousFontSize != newSize)
                {

                    _previousFontSize = newSize;
                    _markdownViewer?.UpdateFontSize(newSize);  // Обновление шрифта в markdownViewer
                }
            }
            finally
            {
                _isUpdatingFontSize = false;
            }
            // Обновляем глобальный размер шрифта
            App.UpdateGlobalFontSize(newSize);


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
                        string paragraphText = textRange.Text.TrimEnd();

                        textToSave.AppendLine(paragraphText); // сохраняем одну строку на абзац
                    }
                }

                // Сохраняем, убрав лишние пустые строки в конце
                File.WriteAllText(filePath, textToSave.ToString().TrimEnd(), Encoding.UTF8);
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
                MarkdownRichTextBox.Document.Blocks.Clear();
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var paragraph = new Paragraph(new Run(line))
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

        private void Header1MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyMarkdownHeader("# ");
        }

        private void Header2MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyMarkdownHeader("## ");
        }

        private void Header3MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyMarkdownHeader("### ");    
        }

        private void ApplyMarkdownHeader(string prefix)
        {
            if (MarkdownRichTextBox.Selection != null)
            {
                string selectedText = MarkdownRichTextBox.Selection.Text;
                string newText = prefix + selectedText;
                MarkdownRichTextBox.Selection.Text = newText;

                // Перемещаем курсор в конец добавленного текста
                TextPointer caretPosition = MarkdownRichTextBox.Selection.End.GetInsertionPosition(LogicalDirection.Forward);
                MarkdownRichTextBox.CaretPosition = caretPosition;
                MarkdownRichTextBox.Selection.Select(caretPosition, caretPosition);
            }
        }


        // Метод для применения стиля к выделенному тексту
        private void ApplyTextStyleToSelection(string markdownSyntax)
        {
            TextSelection selection = MarkdownRichTextBox.Selection;

            if (!selection.IsEmpty)
            {
                string selectedText = selection.Text;

                // Проверка, есть ли уже обёртка нужным синтаксисом
                bool isAlreadyFormatted = selectedText.StartsWith(markdownSyntax) && selectedText.EndsWith(markdownSyntax);

                // Специальная проверка для выделения с двойными символами (например, "==")
                if (markdownSyntax == "==" && selectedText.Length >= 4)
                {
                    isAlreadyFormatted = selectedText.StartsWith("==") && selectedText.EndsWith("==");
                }

                if (isAlreadyFormatted)
                {
                    // Убираем markdown-символы
                    string unformattedText = selectedText.Substring(
                        markdownSyntax.Length,
                        selectedText.Length - 2 * markdownSyntax.Length
                    );
                    selection.Text = unformattedText;
                }
                else
                {
                    // Добавляем markdown-символы вокруг выделенного текста
                    selection.Text = markdownSyntax + selectedText + markdownSyntax;

                    // Смещаем курсор после конца отформатированного текста
                    TextPointer newCaretPosition = selection.End.GetPositionAtOffset(markdownSyntax.Length);
                    if (newCaretPosition != null)
                    {
                        MarkdownRichTextBox.CaretPosition = newCaretPosition;
                    }
                }
            }
        }


        // Обработчик для преобразования клавиши Tab в абзац (4 пробела)
        private void MarkdownRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Обработка Enter
            if (e.Key == Key.Enter)
            {
                var caretPosition = MarkdownRichTextBox.CaretPosition;

                // Проверяем, что CaretPosition не null и что она внутри текста
                if (caretPosition != null && caretPosition.Paragraph != null)
                {
                    caretPosition.InsertTextInRun("\r\n");
                    e.Handled = true; // Предотвращаем дальнейшую обработку Enter по умолчанию

                    // Перемещаем каретку на следующее место
                    MarkdownRichTextBox.CaretPosition = caretPosition.GetPositionAtOffset(1, LogicalDirection.Forward);
                }
                else
                {
                    // Если CaretPosition или Paragraph null, можно сделать дополнительную обработку (например, добавить новый абзац)
                    MessageBox.Show("Ошибка: курсор не находится в тексте.");
                }

                return;
            }


            // Обработка Tab (остается без изменений)
            if (e.Key == Key.Tab)
            {
                HandleTabKey();
                e.Handled = true;
                return;
            }

            // Обработка Ctrl + ... (остается без изменений)
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
