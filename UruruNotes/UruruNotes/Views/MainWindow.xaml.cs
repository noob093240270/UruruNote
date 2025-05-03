using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UruruNote.Views;
using UruruNote.ViewsModels;
using UruruNotes.Models;
using UruruNotes.ViewsModels;

namespace UruruNotes.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window , INotifyPropertyChanged
    {
        private readonly MainViewModel _viewModel;
        private readonly double baseWidth = 800; // Базовая ширина окна
        private readonly double baseHeight = 490; // Базовая высота окна
        public ObservableCollection<FolderItem> Folders { get; set; }
        public ObservableCollection<FileItem> Files { get; set; }

        public MainWindow() 
        {
            InitializeComponent();

            // Предварительная загрузка анимаций
            var openAnimation = (Storyboard)FindResource("OpeningLeftMenu");
            var closeAnimation = (Storyboard)FindResource("ClosingLeftMenu");
            openAnimation.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            closeAnimation.Seek(TimeSpan.Zero, TimeSeekOrigin.BeginTime);

            // Устанавливаем начальное состояние (панель открыта)
            LeftMenu.Width = 200;  // Значение должно совпадать с "To" в анимации открытия
            ButtonClose.Visibility = Visibility.Visible;
            ButtonOpen.Visibility = Visibility.Collapsed;
            isMenuOpen = true;     // Панель открыта
            _isFirstClick = false; // Первый клик не нужен, так как состояние уже корректное

            this.Closing += MainWindow_Closing;

            // Создаем экземпляр ViewModel
            _viewModel = new MainViewModel();
            DataContext = _viewModel; // Установка DataContext на ViewModel

            // Подписываемся на событие OpenFileRequest
            _viewModel.OpenFileRequest += ViewModel_OpenFileRequest;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            UpdateWindowSize(_viewModel.Scale); // Устанавливаем начальные размеры окна с учётом масштаба
            App.ApplyTheme(App.CurrentTheme);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // Отписка от событий
            _viewModel.OpenFileRequest -= ViewModel_OpenFileRequest;
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;

            // Явное завершение приложения
            Application.Current.Shutdown();
        }

        private double _scale = 1.0; // Начальный масштаб

        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged();
                    ScaleTransform.ScaleX = _scale;
                    ScaleTransform.ScaleY = _scale;
                }
            }
        }

        public ScaleTransform ScaleTransform { get; } = new ScaleTransform(1.0, 1.0);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.Scale))
            {
                UpdateWindowSize(_viewModel.Scale); // Обновляем размеры окна
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
        private bool isMenuOpen = false;
        private bool _isFirstClick = true; // Флаг для первого клика

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
            RestoreSelection();
        }

        /// <summary>
        /// Обработчик клика по кнопке создания нового файла
        /// </summary>
        private void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            bool currentTheme = App.CurrentTheme;
            var viewModel = DataContext as MainViewModel;
            viewModel?.CreateNewMarkdownFileCommand.Execute(null);
            App.ApplyTheme(currentTheme);
        }

        /// <summary>
        /// Обработчик клика по кнопке создания новой папки
        /// </summary>
        private void CreateNewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Перед созданием папки, текущая тема: {(App.CurrentTheme ? "Тёмная" : "Светлая")}");
            bool currentTheme = App.CurrentTheme;
            var viewModel = DataContext as MainViewModel;
            viewModel?.CreateFolderCommand.Execute(null);
            Dispatcher.InvokeAsync(() =>
            {
                Debug.WriteLine($"После создания папки, перед восстановлением темы: {(App.CurrentTheme ? "Тёмная" : "Светлая")}");
                App.ApplyTheme(currentTheme);
                Debug.WriteLine($"Тема восстановлена после создания папки: {(currentTheme ? "Тёмная" : "Светлая")}");
            }, System.Windows.Threading.DispatcherPriority.Background);
        }
        

        /// <summary>
        /// Обработчик для добавления файла в папку ПКМ
        /// </summary>
        private void AddFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Перед добавлением файла, текущая тема: {(App.CurrentTheme ? "Тёмная" : "Светлая")}");
            bool currentTheme = App.CurrentTheme;
            if ((sender as MenuItem)?.DataContext is FolderItem selectedFolder)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.AddFileCommand.Execute(selectedFolder);
                }
            }
            Dispatcher.InvokeAsync(() =>
            {
                Debug.WriteLine($"После добавления файла, перед восстановлением темы: {(App.CurrentTheme ? "Тёмная" : "Светлая")}");
                App.ApplyTheme(currentTheme);
                Debug.WriteLine($"Тема восстановлена после добавления файла: {(currentTheme ? "Тёмная" : "Светлая")}");
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private MarkdownViewer _currentMarkdownViewer; // Ссылка на текущее открытое окно

        /// <summary>
        /// Обработчик для предварительного клика мышью по TreeView (снятие выделения)
        /// </summary>

        private FileItem _currentlyOpenedFile; // Текущий открытый файл

        private bool _isProgrammaticSelection = false;
        private FileItem _lastSelectedFile;
        private bool _isInternalSelection = false;
        private FileItem _lastOpenedFile; // Храним последний открытый файл

        /// <summary>
        /// Обработчик изменения выбранного элемента в TreeView
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
        private void SettingsIcon_Click(object sender, RoutedEventArgs e)
        {
            // Передаём _viewModel в SettingsWindow
            var settingsWindow = new SettingsWindow(_viewModel);
            settingsWindow.ShowDialog();
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
            if (viewModel == null) return;

            // Принудительно обновляем привязки TreeView
            FoldersTreeView.Items.Refresh();
            FilesTreeView.Items.Refresh();

            var foundFiles = new List<FileItem>();

            // 1. Поиск в корневых файлах
            var rootFiles = viewModel.Files
                .Where(file => file.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foundFiles.AddRange(rootFiles);

            // 2. Рекурсивный поиск в папках
            foreach (var folder in viewModel.Folders)
            {
                SearchInFolder(folder, query, foundFiles);
            }

            // 3. Сортировка результатов
            var sortedFiles = foundFiles
                .OrderBy(f => f.FileName.Length)
                .ThenBy(f => f.FilePath.Length)
                .ToList();

            if (sortedFiles.Any())
            {
                // Добавляем задержку для гарантированной генерации элементов
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OpenFileFromSearch(sortedFiles.First());
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            else
            {
                MessageBox.Show("Файл не найден.");
            }
        }

        private void OpenFileFromSearch(FileItem file)
        {
            if (file == null) return;

            try
            {
                _isProgrammaticSelection = true;
                _lastSelectedFile = file;

                // 1. Находим родительскую папку
                var parentFolder = FindParentFolder(file);

                // 2. Разворачиваем и выделяем
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (parentFolder != null)
                    {
                        var folderItem = FindTreeViewItem(FoldersTreeView, parentFolder);
                        if (folderItem != null)
                        {
                            folderItem.IsExpanded = true;
                            folderItem.UpdateLayout();
                        }
                    }

                    // Повторяем поиск после разворачивания
                    var fileItem = FindTreeViewItem(FoldersTreeView, file) ??
                                  FindTreeViewItem(FilesTreeView, file);

                    if (fileItem != null)
                    {
                        fileItem.IsSelected = true;
                        fileItem.BringIntoView();
                        fileItem.Focus();
                    }

                    // 3. Открываем файл
                    var markdownViewer = new MarkdownViewer(file);
                    PageFrame.Content = markdownViewer;
                    _lastOpenedFile = file;
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            finally
            {
                _isProgrammaticSelection = false;
            }
        }

        private FolderItem FindParentFolder(FileItem file)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel == null) return null;

            // Проверяем корневые папки
            foreach (var folder in viewModel.Folders)
            {
                var found = FindParentFolderRecursive(folder, file);
                if (found != null) return found;
            }
            return null;
        }

        private FolderItem FindParentFolderRecursive(FolderItem folder, FileItem file)
        {
            // Проверяем файлы в текущей папке
            if (folder.Files.Contains(file)) return folder;

            // Рекурсивно проверяем подпапки
            foreach (var subFolder in folder.SubFolders)
            {
                var found = FindParentFolderRecursive(subFolder, file);
                if (found != null) return found;
            }
            return null;
        }

        private void ExpandParentFolders(FolderItem folder)
        {
            var treeViews = new[] { FoldersTreeView, FilesTreeView };

            foreach (var treeView in treeViews)
            {
                if (treeView == null) continue;

                var item = FindTreeViewItem(treeView, folder);
                if (item != null)
                {
                    // Разворачиваем все родительские элементы
                    var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                    while (parent != null)
                    {
                        parent.IsExpanded = true;
                        parent = ItemsControl.ItemsControlFromItemContainer(parent) as TreeViewItem;
                    }
                    break;
                }
            }
        }

        // Рекурсивный поиск в папках
        private void SearchInFolder(FolderItem folder, string query, List<FileItem> results)
        {
            // Добавляем файлы из текущей папки
            results.AddRange(folder.Files
                .Where(file => file.FileName.Contains(query, StringComparison.OrdinalIgnoreCase)));

            // Рекурсивно проверяем подпапки
            foreach (var subFolder in folder.SubFolders)
            {
                SearchInFolder(subFolder, query, results);
            }
        }

        /// <summary>
        /// Метод для открытия файла в PageFrame
        /// </summary>
        private void OpenFile(FileItem file)
        {
            if (file == null || file == _lastOpenedFile) return;

            try
            {
                _isProgrammaticSelection = true;
                _lastSelectedFile = file; // Сохраняем выделенный файл

                // 1. Снимаем все выделения
                UnselectAllTreeViews();

                // 2. Выделяем нужный файл
                SelectSingleFile(file);

                // 3. Открываем файл
                var markdownViewer = new MarkdownViewer(file);
                PageFrame.Content = markdownViewer;
                _lastOpenedFile = file;
            }
            finally
            {
                _isProgrammaticSelection = false;
            }
        }

        /// <summary>
        /// Mетод для восстановления выделения
        /// </summary>
        private void RestoreSelection()
        {
            if (_lastSelectedFile == null) return;

            Debug.WriteLine($"Восстанавливаем выделение для: {_lastSelectedFile.FileName}");

            // Используем Dispatcher для гарантированного выполнения в UI-потоке
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _isProgrammaticSelection = true;

                    // Сначала снимаем все выделения
                    UnselectAllTreeViews();

                    // Затем выделяем нужный файл
                    SelectSingleFile(_lastSelectedFile);

                    // Убедимся, что элемент видим (разворачиваем родительские папки)
                    EnsureItemVisible(_lastSelectedFile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при восстановлении выделения: {ex.Message}");
                }
                finally
                {
                    _isProgrammaticSelection = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void EnsureItemVisible(FileItem file)
        {
            var treeViews = new[] { FoldersTreeView, FilesTreeView };

            foreach (var treeView in treeViews)
            {
                if (treeView == null) continue;

                var item = FindTreeViewItem(treeView, file);
                if (item != null)
                {
                    // Разворачиваем все родительские элементы
                    var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                    while (parent != null)
                    {
                        parent.IsExpanded = true;
                        parent = ItemsControl.ItemsControlFromItemContainer(parent) as TreeViewItem;
                    }

                    item.BringIntoView();
                    break;
                }
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
            bool currentTheme = App.CurrentTheme;
            if ((sender as MenuItem)?.DataContext is FolderItem folderItem)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    viewModel.DeleteFolder(folderItem); // вызов метода для удаления папки
                }
            }
            App.ApplyTheme(currentTheme);
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            bool currentTheme = App.CurrentTheme;
            // Получаем объект FileItem из CommandParameter
            var fileItem = (sender as MenuItem)?.CommandParameter as FileItem;

            if (fileItem != null)
            {
                if (DataContext is MainViewModel viewModel)
                {
                    // Удаляем файл из коллекции папки
                    var parentFolder = fileItem.ParentFolder;
                    if (parentFolder != null)
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

        // Начальная страница
        private void HomePageButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = new HomePage();
        }

    }
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
                        parentFolder.Files.Remove(fileItem); // Удаляем файл из папки
                    }

                    // Вызов метода для удаления файла в модели данных
                    viewModel.DeleteFile(fileItem); // Удаляем файл из основной коллекции

                    // Перезагружаем данные
                    viewModel.LoadFileStructure(); // Обновляем все данные, как при перезапуске

                    // Принудительно обновляем UI
                    Dispatcher.Invoke(() =>
                    {
                        // Обновляем коллекции в UI
                        FoldersTreeView.ItemsSource = null;
                        FilesTreeView.ItemsSource = null;

                        // Устанавливаем новые источники данных
                        FoldersTreeView.ItemsSource = viewModel.Folders;
                        FilesTreeView.ItemsSource = viewModel.Files;

                        // Принудительно обновляем отображение
                        FoldersTreeView.Items.Refresh();
                        FilesTreeView.Items.Refresh();
                    });
                }
            }
            else
            {
                MessageBox.Show("Файл не найден");
            }
            App.ApplyTheme(currentTheme);
        }










        private Point _startPoint;
        private bool _isDragging;

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Запоминаем точку начала клика
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void TreeView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging)
                return;

            var currentPoint = e.GetPosition(null);
            var diff = _startPoint - currentPoint;

            // Начинаем перетаскивание только если мышь переместилась достаточно
            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDragging = true;
                var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);
                if (treeViewItem != null)
                {
                    var dataObject = new DataObject("myFormat", treeViewItem.DataContext);
                    DragDrop.DoDragDrop(treeViewItem, dataObject, DragDropEffects.Move);
                }
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
            bool currentTheme = App.CurrentTheme;
            var menuItem = sender as MenuItem;
            if (menuItem?.DataContext is FileItem fileItem)
            {
                ShowMoveToFolderDialog(fileItem);
            }
            else if (menuItem?.DataContext is FolderItem folderItem)
            {
                ShowMoveToFolderDialog(folderItem);
            }
            App.ApplyTheme(currentTheme);
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

    public static class TreeViewExtensions
    {
        
    }


}