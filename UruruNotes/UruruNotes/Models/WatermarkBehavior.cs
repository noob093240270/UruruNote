/*ДЛЯ ТЕКСТОВЫХ ПОЛЕЙ, где необходимо указать, какую информацию следует вводить */



using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UruruNotes.Helpers
{
    public static class WatermarkBehavior
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(WatermarkBehavior), new PropertyMetadata(null, OnWatermarkChanged));

        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);
        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if (e.NewValue != null)
                {
                    textBox.GotFocus += TextBoxGotFocus;
                    textBox.LostFocus += TextBoxLostFocus;
                    UpdateWatermark(textBox);
                    textBox.Unloaded += (s, _) =>
                    {
                        textBox.GotFocus -= TextBoxGotFocus;
                        textBox.LostFocus -= TextBoxLostFocus;
                    };
                }
            }
        }


        private static void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Foreground = Brushes.Black;
                UpdateWatermark(textBox);
            }
        }

        private static void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                UpdateWatermark(textBox);
            }
        }

        private static void UpdateWatermark(TextBox textBox)
        {
            string watermark = GetWatermark(textBox);
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Foreground = Brushes.Gray;
                textBox.Text = watermark;
            }
            else
            {
                textBox.Foreground = Brushes.Black;
            }
        }
    }
}
