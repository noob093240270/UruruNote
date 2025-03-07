using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNote.Models
{
    public class User
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public bool IsAuthenticated { get; set; }

        public UserSettings Settings { get; set; }
        
    }
}
