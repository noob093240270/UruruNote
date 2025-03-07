using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNote.Models
{
    public class UserSettings
    {
        public int Id { get; set; }
        public virtual User User { get; set; }

        public int UserId { get; set; }

        public int WindowH {  get; set; }
        public int WindowW { get; set; }

        public bool DarkMode { get; set; }

        public int GridSplitPos { get; set; }
    }
}
