using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UruruNote.Views;
using UruruNote.ViewsModels;
using System.Diagnostics;
using System.Windows.Threading;

namespace UruruNotes.ViewsModels
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private int? _previousFontSize;
        private bool _isInitializedF = false;
        private MarkdownViewer _markdownViewer;
        private bool _isUpdatingFontSize = false;
        private bool _isUpdatingScale = false;
        private double? _previousScale;
        private MainViewModel _mainViewModel;
        private CalendarPage _calendarPage;
        internal SettingsWindow(MainViewModel mainViewModel, MarkdownViewer markdownViewer = null, CalendarPage calendarPage = null)
        {
            InitializeComponent();

            DataContext = mainViewModel;
            _markdownViewer = markdownViewer;
            _mainViewModel = mainViewModel;
            _calendarPage = calendarPage;

            Loaded += (s, e) =>
            {
                if (!_isInitializedF)
                {
                    _isInitializedF = true;

                    // Подписываемся на событие изменения размера шрифта
                    FontSizeComboBox.SelectionChanged -= FontSizeComboBox_SelectionChanged;
                    FontSizeComboBox.SelectionChanged += FontSizeComboBox_SelectionChanged;
                    ScaleComboBox.SelectionChanged -= ScaleComboBox_SelectionChanged;
                    ScaleComboBox.SelectionChanged += ScaleComboBox_SelectionChanged;
                    ScaleComboBox.SelectedItem = _mainViewModel.SelectedScaleOption;
                    _mainViewModel.UpdateScale();
                    _mainViewModel.ApplyFont();
                    Dispatcher.Invoke(() =>
                    {
                        ScaleComboBox.SelectedItem = _mainViewModel.SelectedScaleOption;
                        Debug.WriteLine($"SettingsWindow Loaded: SelectedScaleOption = {_mainViewModel.SelectedScaleOption}, ScaleComboBox.SelectedItem = {ScaleComboBox.SelectedItem}");
                    }, DispatcherPriority.Render);
                }
            };

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Удаляем подписки
            FontSizeComboBox.SelectionChanged -= FontSizeComboBox_SelectionChanged;
            ScaleComboBox.SelectionChanged -= ScaleComboBox_SelectionChanged;
        }

        private void FontSizeComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры при ручном вводе шрифта
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            var comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null || !_isInitializedF) return;

            string currentText = comboBox.Text;
            if (currentText.Length >= 2)
            {
                e.Handled = true; // Блокируем ввод, если уже введено 2 символа
            }
        }

        private void FontSizeComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            // Обрабатываем нажатие Enter
            if (e.Key == Key.Enter)
            {
                ProcessFontSizeInput(comboBox);
            }
        }

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
            if (size < 10)
            {
                ShowErrorMessage("Размер шрифта должен быть не меньше 10.");
                comboBox.Text = "";
            }
            else if (size > 35)
            {
                ShowErrorMessage("Размер шрифта должен быть не больше 35.");
                comboBox.Text = "";
            }
            else
            {
                // Если значение корректное, применяем его
                _mainViewModel.SelectedFontSize = size;
                _markdownViewer.FontSize = size;
                UpdateFontSize(size);
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null || !_isInitializedF) return;

            if (int.TryParse(comboBox.SelectedItem.ToString(), out int size))
            {
                // Проверяем, изменился ли шрифт пользователем, а не при загрузке
                if (_previousFontSize.HasValue && _previousFontSize != size)
                {
                    _mainViewModel.SelectedFontSize = size;
                    UpdateFontSize(size);
                }
                else if (!_previousFontSize.HasValue)
                {
                    _previousFontSize = size; // Устанавливаем начальное значение без уведомления
                    _mainViewModel.SelectedFontSize = size;
                }
            }
        }

        // Обработка ввода масштаба
        private void ScaleComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || !_isInitializedF) return;

            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
                return;
            }

            string currentText = comboBox.Text + e.Text;
            if (currentText.Count(c => c == '.') > 1 || currentText.Length > 4)
            {
                e.Handled = true;
            }
        }
        private void ScaleComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            if (e.Key == Key.Enter)
            {
                ProcessScaleInput(comboBox);
                e.Handled = true;
            }
        }

        private void ProcessScaleInput(ComboBox comboBox)
        {
            string currentText = comboBox.Text.Trim();
            _mainViewModel.SetScaleFromInput(currentText); // Переносим логику в MainViewModel
            comboBox.Text = _mainViewModel.ScaleDisplay;
        }

        private void ScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null || !_isInitializedF) return;

            double scale = (double)comboBox.SelectedItem;
            _mainViewModel.Scale = scale;

        }

        private void UpdateFontSize(int newSize)
        {

            if (_isUpdatingFontSize) return;
            try
            {
                _isUpdatingFontSize = true;

                if (_previousFontSize != newSize)
                {
                    MessageBox.Show($"Установлен размер шрифта: {newSize}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    _previousFontSize = newSize;
                    _markdownViewer?.UpdateFontSize(newSize);
                    _mainViewModel.SelectedFontSize = newSize;
                }
            }
            finally
            {
                _isUpdatingFontSize = false;
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
        /*private void UpdateScale(double newScale)
        {
            if (_isUpdatingScale) return;
            try
            {
                _isUpdatingScale = true;

                if (_previousScale != newScale)
                {
                    MessageBox.Show($"Установлен масштаб: {newScale:P0}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    _previousScale = newScale;
                    _markdownViewer?.UpdateScale(newScale);
                    _mainViewModel.Scale = newScale;
                    Debug.WriteLine($"SettingsWindow: Установлен масштаб: {newScale}");

                }
            }
            finally
            {
                _isUpdatingScale = false;
            }
        }*/
    }
}
