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
        

        public CalendarPage()
        {
            InitializeComponent();
            DataContext = new CalendarViewModel();
        }

        private bool _isPanelVisible = true;

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



    }
}
