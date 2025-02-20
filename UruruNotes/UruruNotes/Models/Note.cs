using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UruruNotes.Models
{
    public class Note
    {
        // Дата, к которой привязана заметка или напоминание
        public DateTime Date { get; set; }

        // Содержимое заметки или напоминания
        public string Content { get; set; }

        // Флаг, указывающий, является ли это напоминанием (true) или обычной заметкой (false)
        public bool IsReminder { get; set; }

        // Новое свойство для времени
        public TimeSpan ReminderTime { get; set; } 

        // Конструктор без параметров для сериализации
        public Note() { }
    }
}
