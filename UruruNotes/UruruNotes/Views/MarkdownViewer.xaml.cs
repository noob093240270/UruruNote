﻿using System.IO;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using UruruNotes.Models;

namespace UruruNote.Views
{
    public partial class MarkdownViewer : UserControl
    {
        private FileItem _file;

        public MarkdownViewer(FileItem file)
        {
            InitializeComponent();

            if (file == null)
            {
                MessageBox.Show("Ошибка: файл не выбран.");
                return;
            }

            _file = file;
            LoadFileContent(_file.FilePath);

            // Подписываемся на событие KeyDown
            this.KeyDown += MarkdownViewer_KeyDown;
        }

        private void MarkdownViewer_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, была ли нажата комбинация клавиш Ctrl + S
            if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Вызываем метод сохранения
                SaveFile();
                e.Handled = true; // Останавливаем дальнейшую обработку события
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            string filePath = _file.FilePath;  // Путь к файлу
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                // Получаем текст из RichTextBox
                TextRange range = new TextRange(MarkdownRichTextBox.Document.ContentStart, MarkdownRichTextBox.Document.ContentEnd);

                // Сохраняем текст в файл
                File.WriteAllText(filePath, range.Text, Encoding.UTF8);

                MessageBox.Show("Файл успешно сохранен.");
            }
            else
            {
                MessageBox.Show("Ошибка: файл не найден.");
            }
        }

        private void LoadFileContent(string filePath)
        {
            if (File.Exists(filePath))
            {
                string fileContent = File.ReadAllText(filePath, Encoding.UTF8);

                // Очищаем RichTextBox перед загрузкой нового текста
                MarkdownRichTextBox.Document.Blocks.Clear();

                // Загружаем текст в RichTextBox
                TextRange range = new TextRange(MarkdownRichTextBox.Document.ContentStart, MarkdownRichTextBox.Document.ContentEnd);
                range.Text = fileContent;
            }
        }

        // Обработчик для "Жирного" текста
        private void BoldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("**");
        }

        // Обработчик для "Курсивного" текста
        private void ItalicMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("_");
        }

        // Обработчик для "Зачеркнутого" текста
        private void StrikethroughMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("~~");
        }

        // Обработчик для "Выделенного" текста
        private void HighlightMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ApplyTextStyleToSelection("==");
        }

        // Метод для применения стиля к выделенному тексту
        private void ApplyTextStyleToSelection(string markdownSyntax)
        {
            // Получаем выделенный текст
            TextSelection selection = MarkdownRichTextBox.Selection;

            if (!selection.IsEmpty)
            {
                // Применяем Markdown синтаксис
                string selectedText = selection.Text;
                string formattedText = markdownSyntax + selectedText + markdownSyntax;

                // Заменяем выделенный текст
                TextRange range = new TextRange(selection.Start, selection.End);
                range.Text = formattedText;
            }
        }

        // Обработчик для преобразования клавиши Tab в абзац (4 пробела)
        private void MarkdownRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                // Получаем текущую позицию курсора в RichTextBox
                TextSelection selection = MarkdownRichTextBox.Selection;

                if (selection.IsEmpty)
                {
                    // Если ничего не выделено, вставляем 4 пробела на месте курсора
                    var caretPosition = MarkdownRichTextBox.CaretPosition;

                    // Вставляем 4 пробела в текущее положение курсора
                    caretPosition.InsertTextInRun("    ");
                }
                else
                {
                    // Если текст выделен, добавляем 4 пробела в начало выделенного текста
                    selection.Text = "    " + selection.Text;
                }

                // Останавливаем обработку клавиши Tab
                e.Handled = true;
            }
        }

    }
}
