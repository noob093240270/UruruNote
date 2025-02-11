﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private MarkdownViewer _markdownViewer;

        public void SetMarkdownViewer(MarkdownViewer markdownViewer)
        {
            _markdownViewer = markdownViewer;
        }


        public UserSettings userSettings { get; set; }



        #region Settings

        // Команда для открытия настроек
        private ICommand _openSettingsCommand;
        public ICommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ??= new RelayCommand(OpenSettings);
            }
        }

        // Метод для открытия окна настроек
        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }
        // Реализация выбора размера шрифта
        private ObservableCollection<int> _fontSizeOptions;
        public ObservableCollection<int> FontSizeOptions
        {
            get => _fontSizeOptions;
            set
            {
                _fontSizeOptions = value;
                OnPropertyChanged(nameof(FontSizeOptions));
            }
        }
        private int _selectedFontSize = 15;
        private int? _previousFontSize;
        private bool _isUpdatingFontSize = false;
        public int SelectedFontSize
        {
            get => _selectedFontSize;
            set
            {
                if (value >= 10 && value <= 35)
                {
                    if (_previousFontSize != value)
                    {
                        if (_isUpdatingFontSize) return;

                        try
                        {
                            _isUpdatingFontSize = true;

                            _selectedFontSize = value;
                            OnPropertyChanged(nameof(SelectedFontSize));

                            // Обновляем глобальный ресурс
                            App.UpdateGlobalFontSize(value);

                            // Сохраняем новое значение
                            SettingsManager.SaveSettings(value);

                            _previousFontSize = value;
                        }
                        finally
                        {
                            _isUpdatingFontSize = false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Размер шрифта должен быть от 10 до 35.");
                }
            }
        }

        // Выбранный шрифт
        private string _selectedFont;
        public string SelectedFont
        {
            get => _selectedFont;
            set
            {
                if (_selectedFont != value)
                {
                    _selectedFont = value;
                    OnPropertyChanged(nameof(SelectedFont));
                    ApplyFont(); // Применение нового шрифта
                }
            }
        }

        // Применение нового шрифта к приложению
        private void ApplyFont()
        {
            if (!string.IsNullOrEmpty(_selectedFont))
            {
                var app = Application.Current;
                if (app.Resources.Contains("GlobalFont"))
                {
                    app.Resources["GlobalFont"] = new FontFamily(_selectedFont);
                }
                else
                {
                    app.Resources.Add("GlobalFont", new FontFamily(_selectedFont));
                }
                foreach (Window window in Application.Current.Windows)
                {
                    window.FontFamily = new FontFamily(_selectedFont);
                }
            }
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
            // Инициализация списка размеров шрифта

            FontSizeOptions = new ObservableCollection<int>(Enumerable.Range(10, 26));
            SelectedFontSize = SettingsManager.LoadSettings();
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



        #region CreateFileInFolder

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

                // Отладка: Показываем путь перед проверкой
                MessageBox.Show($"Путь файла: {filePath}", "Отладка");


                var markdownService = new MarkdownFileService();

                // Создание файла
                string createdFileName = markdownService.CreateMarkdownFile(filePath);

                // Добавляем файл в список (ObservableCollection автоматически уведомит UI)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Files.Add(new FileItem
                    {
                        FileName = createdFileName, // Используем имя файла, а не полный путь
                        FilePath = filePath
                    });
                });

                // Уведомление пользователя
                MessageBox.Show($"Файл успешно создан: {filePath}");
            };

            newFileWindow.ShowDialog();
        }







        private void CreateInitialMarkdownFile()
        {
            var markdownService = new MarkdownFileService();
            string fileName = "initial_file.md";
            string filePath = Path.Combine(RootDirectory, fileName); // Путь для создания файла в нужной папке

            // Проверка существования файла
            if (File.Exists(filePath))
            {
                MessageBox.Show("Файл уже был создан ранее.");
                return;
            }

            // Создание файла
            markdownService.CreateMarkdownFile(filePath);

            IsFileCreated = true; // Устанавливаем флаг, что файл создан

            // Добавляем файл в список
            Files.Add(new FileItem
            {
                FileName = fileName,
                FilePath = filePath
            });

            MessageBox.Show($"Файл успешно создан: {filePath}");
        }


        private void AddFile(FolderItem selectedFolder)
        {
            // Проверяем, что папка выбрана
            if (selectedFolder == null || string.IsNullOrEmpty(selectedFolder.FilePath))
            {
                MessageBox.Show("Папка не выбрана.");
                return;
            }

            // Убедимся, что папка существует
            if (!Directory.Exists(selectedFolder.FilePath))
            {
                Directory.CreateDirectory(selectedFolder.FilePath); // Если папка не существует, создаем её
            }

            // Открываем окно для ввода имени файла
            var newFileWindow = new NewFileWindow();
            if (newFileWindow.ShowDialog() != true)
            {
                return;
            }

            // Получаем имя файла
            string fileName = newFileWindow.FileName;

            // Используем функцию для создания файла только в подкаталоге
            var markdownService = new MarkdownFileService();
            string createdFileName = markdownService.CreateMarkdownFileInSubfolder(selectedFolder.FilePath, fileName);

            if (createdFileName == null)
            {
                // Файл не был создан, если он уже существует
                return;
            }

            // Добавляем файл в коллекцию папки (не в основную папку)
            selectedFolder.Files.Add(new FileItem
            {
                FileName = createdFileName,
                FilePath = Path.Combine(selectedFolder.FilePath, createdFileName + ".md")
            });

            // Уведомление пользователя
            MessageBox.Show($"Файл успешно создан в папке: {selectedFolder.FilePath}");
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
            var markdownViewer = new MarkdownViewer(fileItem)
            {
                FontSize = SelectedFontSize
            };

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







        #endregion

    }
}