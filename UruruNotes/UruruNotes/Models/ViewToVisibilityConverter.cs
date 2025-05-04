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
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            if (!Enum.TryParse(parameter.ToString(), out CalendarViewModel.ViewType targetView))
                return Visibility.Collapsed;

            var currentView = (CalendarViewModel.ViewType)value;
            return currentView == targetView ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
