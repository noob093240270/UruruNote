using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace UruruNotes
{
    public static class SettingsManager
    {
        // Путь к файлу settings.json
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        // Метод для сохранения настроек
        public static void SaveSettings(int fontSize)
        {
            var settings = new { SelectedFontSize = fontSize }; // Создаем объект для сериализации
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented); // Преобразуем в JSON
            File.WriteAllText(SettingsFilePath, json); // Записываем в файл
        }

        // Метод для загрузки настроек
        public static int LoadSettings()
        {
            if (File.Exists(SettingsFilePath)) // Проверяем, существует ли файл
            {
                string json = File.ReadAllText(SettingsFilePath); // Читаем содержимое файла
                var settings = JsonConvert.DeserializeObject<dynamic>(json); // Десериализуем JSON
                return settings.SelectedFontSize; // Возвращаем значение SelectedFontSize
            }
            return 15; // Возвращаем значение по умолчанию, если файл не существует
        }
    }
}
