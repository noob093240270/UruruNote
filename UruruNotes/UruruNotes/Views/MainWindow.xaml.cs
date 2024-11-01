using System.Collections.ObjectModel;
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


        

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); // Установка DataContext на ViewModel
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

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

        private void CreateNewFileButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel; // Получаем доступ к ViewModel
            viewModel?.CreateNewMarkdownFileCommand.Execute(null); 
        }











        private MarkdownViewer _currentMarkdownViewer; // Ссылка на текущее открытое окно

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
        }



        private void UnselectAll(TreeView treeView)
        {
            foreach (var item in treeView.Items)
            {
                var treeViewItem = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = false; // Сбрасываем выделение
                                                     // Рекурсивно снимаем выделение с дочерних элементов
                    UnselectChildItems(treeViewItem);
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

    }
}