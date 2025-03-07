using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;



namespace UruruNotes.ViewsModels
{
    public class TaskViewModal
    {
        public DateTime SelectedDate { get; set; }
        public string TaskDescription { get; set; }

        public ICommand SaveTaskCommand { get; }

        public TaskViewModal()
        {
            SaveTaskCommand = new RelayCommand(SaveTask);
        }

        private void SaveTask()
        {
            // Логика сохранения задачи (например, запись в базу или список)
            Console.WriteLine($"Задача на {SelectedDate:D}: {TaskDescription}");
        }
    }
}
