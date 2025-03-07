using System;
using System.Collections.ObjectModel;
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
using UruruNotes.ViewsModels;

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
        public ObservableCollection<FolderItem> Folders { get; set; }
        public ObservableCollection<FileItem> Files { get; set; }

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

        private Point _startPoint;

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

            if (TreeViewGrid.Visibility == Visibility.Visible)
            {
                TreeViewGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                TreeViewGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке создания нового файла
        /// </summary>
        private void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel; // Получаем доступ к ViewModel
            viewModel?.CreateNewMarkdownFileCommand.Execute(null);
        }

        /// <summary>
        /// Обработчик клика по кнопке создания новой папки
        /// </summary>
        private void CreateNewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            viewModel?.CreateFolderCommand.Execute(null);
        }

        /// <summary>
        /// Обработчик для добавления файла в папку ПКМ
        /// </summary>
        private void AddFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is FolderItem selectedFolder)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.AddFileCommand.Execute(selectedFolder);
                }
            }
        }

        private MarkdownViewer _currentMarkdownViewer; // Ссылка на текущее открытое окно

        /// <summary>
        /// Обработчик для предварительного клика мышью по TreeView (снятие выделения)
        /// </summary>
        
       

        /// <summary>
        /// Обработчик изменения выбранного элемента в TreeView
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                var selectedItem = e.NewValue as FileItem;
                if (selectedItem != null)
                {
                    viewModel.SelectedTreeViewItemChanged(selectedItem); // Передаем выбранный элемент

                    // Очищаем выделение
                    var treeView = sender as TreeView;
                    if (treeView != null)
                    {
                        UnselectAll(treeView); // Используем наш метод для снятия выделения
                    }

                    // Загружаем содержимое выбранного файла в правую часть окна
                    OpenFile(selectedItem);
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
            var viewModel = DataContext as MainViewModel;

            var foundFile = viewModel.Files.FirstOrDefault(file => file.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));

            if (foundFile != null)
            {
                OpenFile(foundFile); return;
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
                // Создаем новый экземпляр MarkdownViewer и передаем ему файл для отображения
                var markdownViewer = new MarkdownViewer(file);// MarkdownViewer — это UserControl, который отображает содержимое
                _currentMarkdownViewer = markdownViewer;
                PageFrame.Content = markdownViewer; // Загружаем этот UserControl в PageFrame
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
            }
        }

        private void PageFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }


        //рома добавил снизу

        // Для папок
        private void FoldersTreeView_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var selectedFolder = (sender as TreeView).SelectedItem;
            if (selectedFolder is FolderItem folderItem)
            {
                // Логика работы с выбранной папкой
                MessageBox.Show("Выбрана папка: " + folderItem.FileName);
            }
        }

        // Для файлов
        private void FilesTreeView_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            var selectedFile = (sender as TreeView).SelectedItem;
            if (selectedFile is FileItem fileItem)
            {
                // Логика работы с выбранным файлом
                MessageBox.Show("Выбран файл: " + fileItem.FileName);
            }
        }
        private void DeleteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is FolderItem folderItem)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.DeleteFolder(folderItem); // вызов метода для удаления папки
                }
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем объект FileItem из CommandParameter
            var fileItem = (sender as MenuItem)?.CommandParameter as FileItem;

            if (fileItem != null)
            {
                // Вызов метода удаления файла
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.DeleteFile(fileItem); // Метод для удаления файла
                }
            }
            else
            {
                MessageBox.Show("Файл не найден");
            }
        }


        private void TreeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                var dataObject = new DataObject("myFormat", treeViewItem.DataContext);
                DragDrop.DoDragDrop(treeViewItem, dataObject, DragDropEffects.Move);
            }
        }

        private static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as TreeViewItem;
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            var targetFolder = (sender as TreeView).SelectedItem as FolderItem;

            if (e.Data.GetDataPresent("FileItem"))
            {
                var droppedFile = e.Data.GetData("FileItem") as FileItem;
                viewModel?.DropItem(targetFolder, droppedFile);
            }
            else if (e.Data.GetDataPresent("FolderItem"))
            {
                var droppedFolder = e.Data.GetData("FolderItem") as FolderItem;
                viewModel?.DropItem(targetFolder, droppedFolder);
            }
        }

        private void TreeView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("myFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
        }


        private void MoveToFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.DataContext is FileItem fileItem)
            {
                ShowMoveToFolderDialog(fileItem);
            }
            else if (menuItem?.DataContext is FolderItem folderItem)
            {
                ShowMoveToFolderDialog(folderItem);
            }
        }

        private void ShowMoveToFolderDialog(object itemToMove)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null) return;

            var folders = viewModel.Folders; // Получаем список папок из ViewModel

            // Создаем диалоговое окно для выбора папки
            var dialog = new SelectFolderDialog(folders);
            if (dialog.ShowDialog() == true)
            {
                if (dialog.MoveToRoot)
                {
                    // Перемещаем в папку MyFolders
                    viewModel.DropItem(null, itemToMove);
                }
                else if (dialog.SelectedFolder != null)
                {
                    // Перемещаем в выбранную папку
                    viewModel.DropItem(dialog.SelectedFolder, itemToMove);
                }
            }
        }
    }
}