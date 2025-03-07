using System.Configuration;
using System.Data;
using System.Windows;

namespace UruruNotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
         
        /// <summary>
        /// Метод для обновления глобального размера шрифта
        /// </summary>
        /// <param name="fontSize">Размер шрифта</param>
        public static void UpdateGlobalFontSize(double fontSize)
        {
            if (Application.Current.Resources.Contains("GlobalFontSize"))
            {
                Application.Current.Resources["GlobalFontSize"] = fontSize;
            }
            else
            {
                Application.Current.Resources.Add("GlobalFontSize", fontSize);
            }
        }

        /// <summary>
        /// Событие запуска приложения
        /// </summary>

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Устанавливаем размер шрифта по умолчанию
            int savedFontSize = SettingsManager.LoadSettings();
            UpdateGlobalFontSize(savedFontSize);
        }
    }
}


