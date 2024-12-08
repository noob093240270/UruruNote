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
using UruruNotes;
using UruruNotes.Models;
using UruruNotes.Views;
using UruruNotes.ViewsModels;

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
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
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

            

            /*
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "MyFolder");
            Directory.CreateDirectory(folderPath);*/

            if (!Directory.Exists(RootDirectory))
            {
                Directory.CreateDirectory(RootDirectory);
            }

            Files = new ObservableCollection<FileItem>();
            CreateNewMarkdownFileCommand = new RelayCommand(CreateNewMarkdownFile);
            LoadFiles();

            Folders = new ObservableCollection<FolderItem>();
            CreateFolderCommand = new RelayCommand(CreateFolder);
            LoadFolders();

            OpenCalendarCommand = new RelayCommand(OpenCalendar);



        }

        #region CreateFileInFolder


        private void AddFile(FolderItem selectedFolder)
        {
            if (selectedFolder == null)
            {
                MessageBox.Show("Не выбрана папка для добавления файла.");
                return;
            }

            var newFileWindow = new NewFileWindow();
            if (newFileWindow.ShowDialog() != true)
            {
                return;
            }

            string fileName = newFileWindow.FileName + ".md";
            string filePath = Path.Combine(selectedFolder.FilePath, fileName);

            // Проверяем уникальность
            if (!IsFileNameUnique(fileName, selectedFolder))
            {
                MessageBox.Show("Файл с таким именем уже существует в этом или другом каталоге.");
                return;
            }

            try
            {
                var markdownService = new MarkdownFileService();
                markdownService.CreateMarkdownFile(filePath);

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

            // Если диалог с названием папки был подтвержден
            if (newFolderWindow.ShowDialog() == true)
            {
                var folderName = newFolderWindow.FolderName;

                // Проверяем уникальность имени папки
                if (!IsFolderNameUnique(folderName))
                {
                    MessageBox.Show("Папка с таким именем уже существует.");
                    return;
                }

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

        private bool IsFolderNameUnique(string folderName)
        {
            return !Folders.Any(f => f.FileName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
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
                    SubFolders = LoadFoldersRecursively(dir), // Загружаем вложенные папки
                    Files = new ObservableCollection<FileItem>(
                        Directory.GetFiles(dir, "*.md").Select(filePath => new FileItem
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath
                        })
                    )
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
            var newFileWindow = new NewFileWindow();

            newFileWindow.FileCreated += (fileName) =>
            {
                // Убираем расширение .md, если оно уже есть
                if (fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = fileName.Substring(0, fileName.Length - 3); // Убираем расширение .md
                }

                string filePath = Path.Combine(RootDirectory, fileName + ".md");

                // Проверяем уникальность
                if (!IsFileNameUnique(fileName))
                {
                    MessageBox.Show("Файл с таким именем уже существует.");
                    return;
                }

                var markdownService = new MarkdownFileService();
                markdownService.CreateMarkdownFile(filePath);
                Files.Add(new FileItem
                {
                    FileName = fileName + ".md",
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
            if (Directory.Exists(RootDirectory))
            {
                var files = Directory.GetFiles(RootDirectory, "*.md")
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
                MessageBox.Show("Папка не найдена: " + RootDirectory);
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

        #region Calendar

        public ICommand OpenCalendarCommand { get; }

        private void OpenCalendar()
        {
            // Создаём и открываем окно календаря
            var calendarWindow = new CalendarPage();
            calendarWindow.ShowDialog(); // Если хотите, чтобы окно было модальным
        }


        private bool IsFileNameUnique(string fileName, FolderItem targetFolder = null)
        {
            // Проверяем файлы в общем каталоге (если targetFolder == null)
            if (targetFolder == null)
            {
                if (Files.Any(file => file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Проверяем файлы во всех папках
                foreach (var folder in Folders)
                {
                    if (!IsFileNameUnique(fileName, folder))
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Проверяем файлы в указанной папке
                if (targetFolder.Files.Any(file => file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                // Рекурсивная проверка в подкаталогах
                foreach (var subFolder in targetFolder.SubFolders)
                {
                    if (!IsFileNameUnique(fileName, subFolder))
                    {
                        return false;
                    }
                }
            }

            return true; // Имя уникально
        }




        #endregion

    }
}