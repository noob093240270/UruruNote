using System;
using System.IO;

namespace UruruNote.Models
{
    public class MarkdownFileService
    {
        private readonly string _directoryPath;

        public MarkdownFileService()
        {
            // Путь к папке внутри директории приложения
            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MarkdownFiles");

            // Создание папки, если она не существует
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        public string CreateMarkdownFile(string fileName)
        {
            string filePath = Path.Combine(_directoryPath, fileName);

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Заголовок первого уровня
                    writer.WriteLine("# Мой начальный файл");
                    writer.WriteLine("Создано: " + DateTime.Now.ToString("G"));
                    writer.WriteLine();

                    // Заголовок второго уровня
                    writer.WriteLine("## Задачи на сегодня");
                    writer.WriteLine("1. Поспать");
                    writer.WriteLine("2. Поспать");
                    writer.WriteLine("3. Поспать");
                    writer.WriteLine();


                    // Заключение
                    writer.WriteLine("### Дополнительная информация");
                    writer.WriteLine("Этот файл создан автоматически для организации задач на день.");
                }

                return filePath; // Возвращаем путь к созданному файлу
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при создании файла: " + ex.Message);
            }
        }


        public bool IsMarkdownFileExists(string fileName)
        {
            string filePath = Path.Combine(_directoryPath, fileName);
            return File.Exists(filePath); // Проверяем, существует ли файл по полному пути
        }

    }
}