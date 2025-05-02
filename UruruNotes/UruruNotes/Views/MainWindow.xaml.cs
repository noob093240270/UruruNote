using System.Collections.ObjectModel;
using System.IO;
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
        public ObservableCollection<FolderItem> Folders { get; set; }
        public ObservableCollection<FileItem> Files { get; set; }
        public CalendarViewModel CalendarViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            // Создаем экземпляр ViewModel
            var viewModel = new MainViewModel();
            DataContext = viewModel; // Установка DataContext на ViewModel

            Folders = new ObservableCollection<FolderItem>();

            // Подписываемся на событие OpenFileRequest
            viewModel.OpenFileRequest += ViewModel_OpenFileRequest;

            CalendarViewModel = new CalendarViewModel();
        }

        private void ViewModel_OpenFileRequest(UserControl userControl)
        {
            // Заменяем контент в PageFrame
            PageFrame.Content = userControl;
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{

        //}

        /*private void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            NewFileWindow newFileWindow = new NewFileWindow();
            if (newFileWindow.ShowDialog() == true)
            {
                string fileName = newFileWindow.FileName;

                // Создаем новый файл
                var markdownService = new MarkdownFileService();
                string newFileName = fileName.Trim() + ".md";

                // Проверяем, существует ли файл с таким именем
                if (!markdownService.IsMarkdownFileExists(newFileName))
                {
                    // Если файл не существует, создаем его
                    string filePath = markdownService.CreateMarkdownFile(newFileName);

                    // Получаем доступ к ViewModel через DataContext
                    var viewModel = DataContext as MainViewModel;

                    if (viewModel != null)
                    {
                        // Добавляем файл в коллекцию Files для обновления UI
                        viewModel.Files.Add(new FileItem { FileName = newFileName, FilePath = filePath });
                    }

                    MessageBox.Show($"Файл успешно создан: {filePath}");
                }
            }
        }*/

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Получаем элемент, по которому кликнули
            var clickedElement = e.OriginalSource as DependencyObject;

            // Ищем TreeViewItem, к которому относится клик
            while (clickedElement != null && !(clickedElement is TreeViewItem))
            {
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }

            if (clickedElement is TreeViewItem treeViewItem)
            {
                // Сбрасываем выделение, если оно было
                if (treeViewItem.IsSelected)
                {
                    treeViewItem.IsSelected = false; // Убираем выделение
                    e.Handled = true;               // Останавливаем дальнейшую обработку события
                }
            }
        }




<<<<<<< Updated upstream
=======
        /// <summary>
        /// Oбработчик кнопки сворачивания/разворачивания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            var openAnimation = (Storyboard)FindResource("OpeningLeftMenu");
            var closeAnimation = (Storyboard)FindResource("ClosingLeftMenu");

            if (isMenuOpen)
            {
                closeAnimation.Begin();
                ButtonClose.Visibility = Visibility.Collapsed;
                ButtonOpen.Visibility = Visibility.Visible;
            }
            else
            {
                openAnimation.Begin();
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOpen.Visibility = Visibility.Collapsed;
            }

            isMenuOpen = !isMenuOpen;

            // Восстанавливаем выделение после анимации
            Dispatcher.BeginInvoke(new Action(() =>
            {
                RestoreSelection();
            }), System.Windows.Threading.DispatcherPriority.Background);

            e.Handled = true;
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            RestoreSelection();
        }

        private void TreeViewItem_Collapsed(object sender, RoutedEventArgs e)
        {
            if (_lastSelectedFile == null) return;

            if (sender is TreeViewItem collapsedItem && collapsedItem.DataContext is FolderItem collapsedFolder)
            {
                if (collapsedFolder.Files.Contains(_lastSelectedFile) || collapsedFolder.SubFolders.Any(f => f.Files.Contains(_lastSelectedFile)))
                {
                    Debug.WriteLine("Файл находится внутри свернутой папки, выделение не восстанавливается.");
                    return;
                }
            }

            RestoreSelection();
        }


        /// <summary>
        /// Обработчик клика по кнопке создания нового файла
        /// </summary>
>>>>>>> Stashed changes
        private void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel; // Получаем доступ к ViewModel
            viewModel?.CreateNewMarkdownFileCommand.Execute(null);
        }


        private void CreateNewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            viewModel?.CreateFolderCommand.Execute(null);
        }


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


        /*private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                var selectedItem = e.NewValue as FileItem;
                viewModel.SelectedTreeViewItemChanged(selectedItem); // Передаем выбранный элемент

                // Очищаем выделение
                var treeView = sender as TreeView;
                if (treeView != null)
                {
                    UnselectAll(treeView); // Используем наш метод для снятия выделения
                }
            }
        }*/

