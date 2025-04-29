using System;
using System.Collections.ObjectModel;
using System.Windows;
using UruruNote.Models;
using UruruNote.ViewsModels;
using UruruNotes.Models;

namespace UruruNotes.Views
{
    public partial class NewFolderWindow : Window
    {
        public string FolderName { get; private set; }

        private readonly MainViewModel _viewModel;

        public NewFolderWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            FolderName = FolderNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(FolderName))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите имя папки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

}
