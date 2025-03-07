using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace UruruNotes.Models
{
    class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCurrentMonth = (bool)value;
            return isCurrentMonth ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); // Светло-серый для неактивных дней
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
