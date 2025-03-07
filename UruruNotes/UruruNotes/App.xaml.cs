using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Media;
using UruruNote.ViewsModels;

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
        public static void UpdateGlobalScale(double scale)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var scaleTransform = new ScaleTransform(scale, scale);
                window.LayoutTransform = scaleTransform;
            }
        }

        /// <summary>
        /// Событие запуска приложения
        /// </summary>

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загрузка сохраненных настроек
            var settings = SettingsManager.LoadSettings();
            UpdateGlobalFontSize(settings.FontSize);
            UpdateGlobalScale(settings.Scale);
        }

    }
}


