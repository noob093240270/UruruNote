using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using UruruNote.Models;
using UruruNote.Views;
using UruruNotes.Models;
using UruruNotes.Views;

namespace UruruNote.ViewsModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        /*public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }*/

        public UserSettings userSettings { get; set; }



        #region Settings

        // Добавленное свойство команды для открытия настроек
        private ICommand _openSettingsCommand;
        public ICommand OpenSettingsCommand
        {
            get
            {
                // Инициализируем команду
                return _openSettingsCommand ??= new RelayCommand(OpenSettings);
            }
        }

        // Метод для открытия окна настроек
        private void OpenSettings()
        {
            // Создаем новое окно настроек и показываем его
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
        }

        /*private bool _isDarkModeEanbled;
        public bool IsDarkModeEnabled
        {
            get { return _isDarkModeEanbled; }
            set
            {
                _isDarkModeEanbled = value;
                SwitchTheme();
                userSettings.DarkMode = IsDarkModeEnabled;
                
            }
        }
 
        private void SwitchTheme()
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            
        }*/
        #endregion

        #region TaskList

        private int _selectedTaskListId;
        public object SelectedTreeViewItem { get; set; }

        private ICommand _selectedItemChangedCommand;

        public ICommand SelectedTaskChangedCommand
        {
            get
            {
                if (_selectedItemChangedCommand == null)
                {
                    _selectedItemChangedCommand = new RelayCommand<object>(selectedItem =>
                    {

                        SelectedTreeViewItem = selectedItem;
                    });
                }
                return _selectedItemChangedCommand;
            }
        }

        public void SelectedTreeViewItemLoadTask(object selectedItem)
        {
            var taskListId = (selectedItem as TaskList)?.Id;
            if (taskListId != null)
            {
                _selectedTaskListId = (int)taskListId;
                SelectedTaskListItems.Clear();

            }
        }


        private ObservableCollection<Models.Task> _selectedTaskListItems;
        public ObservableCollection<Models.Task> SelectedTaskListItems
        {
            get
            {
                return _selectedTaskListItems;
            }
            set
            {
                if (_selectedTaskListItems != null)
                {
                    foreach (var item in _selectedTaskListItems)
                    {
                        item.PropertyChanged -= PropertyChanged;
                    }
                }
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        item.PropertyChanged += PropertyChanged;
                    }
                }
                _selectedTaskListItems = value;
                OnPropertyChanged();
            }
        }


        #endregion





        public ObservableCollection<FileItem> Files { get; set; }

        public ICommand CreateNewMarkdownFileCommand { get; }

        public ObservableCollection<FolderItem> Folders { get; set; }

        public ICommand CreateFolderCommand { get; }

        // команда для добавления файла в папку ПКМ

        private ICommand _addFileCommand;
        public ICommand AddFileCommand
        {
            get
            {
                return _addFileCommand ??= new RelayCommand<FolderItem>(AddFile);
            }
        }


        public string RootDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "MyFolders");

        public MainViewModel()
        {
            /*
            var markdownService = new MarkdownFileService();
            string fileName = "initial_file.md";

            IsFileCreated = markdownService.IsMarkdownFileExists(fileName);*/

            Files = new ObservableCollection<FileItem>();
            CreateNewMarkdownFileCommand = new RelayCommand(CreateNewMarkdownFile);
            LoadFiles();

            /*
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "MyFolder");
            Directory.CreateDirectory(folderPath);*/
            if (!Directory.Exists(RootDirectory))
            {
                Directory.CreateDirectory(RootDirectory);
            }
            Folders = new ObservableCollection<FolderItem>();
            CreateFolderCommand = new RelayCommand(CreateFolder);
            //AddFileCommand = new RelayCommand<FolderItem>(AddFile); // Используем параметр типа FolderItem
            LoadFolders();



        }

        #region CreateFileInFolder


        private void AddFile(FolderItem selectedFolder)
        {
            if (selectedFolder == null)
            {
                MessageBox.Show("Не выбрана папка для добавления файла.");
                return;
            }

            // Открываем окно для ввода имени файла
            var newFileWindow = new NewFileWindow();
            if (newFileWindow.ShowDialog() != true)
            {
                return; // Если окно было закрыто
            }

            // Получаем имя нового файла
            string fileName = newFileWindow.FileName + ".md";
            string filePath = Path.Combine(selectedFolder.FilePath, fileName);

            // Проверяем, существует ли файл
            if (File.Exists(filePath))
            {
                MessageBox.Show("Файл уже существует.");
                return;
            }

            try
            {
                // Создаем файл
                var markdownService = new MarkdownFileService();
                markdownService.CreateMarkdownFile(filePath);

                // Добавляем файл в коллекцию
                selectedFolder.Files.Add(new FileItem
                {
                    FileName = fileName,
                    FilePath = filePath
                });

                MessageBox.Show("Файл успешно создан: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании файла: {ex.Message}");
            }
        }



        #endregion


        #region CreateFolder

        private void CreateFolder()
        {
            var newFolderWindow = new NewFolderWindow();
            if (newFolderWindow.ShowDialog() == true)
            {
                var folderName = newFolderWindow.FolderName;

                // Создаем папку на диске
                var folderPath = Path.Combine(RootDirectory, folderName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Добавляем папку в коллекцию
                var newFolder = new FolderItem { FileName = folderName, FilePath = folderPath };
                Folders.Add(newFolder);
            }
        }


        // Метод для загрузки структуры папок из файловой системы
        private void LoadFolders()
        {
            Folders.Clear();
            var rootFolders = LoadFoldersRecursively(RootDirectory);
            foreach (var folder in rootFolders)
            {
                Folders.Add(folder);
            }
        }

        // Рекурсивный метод для загрузки папок
        private ObservableCollection<FolderItem> LoadFoldersRecursively(string directoryPath)
        {
            var folders = new ObservableCollection<FolderItem>();

            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                var folder = new FolderItem
                {
                    FileName = Path.GetFileName(dir),
                    FilePath = dir,
                    SubFolders = LoadFoldersRecursively(dir) // Загружаем вложенные папки
                };
                folders.Add(folder);
            }

            return folders;
        }


        #endregion



        #region CreateMdFile


        private bool _isFileCreated;

        public bool IsFileCreated
        {
            get => _isFileCreated;
            set
            {
                _isFileCreated = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCreateButtonVisible));
            }
        }

        public bool IsCreateButtonVisible => !IsFileCreated;

        private ICommand _createInitialMarkdownFileCommand;
        public ICommand CreateInitialMarkdownFileCommand
        {
            get
            {
                return _createInitialMarkdownFileCommand ??= new RelayCommand(CreateInitialMarkdownFile);
            }
        }

        private void CreateInitialMarkdownFile()
        {
            var markdownService = new MarkdownFileService();
            string fileName = "initial_file.md";

            if (markdownService.IsMarkdownFileExists(fileName))
            {
                MessageBox.Show("Файл уже был создан ранее.");
                return;
            }

            try
            {
                string filePath = markdownService.CreateMarkdownFile(fileName);
                IsFileCreated = true; // Устанавливаем флаг, что файл создан
                LoadFiles(); // Загрузите файлы после создания
                MessageBox.Show("Файл успешно создан: " + filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void CreateNewMarkdownFile()
        {
            NewFileWindow newFileWindow = new NewFileWindow();

            // Подписка на событие FileCreated
            newFileWindow.FileCreated += (filePath) =>
            {
                // Добавляем новый файл в коллекцию
                Files.Add(new FileItem
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath
                });
            };

            newFileWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoadFilesCommand { get; }

        public void LoadFiles()
        {
            string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MarkdownFiles");

            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath)
                    .Select(filePath => new FileItem
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath
                    });

                Files.Clear();
                foreach (var file in files)
                {
                    Files.Add(file);
                }
            }
            else
            {
                MessageBox.Show("Папка не найдена: " + directoryPath);
            }
        }

        private object _selectedItem;
        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(); // уведомление о смене значения
                }
            }
        }

        #endregion

        public event Action<UserControl> OpenFileRequest;

        public void SelectedTreeViewItemChanged(FileItem fileItem)
        {
            // Проверяем, что fileItem не равен null
            if (fileItem == null)
            {
                return; // Игнорируем, если выбранный элемент null
            }

            // Создаем новый объект MarkdownViewer
            var markdownViewer = new MarkdownViewer(fileItem);

            // Если есть подписчики на событие, вызываем его
            OpenFileRequest?.Invoke(markdownViewer);  // Передаем UserControl (MarkdownViewer)
        }


    }
}