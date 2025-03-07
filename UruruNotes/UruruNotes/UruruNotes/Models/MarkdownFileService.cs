using System;
using System.IO;
using System.Windows;

namespace UruruNote.Models
{
    public class MarkdownFileService
    {
        // Путь к папке MyFolders для хранения файлов
        public string RootDirectory { get; }

        public MarkdownFileService()
        {
            // Указываем путь к папке MyFolders внутри приложения
            RootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyFolders");

            // Создание папки MyFolders, если она не существует
            if (!Directory.Exists(RootDirectory))
            {
                Directory.CreateDirectory(RootDirectory);
            }
        }

        public string CreateMarkdownFile(string filePath)
        {
            // Получаем имя файла без пути
            string fileName = Path.GetFileName(filePath);

            // Создаем файл в указанной папке
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

            return fileName; // Возвращаем только имя файла
        }

        public string CreateMarkdownFileInSubfolder(string folderPath, string fileName)
        {
            // Формируем полный путь для создания файла в подкаталоге
            string filePath = Path.Combine(folderPath, fileName + ".md");

            string mainFolderFilePath = Path.Combine(RootDirectory, fileName + ".md");

            // Проверяем, существует ли файл
            if (File.Exists(filePath))
            {
                MessageBox.Show($"Файл {fileName}.md уже существует в выбранной папке.");
                return null; // Если файл существует, ничего не делаем
            }

            // Создаем файл в подкаталоге
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Добавляем начальный контент
                writer.WriteLine("# Мой начальный файл");
                writer.WriteLine("Создано: " + DateTime.Now.ToString("G"));
                writer.WriteLine();

                writer.WriteLine("## Задачи на сегодня");
                writer.WriteLine("1. Поспать");
                writer.WriteLine("2. Поспать");
                writer.WriteLine("3. Поспать");

                writer.WriteLine("### Дополнительная информация");
                writer.WriteLine("Этот файл создан автоматически.");
            }

            // После создания файла в подкаталоге удаляем его из основной папки
            if (File.Exists(mainFolderFilePath))
            {
                File.Delete(mainFolderFilePath);
            }

            return fileName; // Возвращаем имя файла
        }






        public bool IsMarkdownFileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }

}