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
            // value - ActualWidth панели
            // parameter - величина отступа (например, "20" для margin 10+10)

            if (value is double width)
            {
                double margin = 20; // значение по умолчанию

                if (parameter != null)
                {
                    // Пробуем получить margin из параметра
                    if (parameter is string strParam && double.TryParse(strParam, out double parsedMargin))
                    {
                        margin = parsedMargin;
                    }
                    else if (parameter is double doubleParam)
                    {
                        margin = doubleParam;
                    }
                }

                return Math.Max(0, width - margin);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}