using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;


namespace UruruNotes.Converters
{
    public class ThemeToIconPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool darkMode && parameter is string iconName)
            {
                string themeFolder = darkMode ? "Dark" : "Light";
                string path = $"pack://application:,,,/UruruNotes;component/Resources/{themeFolder}/{iconName}.png";
                Debug.WriteLine($"ThemeToIconPathConverter: DarkMode={darkMode}, IconName={iconName}, Path={path}");
                return path;
            }
            Debug.WriteLine("ThemeToIconPathConverter: Используется путь по умолчанию");
            return "pack://application:,,,/UruruNotes;component/Resources/Light/settingsbutton.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}