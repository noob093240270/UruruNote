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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UruruNotes.ViewsModels;

namespace UruruNotes
{
    /// <summary>
    /// Логика взаимодействия для CalendarPage.xaml
    /// </summary>
    public partial class CalendarPage : Window
    {

        // Объявляем заголовок на уровне класса, чтобы он был доступен во всех методах
        private string _defaultText = "Создание задачи на 07 января 2025";
        public CalendarPage()
        {
            InitializeComponent();
            DataContext = new CalendarViewModel();

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
