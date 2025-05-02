using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UruruNotes.ViewsModels;

namespace UruruNotes.Models
{
    public class ViewToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CalendarViewModel.ViewType viewType)
            {
                return viewType == CalendarViewModel.ViewType.Notes ?
                       "Редактирование заметки" : "Редактирование напоминания";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
