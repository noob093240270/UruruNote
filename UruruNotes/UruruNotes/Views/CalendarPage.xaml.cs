using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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

        private double _buttonWidth = 50;
        public double ButtonWidth
        {
            get => _buttonWidth;
            set { _buttonWidth = value; OnPropertyChanged(); }
        }

        private double _buttonHeight = 50;
        public double ButtonHeight
        {
            get => _buttonHeight;
            set { _buttonHeight = value; OnPropertyChanged(); }
        }

        private double _dayFontSize = 14;
        public double DayFontSize
        {
            get => _dayFontSize;
            set { _dayFontSize = value; OnPropertyChanged(); }
        }

        public void UpdateScale(double scale)
        {
            ButtonWidth = 50 * scale;
            ButtonHeight = 50 * scale;
            DayFontSize = 14 * scale;
        }

        public CalendarPage(MainViewModel mainViewModel)
        {
            InitializeComponent();
            _viewModel = new CalendarViewModel();

            // Передаём масштаб главного окна
            _viewModel.Scale = mainViewModel.Scale;
            DataContext = _viewModel;

            mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(mainViewModel.Scale))
                {
                    _viewModel.Scale = mainViewModel.Scale;
                    Debug.WriteLine($"CalendarPage: Scale обновлён: {_viewModel.Scale}");
                }
            };
        }

        private double _scale = 1.0;

        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged();
                    ScaleTransform.ScaleX = _scale;
                    ScaleTransform.ScaleY = _scale;
                }
            }
        }

        public ScaleTransform ScaleTransform { get; } = new ScaleTransform(1.0, 1.0);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void TogglePanel_Click(object sender, RoutedEventArgs e)
        {
            if (_isPanelVisible)
            {
                BeginStoryboard((Storyboard)FindResource("ClosingRightMenu"));
                ToggleButtonClose.Visibility = Visibility.Collapsed;
                ToggleButtonOpen.Visibility = Visibility.Visible;
            }
            else
            {
                // Принудительно устанавливаем ширину перед анимацией
                TaskPanel.Width = 400;
                BeginStoryboard((Storyboard)FindResource("OpeningRightMenu"));
                ToggleButtonClose.Visibility = Visibility.Visible;
                ToggleButtonOpen.Visibility = Visibility.Collapsed;
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

        //private bool _isPanelVisible = true;

        /*private void TogglePanel_Click(object sender, RoutedEventArgs e)
        {
            if (_isPanelVisible)
            {
                TaskPanelColumn.Width = new GridLength(0); // Закрываем шторку
            }
            else
            {
                TaskPanelColumn.Width = GridLength.Auto; // Открываем шторку
            }

            _isPanelVisible = !_isPanelVisible; // Меняем состояние
        }*/
    }
}