<<<<<<< Updated upstream
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
=======
        /// <summary>
        /// Обработчик изменения выбранного элемента в TreeView
        /// </summary>
        /*private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Игнорируем события, когда выделение меняется программно
            if (_isProgrammaticSelection) return;

            if (e.NewValue is FileItem selectedFile)
            {
                // Если кликнули на уже открытый файл - ничего не делаем
                if (selectedFile == _lastOpenedFile) return;

                OpenFile(selectedFile);
                RestoreSelection();
            }
            else if (e.NewValue is FolderItem)
            {
                // Снимаем выделение с папки
                var treeView = sender as TreeView;
                UnselectAll(treeView);
            }
        }*/
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isProgrammaticSelection) return;

            if (e.NewValue is FileItem selectedFile)
            {
                if (selectedFile == _lastOpenedFile) return;

                OpenFile(selectedFile);
                _lastOpenedFile = selectedFile;
                RestoreSelection();
            }
            // Папку трогать не нужно, просто ничего не делаем
        }
        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject);
            if (treeViewItem != null)
            {
                treeViewItem.Focus(); // вручную устанавливаем фокус
                e.Handled = true; // предотвращаем автоматическое выделение
            }
        }
        private static T VisualUpwardSearch<T>(DependencyObject source) where T : DependencyObject
        {
            while (source != null && !(source is T))
                source = VisualTreeHelper.GetParent(source);

            return source as T;
        }


        /// <summary>
        /// Метод для снятия выделения со всех элементов TreeView
        /// </summary>
        private void UnselectAll(TreeView treeView)
        {
            if (treeView == null) return;


            // Очищаем все дочерние элементы
            foreach (var item in treeView.Items)
            {
                var treeViewItem = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = false;
                    UnselectChildItems(treeViewItem);
                }
            }
        }

        private void UnselectChildItems(TreeViewItem parentItem)
        {
            foreach (var item in parentItem.Items)
            {
                var treeViewItem = parentItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = false;
                    UnselectChildItems(treeViewItem);
                }
            }
        }

        /// <summary>
        /// Рекурсивный метод для снятия выделения с дочерних элементов TreeViewItem
        /// </summary>
        private void UnselectAllItems(TreeViewItem parentItem)
        {
            foreach (var item in parentItem.Items)
            {
                var treeViewItem = parentItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = false;
                    UnselectAllItems(treeViewItem);
                }
            }
        }

        private void UnselectAllTreeViews()
        {
            UnselectAll(FoldersTreeView);
            UnselectAll(FilesTreeView);
        }

        // Метод для выделения файла в дереве
        public void SelectFileInTree(FileItem file)
        {
            if (file == null || file == _currentlyOpenedFile) return;

            try
            {
                _isProgrammaticSelection = true;
                _currentlyOpenedFile = file;

                // Снимаем выделение со всех TreeView
                UnselectAllTreeViews();

                // Ищем и выделяем нужный файл
                var itemToSelect = FindTreeViewItem(FoldersTreeView, file) ??
                                 FindTreeViewItem(FilesTreeView, file);

                if (itemToSelect != null)
                {
                    itemToSelect.IsSelected = true;
                    itemToSelect.BringIntoView();
                }
            }
            finally
            {
                _isProgrammaticSelection = false;
            }
        }

        /// <summary>
        /// Выделяет файл в дереве без вызова события SelectedItemChanged
        /// </summary>
        public void SelectFileSilently(FileItem file)
        {
            try
            {
                _isInternalSelection = true;

                // Ищем элемент в обоих TreeView
                var itemToSelect = FindTreeViewItem(FoldersTreeView, file) ?? FindTreeViewItem(FilesTreeView, file);

                if (itemToSelect != null)
                {
                    itemToSelect.IsSelected = true;
                    itemToSelect.BringIntoView();
                }
            }
            finally
            {
                _isInternalSelection = false;
            }
        }

        // Вспомогательный метод для поиска элемента в TreeView
        private TreeViewItem FindTreeViewItem(ItemsControl parent, object itemToFind)
        {
            if (parent == null || itemToFind == null) return null;

            // Принудительно генерируем контейнеры для видимых элементов
            parent.UpdateLayout();

            foreach (var item in parent.Items)
            {
                var container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container == null)
                {
                    // Если контейнер не создан, пытаемся сгенерировать его
                    container = parent.ItemContainerGenerator.ContainerFromIndex(parent.Items.IndexOf(item)) as TreeViewItem;
                    if (container == null) continue;
                }

                container.Expanded -= TreeViewItem_Expanded;
                container.Expanded += TreeViewItem_Expanded;
                container.Collapsed -= TreeViewItem_Collapsed;
                container.Collapsed += TreeViewItem_Collapsed;

                if (item == itemToFind)
                {
                    // Разворачиваем всю цепочку родителей
                    ExpandParentChain(container);
                    return container;
                }

                var found = FindTreeViewItem(container, itemToFind);
                if (found != null)
                {
                    ExpandParentChain(container);
                    return found;
                }
            }
            return null;
        }

        private void ExpandParentChain(TreeViewItem item)
        {
            var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent.UpdateLayout(); // Важно для виртуализированных деревьев
                parent = ItemsControl.ItemsControlFromItemContainer(parent) as TreeViewItem;
            }
        }


        private void SelectSingleFile(FileItem file)
        {
            if (file == null) return;

            // Ищем файл в обоих TreeView
            var treeViews = new[] { FoldersTreeView, FilesTreeView };

            foreach (var treeView in treeViews)
            {
                if (treeView == null) continue;

                var item = FindTreeViewItem(treeView, file);
                if (item != null)
                {
                    item.IsSelected = true;
                    item.BringIntoView();
                    item.Focus(); // Добавляем фокус
                    break;
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
>>>>>>> Stashed changes
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



        private void OpenFileInPageFrame(FileItem file)
        {
            try
            {
                // Создаём экземпляр страницы для просмотра Markdown
                var markdownViewerPage = new MarkdownViewer(file);

                // Устанавливаем страницу в PageFrame
                PageFrame.Content = markdownViewerPage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
            }
        }


        private void UnselectAll(TreeView treeView)
        {
            // Перебираем все элементы в TreeView и отменяем выделение
            foreach (var item in treeView.Items)
            {
                if (item is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsSelected = false;
                    UnselectAllItems(treeViewItem);
                }
            }
        }

        private void UnselectAllItems(TreeViewItem treeViewItem)
        {
            // Рекурсивно отменяем выделение всех дочерних элементов
            foreach (var child in treeViewItem.Items)
            {
                if (child is TreeViewItem childTreeViewItem)
                {
                    childTreeViewItem.IsSelected = false;
                    UnselectAllItems(childTreeViewItem);
                }
            }
        }


        private void UnselectChildItems(TreeViewItem item)
        {
            foreach (var child in item.Items)
            {
                var childItem = item.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                if (childItem != null)
                {
                    childItem.IsSelected = false; // Сбрасываем выделение
                    UnselectChildItems(childItem); // Рекурсивно для дочерних
                }
            }
        }
        // Обработчик для значка настроек
        private void SettingsIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {

            // Открываем окно настроек
            if (DataContext is MainViewModel mainViewModel)
            {
                // Передаем MainViewModel в конструктор SettingsWindow
                var settingsWindow = new SettingsWindow(mainViewModel, _currentMarkdownViewer);
                settingsWindow.ShowDialog();
            }
        }

        // Обработчик для сворачивания и разворачивания treeview
        private void ToggleVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if ((TreeViewGrid.Visibility == Visibility.Visible) & (ButtonOpen.Visibility == Visibility.Collapsed))
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

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string query = SearchTextBox.Text;
                PerformSearch(query); // Метод для выполнения поиска по введенному тексту
            }
        }

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


        // Страница с календарём

        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = new CalendarPage();
        }
<<<<<<< Updated upstream

        // Начальная страница
        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = new HomePage();
        }

    }
=======
        private void AddSubFolderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.CommandParameter is FolderItem parentFolder)
            {
                var newFolderWindow = new NewFolderWindow();
                if (newFolderWindow.ShowDialog() == true)
                {
                    var folderName = newFolderWindow.FolderName;

                    if (parentFolder.SubFolders.Any(f => f.FileName == folderName))
                    {
                        MessageBox.Show("Папка с таким именем уже существует.");
                        return;
                    }

                    var newFolderPath = Path.Combine(parentFolder.FilePath, folderName);
                    Directory.CreateDirectory(newFolderPath);

                    var newFolder = new FolderItem
                    {
                        FileName = folderName,
                        FilePath = newFolderPath
                    };

                    parentFolder.SubFolders.Add(newFolder);
                }
            }
        }



    }

    




    public static class TreeViewExtensions
    {
        
    }


>>>>>>> Stashed changes
}