using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNotes.ViewsModels
{
    public class ThemeOption
    {
        public string DisplayName { get; set; }
        public bool IsDarkMode { get; set; }

        public override string ToString() => DisplayName;
    }
}
