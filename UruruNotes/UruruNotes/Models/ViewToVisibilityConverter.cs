using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using UruruNotes.ViewsModels;

namespace UruruNotes.Models
{
    public class ViewToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentView = (CalendarViewModel.ViewType)value;
            var targetView = (CalendarViewModel.ViewType)Enum.Parse(
                typeof(CalendarViewModel.ViewType),
                parameter.ToString()
            );

            return currentView == targetView
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
