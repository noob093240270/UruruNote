using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UruruNote.Models;
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
        public ObservableCollection<FolderItem> Folders { get; set; }
        public ObservableCollection<FileItem> Files { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            // Создаем экземпляр ViewModel
            var viewModel = new MainViewModel();
            DataContext = viewModel; // Установка DataContext на ViewModel

            Folders = new ObservableCollection<FolderItem>();

            // Подписываемся на событие OpenFileRequest
            viewModel.OpenFileRequest += ViewModel_OpenFileRequest;
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
                var markdownViewerPage = new MarkdownViewerPage(file);

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
        // Обработчик для значка настроек (шестеренка)
        private void SettingsIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Открываем окно настроек
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        // Обработчик для сворачивания и разворачивания treeview
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




        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Если поле поиска скрыто, показать его, если оно показано, скрыть его
            SearchTextBox.Visibility = SearchTextBox.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Автоматически устанавливаем фокус на поле поиска при его открытии
            if (SearchTextBox.Visibility == Visibility.Visible)
            {
                SearchTextBox.Focus();
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
            /*
            foreach (var file in viewModel.Files)
            {
                Console.WriteLine($"File in collection: {file.FileName}");
            }*/

            var foundFile = viewModel.Files.FirstOrDefault(file => file.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));

            if (foundFile != null)
            {
                OpenFile(foundFile); // Открываем найденный файл
            }
            else
            {
                MessageBox.Show("Файл не найден.");
            }
        }



        private void OpenFile(FileItem file)
        {
            try
            {
                // Создаем новый экземпляр MarkdownViewer и передаем ему файл для отображения
                var markdownViewer = new MarkdownViewer(file); // MarkdownViewer — это UserControl, который отображает содержимое
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
}