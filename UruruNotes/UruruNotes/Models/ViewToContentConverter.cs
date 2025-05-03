using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UruruNotes.ViewsModels;

using System;
using System.Globalization;
using System.Windows.Data;

namespace UruruNotes.Models
{
    public class ViewToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Мы будем передавать сам ViewModel через parameter
            if (value is CalendarViewModel.ViewType viewType &&
                parameter is CalendarViewModel vm)
            {
                return viewType == CalendarViewModel.ViewType.Notes ?
                       vm.NewTaskContent :
                       vm.NewTaskContentRemind;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string content && parameter is CalendarViewModel vm)
            {
                if (vm.CurrentView == CalendarViewModel.ViewType.Notes)
                    vm.NewTaskContent = content;
                else
                    vm.NewTaskContentRemind = content;
            }
            return value;
        }
    }
}
