using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace UruruNotes.Models
{
    public class SubtractMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double parentWidth)
            {
                double margin = 40; // значение по умолчанию
                if (parameter != null)
                {
                    if (!double.TryParse(parameter.ToString(), out margin))
                        margin = 40;
                }
                return Math.Max(0, parentWidth - margin);
            }
            return 0;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}