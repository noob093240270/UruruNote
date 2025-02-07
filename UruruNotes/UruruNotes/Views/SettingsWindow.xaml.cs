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
        internal SettingsWindow(MainViewModel mainViewModel, MarkdownViewer markdownViewer = null)
        {
            InitializeComponent();

            DataContext = mainViewModel;
            _markdownViewer = markdownViewer;

            Loaded += (s, e) => {
                if (!_isInitializedF)
                {
                    _isInitializedF = true;

                    // Подписываемся на событие изменения размера шрифта
                    FontSizeComboBox.SelectionChanged -= FontSizeComboBox_SelectionChanged;
                    FontSizeComboBox.SelectionChanged += FontSizeComboBox_SelectionChanged;
                }
            };
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Удаляем подписки
            FontSizeComboBox.SelectionChanged -= FontSizeComboBox_SelectionChanged;
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
                UpdateFontSize(size);
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.SelectedItem == null || !_isInitializedF) return;

            if (int.TryParse(comboBox.SelectedItem.ToString(), out int size))
            {
                if (_previousFontSize != size)
                {
                    UpdateFontSize(size);
                }
            }
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
    }
}
