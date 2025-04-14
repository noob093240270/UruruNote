using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;
using UruruNotes.Properties;
using System.Diagnostics;

namespace UruruNotes
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        public class Settings
        {
            public int FontSize { get; set; }
            public double Scale { get; set; }
            public bool DarkMode { get; set; } = false;
        }
        public static void SaveSettings(int fontSize, double scale, bool darkMode)
        {
            try
            {
                var settings = new Settings
                {
                    FontSize = Math.Clamp(fontSize, 10, 35),
                    Scale = Math.Clamp(scale, 0.5, 2.0),
                    DarkMode = darkMode
                };

                // Убедимся, что директория существует
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Сохраняем настройки в JSON
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
                Debug.WriteLine("Настройки успешно сохранены.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
                Debug.WriteLine($"Стек вызовов: {ex.StackTrace}");
            }
        }

        // Метод для загрузки размера шрифта
        public static int LoadFontSize()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);
                    return Math.Clamp(settings?.FontSize ?? 15, 10, 35);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке размера шрифта: {ex.Message}");
            }
            return 15; // Значение по умолчанию
        }

        // Метод для загрузки масштаба
        public static double LoadScale()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);
                    return Math.Clamp(settings?.Scale ?? 1.0, 0.5, 2.0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке масштаба: {ex.Message}");
            }
            return 1.0; // Значение по умолчанию
        }
        /// <summary>
        ///  // Метод для загрузки всех настроек сразу (опционально)
        /// </summary>
        /// <returns></returns>
        public static Settings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);
                    if (settings == null)
                    {
                        return new Settings();
                    }

                    // Применяем ограничения к значениям
                    settings.FontSize = Math.Clamp(settings.FontSize, 10, 35);
                    settings.Scale = Math.Clamp(settings.Scale, 0.5, 2.0);
                    return settings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
                Debug.WriteLine($"Стек вызовов: {ex.StackTrace}");
            }

            // Возвращаем настройки по умолчанию, если файл не существует или произошла ошибка
            return new Settings();
        }
    }
}
