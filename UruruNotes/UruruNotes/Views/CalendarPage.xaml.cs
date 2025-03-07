using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UruruNote.ViewsModels;
using UruruNotes.ViewsModels;

namespace UruruNotes
{
    /// <summary>
    /// Логика взаимодействия для CalendarPage.xaml
    /// </summary>
    public partial class CalendarPage : Window
    {
        private readonly CalendarViewModel _viewModel;
        private readonly double baseWidth = 450; // Базовая ширина окна
        private readonly double baseHeight = 450; // Базовая высота окна
        private bool _isPanelVisible = true;
        private string _defaultText = "Создание задачи на 07 января 2025";

        public CalendarPage(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _viewModel = new CalendarViewModel();
            _viewModel.Scale = mainViewModel.Scale;
            DataContext = _viewModel;

            UpdateWindowSize(_viewModel.Scale);

            mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(mainViewModel.Scale))
                {
                    _viewModel.Scale = mainViewModel.Scale;
                    UpdateWindowSize(_viewModel.Scale);
                    Debug.WriteLine($"CalendarPage: MainViewModel.Scale передан и применён: {_viewModel.Scale}");
                }
            };
        }

        private void UpdateWindowSize(double scale)
        {
            this.Width = Math.Max(baseWidth * scale, 450); // Учитываем MinWidth
            this.Height = Math.Max(baseHeight * scale, 450); // Учитываем MinHeight
            Debug.WriteLine($"CalendarPage: Масштаб {scale}, Размеры окна: {this.Width}x{this.Height}");
        }

        private void TogglePanel_Click(object sender, RoutedEventArgs e)
        {
            if (_isPanelVisible)
            {
                TaskPanelColumn.Width = new GridLength(0); // Полностью схлопываем колонку
                TrianglePath.RenderTransform = new RotateTransform(90, 15, 15);
            }
            else
            {
                TaskPanelColumn.Width = GridLength.Auto; // Возвращаем колонку
                TrianglePath.RenderTransform = new RotateTransform(0, 15, 15);
            }

            _isPanelVisible = !_isPanelVisible;
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Text = _defaultText + "\n"; // Устанавливаем заголовок + новая строка
                textBox.CaretIndex = textBox.Text.Length; // Перемещаем курсор в конец
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                int caretIndex = textBox.CaretIndex;
                string[] lines = textBox.Text.Split('\n');

                // Запрещаем удаление заголовка
                if ((e.Key == Key.Back || e.Key == Key.Delete) && caretIndex <= lines[0].Length)
                {
                    e.Handled = true;
                    return;
                }

                // Запрещаем изменение заголовка
                if (caretIndex <= lines[0].Length && e.Key != Key.Right && e.Key != Key.Left)
                {
                    e.Handled = true;
                }
            }
        }
    }
}

