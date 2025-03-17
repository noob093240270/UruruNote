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

namespace UruruNotes
{
    public static class SettingsManager
    {
        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        public class Settings
        {
            public int FontSize { get; set; }
            public double Scale { get; set; }
        }
        public static void SaveSettings(int fontSize, double scale)
        {
            try
            {
                var settings = new Settings
                {
                    FontSize = fontSize,
                    Scale = scale
                };
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
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

        // Метод для загрузки всех настроек сразу (опционально)
        public static Settings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonConvert.DeserializeObject<Settings>(json);
                    var loadedSettings = new Settings
                    {
                        FontSize = Math.Clamp(settings?.FontSize ?? 15, 10, 35),
                        Scale = Math.Clamp(settings?.Scale ?? 1.0, 0.5, 2.0)
                    };
                    return loadedSettings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
            return new Settings { FontSize = 15, Scale = 1.0 }; // Значения по умолчанию
        }
    }
}
