using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UruruNote.Views;
using UruruNote.ViewsModels;
using UruruNotes.Models;

namespace UruruNotes.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly double baseWidth = 800; // Базовая ширина окна
        private readonly double baseHeight = 490; // Базовая высота окна

        public MainWindow()
        {
            InitializeComponent();

            // Создаем экземпляр ViewModel
            _viewModel = new MainViewModel();
            DataContext = _viewModel; // Установка DataContext на ViewModel

            // Подписываемся на событие OpenFileRequest
            _viewModel.OpenFileRequest += ViewModel_OpenFileRequest;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateWindowSize(_viewModel.Scale); // Устанавливаем начальные размеры окна с учётом масштаба
        }

        /// <summary>
        /// Обработчик события открытия файла из ViewModel
        /// </summary>
        private void ViewModel_OpenFileRequest(UserControl userControl)
        {
            // Заменяем контент в PageFrame
            PageFrame.Content = userControl;
        }

        /// <summary>
        /// Обработчик изменения свойств ViewModel для обновления масштаба
        /// </summary>
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.Scale))
            {
                UpdateWindowSize(_viewModel.Scale); // Обновляем размеры окна при изменении масштаба
            }
        }

        /// <summary>
        /// Метод для динамического обновления размеров окна в зависимости от масштаба
        /// </summary>
        private void UpdateWindowSize(double scale)
        {
            this.Width = Math.Max(baseWidth * scale, MinWidth);
            this.Height = Math.Max(baseHeight * scale, MinHeight);
        }

        /// <summary>
        /// Обработчик для сворачивания и разворачивания TreeView
        /// </summary>
        private void ToggleVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if ((TreeViewGrid.Visibility == Visibility.Visible) && (ButtonOpen.Visibility == Visibility.Collapsed))
            {
                //TreeViewGrid.Visibility = Visibility.Collapsed;
                ButtonClose.Visibility = Visibility.Collapsed;
                ButtonOpen.Visibility = Visibility.Visible;
            }
            else
            {
                //TreeViewGrid.Visibility = Visibility.Visible;
                ButtonOpen.Visibility = Visibility.Collapsed;
                ButtonClose.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке создания нового файла
        /// </summary>
        public void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateNewMarkdownFile();
        }

        /// <summary>
        /// Обработчик клика по кнопке создания новой папки
        /// </summary>
        private void CreateNewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateFolder();
        }

        /// <summary>
        /// Обработчик для добавления файла в папку ПКМ
        /// </summary>
        private void AddFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is FolderItem selectedFolder)
            {
                _viewModel.AddFileCommand.Execute(selectedFolder);
            }
        }

        /// <summary>
        /// Обработчик для предварительного клика мышью по TreeView (снятие выделения)
        /// </summary>
        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому кликнули
            var clickedElement = e.OriginalSource as DependencyObject;

            // Ищем TreeViewItem, к которому относится клик
            while (clickedElement != null && !(clickedElement is TreeViewItem))
            {
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            if (clickedElement is TreeViewItem treeViewItem && treeViewItem.IsSelected)
            {
                treeViewItem.IsSelected = false; // Убираем выделение
                e.Handled = true;               // Останавливаем дальнейшую обработку события
            }
        }

        /// <summary>
        /// Обработчик изменения выбранного элемента в TreeView
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileItem fileItem)
            {
                _viewModel.SelectedTreeViewItemChanged(fileItem); // Передаем выбранный элемент
                OpenFile(fileItem); // Открываем файл в PageFrame

                // Очищаем выделение
                if (sender is TreeView treeView)
                {
                    UnselectAll(treeView);
                }
            }
        }

        /// <summary>
        /// Метод для снятия выделения со всех элементов TreeView
        /// </summary>
        private void UnselectAll(TreeView treeView)
        {
            // Перебираем все элементы в TreeView и отменяем выделение
            foreach (var item in treeView.Items)
            {
                var treeViewItem = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = false;
                    UnselectAllItems(treeViewItem); // Рекурсивно для дочерних
                }
            }
        }

        /// <summary>
        /// Рекурсивный метод для снятия выделения с дочерних элементов TreeViewItem
        /// </summary>
        private void UnselectAllItems(TreeViewItem treeViewItem)
        {
            foreach (var child in treeViewItem.Items)
            {
                var childTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                if (childTreeViewItem != null)
                {
                    childTreeViewItem.IsSelected = false;
                    UnselectAllItems(childTreeViewItem);
                }
            }
        }

        /// <summary>
        /// Обработчик для значка настроек
        /// </summary>
        private void SettingsIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Открываем окно настроек
            _viewModel.OpenSettingsCommand.Execute(null);
        }

        /// <summary>
        /// Обработчик для поля поиска при нажатии Enter
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string query = SearchTextBox.Text;
                PerformSearch(query); // Метод для выполнения поиска по введенному тексту
            }
        }

        /// <summary>
        /// Метод для выполнения поиска по введённому запросу
        /// </summary>
        private void PerformSearch(string query)
        {
            var foundFile = _viewModel.Files.FirstOrDefault(file => file.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));
            if (foundFile != null)
            {
                OpenFile(foundFile);
                return;
            }

            string rootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyFolders");
            if (!Directory.Exists(rootDirectory))
            {
                MessageBox.Show("Файл не найден.");
                return;
            }

            var allFiles = Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories)
                                   .Where(path => Path.GetFileName(path).Contains(query, StringComparison.OrdinalIgnoreCase))
                                   .ToList();

            if (allFiles.Any())
            {
                string foundFilePath = allFiles.First();
                var newFileItem = new FileItem
                {
                    FileName = Path.GetFileName(foundFilePath),
                    FilePath = foundFilePath
                };
                OpenFile(newFileItem);
            }
            else
            {
                MessageBox.Show("Файл не найден во всём каталоге.");
            }
        }

        /// <summary>
        /// Метод для открытия файла в PageFrame
        /// </summary>
        private void OpenFile(FileItem file)
        {
            try
            {
                var markdownViewer = new MarkdownViewer(file);
                PageFrame.Content = markdownViewer;
                Debug.WriteLine($"PageFrame Width: {PageFrame.ActualWidth}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
            }
        }
    }
}