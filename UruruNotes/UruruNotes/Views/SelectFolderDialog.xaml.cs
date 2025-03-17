using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using UruruNotes.Models;

namespace UruruNotes.Views
{
    public partial class SelectFolderDialog : Window
    {
        public ObservableCollection<FolderItem> Folders { get; set; }
        public FolderItem SelectedFolder
        {
            get { return _selectedFolder; }
            set { _selectedFolder = value; }
        }
        private FolderItem _selectedFolder;

        public bool MoveToRoot { get; set; } // Добавляем свойство для выбора "Без папки"

        public SelectFolderDialog(ObservableCollection<FolderItem> folders)
        {
            InitializeComponent();
            Folders = new ObservableCollection<FolderItem>(); // Создаем новый ObservableCollection
            Folders.Add(new FolderItem { FileName = "Корень" }); // Добавляем фиктивный корневой элемент
            foreach (var folder in folders) // Добавляем остальные папки
            {
                Folders.Add(folder);
            }
            DataContext = this;

            // Устанавливаем выбранный элемент TreeView, если SelectedFolder не null
            if (SelectedFolder != null)
            {
                SelectTreeViewItem(TreeViewFolders, SelectedFolder);
            }
        }

        private void SelectTreeViewItem(ItemsControl parent, FolderItem folder)
        {
            foreach (var item in parent.Items)
            {
                if (item is FolderItem folderItem && folderItem == folder)
                {
                    if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
                    {
                        treeViewItem.IsSelected = true;
                        return;
                    }
                }
                else if (item is FolderItem subFolder && subFolder.SubFolders.Count > 0)
                {
                    if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
                    {
                        SelectTreeViewItem(treeViewItem, folder);
                    }
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный элемент из TreeView
            if (TreeViewFolders.SelectedItem is FolderItem selectedFolder)
            {
                // Проверяем, выбран ли корневой элемент
                if (selectedFolder.FileName == "Корень")
                {
                    MoveToRoot = true; // Устанавливаем флаг, если выбран корень
                    SelectedFolder = null;
                }
                else
                {
                    MoveToRoot = false;
                    SelectedFolder = selectedFolder;
                }
                DialogResult = true;
            }
        }
    }
}