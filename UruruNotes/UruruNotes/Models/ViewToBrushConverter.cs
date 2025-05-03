using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Data;
using UruruNotes.ViewsModels;
using System.Threading.Tasks;
using System.Globalization;

namespace UruruNotes.Models
{
    public class ViewToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentView = (CalendarViewModel.ViewType)value;
            var targetView = (CalendarViewModel.ViewType)Enum.Parse(
                typeof(CalendarViewModel.ViewType),
                parameter.ToString()
            );

            return currentView == targetView
                ? Brushes.LightBlue
                : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
