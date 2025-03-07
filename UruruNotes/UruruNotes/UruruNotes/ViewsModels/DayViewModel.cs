using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNotes.ViewsModels
{
    public class DayViewModel
    {
        public DateTime? Date { get; set; }
        public string DisplayText { get; set; }

        public bool IsCurrentMonth { get; set; }
    }

}
