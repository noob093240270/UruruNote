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
using GalaSoft.MvvmLight.Command;
using UruruNote.Models;
using UruruNote.Views;
using UruruNotes;
using UruruNotes.Models;
using UruruNotes.Views;
using UruruNotes.ViewsModels;


namespace UruruNote.ViewsModels
{
    public class MainViewModel : INotifyPropertyChanged
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
        private bool _isUpdatingFontSize = true;
        public int SelectedFontSize
        {
            get => _selectedFontSize;
            set
            {
                if (value >= 10 && value <= 35)
                {
                    if (_selectedFontSize != value) // Изменил условие, чтобы всегда сохранять
                    {
                        _selectedFontSize = value;
                        Debug.WriteLine($"SelectedFontSize изменён на: {value}");
                        OnPropertyChanged(nameof(SelectedFontSize));
                        ApplyFont();
                        App.UpdateGlobalFontSize(value);
                        SettingsManager.SaveSettings(value, Scale, IsNotificationsEnabled); // Сохраняем всегда
                        if (!_isUpdatingFontSize)
                        {
                            Debug.WriteLine($"FontSize notification: {value}");

                        }
                        _previousFontSize = value;
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
        public void ApplyFont()
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
        private double _scale = 1.0; // Начальный масштаб (100%)
        private double? _previousScale;
        private bool _isInitializing = true;
        public double Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    Debug.WriteLine($"Scale изменён на: {value}");
                    OnPropertyChanged(nameof(Scale));
                    UpdateScale();
                    SettingsManager.SaveSettings(SelectedFontSize, value, IsNotificationsEnabled); // Сохраняем при каждом изменении
                    if (!_isInitializing)
                    {
                        //ShowScaleNotification(value);
                        UpdateSelectedScaleOption();
                    }
                    ScaleDisplay = $"{value * 100:F0}%";
                    _previousScale = value;
                }
            }
        }
        private bool _isNotificationsEnabled;
        public bool IsNotificationsEnabled
        {
            get => _isNotificationsEnabled;
            set
            {
                if (_isNotificationsEnabled != value)
                {
                    _isNotificationsEnabled = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"ПРИМЕНЕНИЕ НАСТРОЕК ---------------------------------{_isNotificationsEnabled}");
                    SettingsManager.SaveSettings(SelectedFontSize, Scale, _isNotificationsEnabled);
                }
            }
        }
        public void UpdateScale()
        {
            foreach (Window window in Application.Current.Windows)
            {
                var scaleTransform = new ScaleTransform(Scale, Scale);
                window.LayoutTransform = scaleTransform;
                Debug.WriteLine($"Scale применён к окну: {window.GetType().Name}, Scale: {Scale}");
            }
        }
        public void SetScaleFromInput(string input)
        {
            if (double.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double scale))
            {
                if (scale > 2.0 && scale <= 200.0) // Предполагаем, что пользователь ввёл проценты
                {
                    scale /= 100.0;
                }

                if (scale >= 0.5 && scale <= 2.0)
                {
                    Scale = scale; // Уведомление в Scale setter
                }
                else
                {
                    MessageBox.Show("Масштаб должен быть от 50% до 200% (0.5–2.0).");
                }
            }
            else
            {
                MessageBox.Show("Введите числовое значение (например, 50 или 0.5).");
            }
        }
        private void UpdateSelectedScaleOption()
        {
            if (ScaleOptions == null || !ScaleOptions.Any()) return;
            var closestScale = ScaleOptions.OrderBy(option => Math.Abs(option - Scale)).First();
            if (_selectedScaleOption != closestScale) // Избегаем лишнего вызова Scale setter
            {
                _selectedScaleOption = closestScale;
                OnPropertyChanged(nameof(SelectedScaleOption));
            }
        }

        private double _selectedScaleOption;
        public double SelectedScaleOption
        {
            get => _selectedScaleOption;
            set
            {
                if (value > 0 && ScaleOptions.Contains(value))
                {
                    _selectedScaleOption = value;
                    Console.WriteLine($"SelectedScaleOption изменён на: {value}");
                    Scale = value;
                    OnPropertyChanged(nameof(SelectedScaleOption));
                    OnPropertyChanged(nameof(ScaleDisplay));
                }
            }
        }
        private ObservableCollection<double> _scaleOptions;
        public ObservableCollection<double> ScaleOptions
        {
            get => _scaleOptions;
            set
            {
                _scaleOptions = value;
                OnPropertyChanged(nameof(ScaleOptions));
            }
        }


        // Команды для увеличения/уменьшения масштаба
        private ICommand _increaseScaleCommand;
        public ICommand IncreaseScaleCommand
        {
            get
            {
                return _increaseScaleCommand ??= new RelayCommand(IncreaseScale);
            }
        }

        private ICommand _decreaseScaleCommand;
        public ICommand DecreaseScaleCommand
        {
            get
            {
                return _decreaseScaleCommand ??= new RelayCommand(DecreaseScale);
            }
        }

        private void IncreaseScale()
        {
            var closestScale = ScaleOptions.OrderBy(option => Math.Abs(option - Scale)).First();
            var currentScaleIndex = ScaleOptions.IndexOf(closestScale);
            if (currentScaleIndex < ScaleOptions.Count - 1)
            {
                var newScale = ScaleOptions[currentScaleIndex + 1];
                Scale = newScale;
                SelectedScaleOption = newScale; // Синхронизируем SelectedScaleOption
               
            }
        }

        private void DecreaseScale()
        {
            var closestScale = ScaleOptions.OrderBy(option => Math.Abs(option - Scale)).First();
            var currentScaleIndex = ScaleOptions.IndexOf(closestScale);
            if (currentScaleIndex > 0)
            {
                var newScale = ScaleOptions[currentScaleIndex - 1];
                Scale = newScale;
                SelectedScaleOption = newScale; // Синхронизируем SelectedScaleOption
               
            }
        }
        private string _scaleDisplay = "100%";
        public string ScaleDisplay
        {
            get => _scaleDisplay;
            set
            {
                if (_scaleDisplay != value)
                {
                    _scaleDisplay = value;
                    OnPropertyChanged(nameof(ScaleDisplay));
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

        private ICommand _deleteFolderCommand;
        public ICommand DeleteFolderCommand
        {
            get
            {
                return _deleteFolderCommand ??= new RelayCommand<FolderItem>(DeleteFolder);
            }
        }

        private ICommand _deleteFileCommand;
        public ICommand DeleteFileCommand
        {
            get
            {
                return _deleteFileCommand ??= new RelayCommand<FileItem>(DeleteFile);
            }
        }


        public string RootDirectory { get; } = Path.Combine(Directory.GetCurrentDirectory(), "MyFolders");



       
        public MainViewModel()
        {

            FontSizeOptions = new ObservableCollection<int>(Enumerable.Range(10, 26));
            ScaleOptions = new ObservableCollection<double> { 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0 };
            var settings = SettingsManager.LoadSettings();

            _isUpdatingFontSize = true;
            _selectedFontSize = settings.FontSize; // Устанавливаем начальное значение напрямую
            _previousFontSize = settings.FontSize; // Синхронизируем _previousFontSize
            OnPropertyChanged(nameof(SelectedFontSize));
            App.UpdateGlobalFontSize(_selectedFontSize);
            ApplyFont();
            _isUpdatingFontSize = false;

            _isInitializing = true;
            Scale = settings.Scale;
            SelectedScaleOption = settings.Scale;
            _isInitializing = false;
            IsNotificationsEnabled = settings.IsNotificationsEnabled ?? false;

            // Явно вызываем уведомления для начальной синхронизации
            OnPropertyChanged(nameof(Scale));
            OnPropertyChanged(nameof(SelectedScaleOption));
            OnPropertyChanged(nameof(ScaleDisplay));

            OpenCalendarCommand = new RelayCommand(OpenCalendar);
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

            /*
            Files = new ObservableCollection<FileItem>();
            CreateNewMarkdownFileCommand = new RelayCommand(CreateNewMarkdownFile);
            LoadFiles();

            Folders = new ObservableCollection<FolderItem>();
            CreateFolderCommand = new RelayCommand(CreateFolder);
            LoadFolders();

            OpenCalendarCommand = new RelayCommand(OpenCalendar);*/
            Files = new ObservableCollection<FileItem>();
            Folders = new ObservableCollection<FolderItem>();

            CreateNewMarkdownFileCommand = new RelayCommand(CreateNewMarkdownFile);
            CreateFolderCommand = new RelayCommand(CreateFolder);
            OpenCalendarCommand = new RelayCommand(OpenCalendar);

            LoadFileStructure(); // Загружаем сразу и папки, и файлы в одной структуре


            // Обработка глобального сохранения шрифта при изменении значения
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SelectedFontSize))
                {
                    SettingsManager.SaveSettings(SelectedFontSize, SelectedScaleOption, IsNotificationsEnabled);
                }
            };
        }

        #region CreateFileInFolder



        #endregion


        #region CreateFolder

        public void CreateFolder()
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
        // Рекурсивный метод для загрузки папок и файлов в них
        private ObservableCollection<FolderItem> LoadFoldersRecursively(string directoryPath)
        {
            Debug.WriteLine($"Читаем папку: {directoryPath}");

            var folders = new ObservableCollection<FolderItem>();

            foreach (var dir in Directory.GetDirectories(directoryPath))
            {
                string folderName = Path.GetFileName(dir);

                // Проверяем, нет ли уже папки с таким именем
                if (folders.Any(f => f.FileName == folderName))
                {
                    Debug.WriteLine($"Пропускаем дублирующуюся папку: {folderName}");
                    continue; // Пропускаем дубликаты
                }

                var folder = new FolderItem
                {
                    FileName = folderName,
                    FilePath = dir,
                    SubFolders = LoadFoldersRecursively(dir),
                    Files = new ObservableCollection<FileItem>(
                        Directory.GetFiles(dir, "*.md").Select(filePath => new FileItem
                        {
                            FileName = Path.GetFileName(filePath),
                            FilePath = filePath
                        })
                    )
                };

                Debug.WriteLine($"Добавляем папку: {folder.FileName}");
                folders.Add(folder);
            }

            return folders;
        }




        #endregion



        #region CreateFileInFolder

        public void CreateNewMarkdownFile()
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

            //MessageBox.Show($"Файл успешно создан: {filePath}");
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
            //MessageBox.Show($"Файл успешно создан в папке: {selectedFolder.FilePath}");
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


        public void LoadFileStructure()
        {
            Folders.Clear(); // Очищаем перед добавлением
            Files.Clear();   // Очищаем перед добавлением

            var rootFolders = LoadFoldersRecursively(RootDirectory);
            foreach (var folder in rootFolders)
            {
                Folders.Add(folder);
            }

            var files = Directory.GetFiles(RootDirectory, "*.md")
                .Select(filePath => new FileItem
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath
                });

            foreach (var file in files)
            {
                Files.Add(file);
            }
        }


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
                FontSize = SelectedFontSize,
                Scale = Scale,
            };

            // Если есть подписчики на событие, вызываем его
            OpenFileRequest?.Invoke(markdownViewer);  // Передаем UserControl (MarkdownViewer)
        }

        #region Calendar

        public ICommand OpenCalendarCommand { get; }

        private void OpenCalendar()
        {
            // Создаём и открываем окно календаря
            var calendarWindow = new CalendarPage(this);
            calendarWindow.ShowDialog(); // Если хотите, чтобы окно было модальным
        }







        #endregion



        //рома добавил снизу
        // Логика удаления файла
        public void DeleteFile(FileItem fileItem)
        {
            //MessageBox.Show("Метод DeleteFile вызван"); // Это должно появиться при попытке удалить файл

            if (fileItem != null)
            {
                //MessageBox.Show($"Удаление файла: {fileItem.FilePath}");

                // Удаляем файл из родительской коллекции
                var parentFolder = fileItem.ParentFolder;
                if (parentFolder != null)
                {
                    parentFolder.RemoveFile(fileItem);
                }

                // Удаляем файл с диска
                if (File.Exists(fileItem.FilePath))
                {
                    try
                    {
                        File.Delete(fileItem.FilePath);
                        MessageBox.Show($"Файл удалён: {fileItem.FilePath}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении файла: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Файл не существует на диске.");
                }

                // Обновляем UI
                if (parentFolder != null)
                {
                    // Обновляем ObservableCollection вручную
                    parentFolder.Files.Remove(fileItem);  // Убираем удалённый файл из коллекции
                    CollectionViewSource.GetDefaultView(parentFolder.Files)?.Refresh();  // Принудительное обновление
                }

                if (parentFolder != null)
                {
                    parentFolder.RemoveFile(fileItem);

                    // Принудительно обновим папку целиком
                    parentFolder.OnPropertyChanged(nameof(FolderItem.Files));
                    parentFolder.OnPropertyChanged(nameof(FolderItem.SubFolders));
                    parentFolder.OnPropertyChanged(nameof(FolderItem.CompositeSubItems));
                }


                // Уведомляем о изменениях коллекции (если необходимо)
                OnPropertyChanged(nameof(Files));
                OnPropertyChanged(nameof(Folders));
            }
        }



        // Логика удаления папки
        public void DeleteFolder(FolderItem folderItem)
        {
            if (folderItem != null)
            {
                // Рекурсивное удаление подкаталогов
                foreach (var subFolder in folderItem.SubFolders.ToList())
                {
                    DeleteFolder(subFolder); // Рекурсивно удаляем все подпапки
                }

                // Удаление файлов из папки
                foreach (var file in folderItem.Files.ToList())
                {
                    folderItem.RemoveFile(file); // Удаляем файлы из коллекции
                }

                // Удаление папки из коллекции
                Folders.Remove(folderItem);

                // Удаление папки с диска
                if (Directory.Exists(folderItem.FilePath))
                {
                    try
                    {
                        Directory.Delete(folderItem.FilePath, true); // true - для удаления вложенных файлов и папок
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при удалении папки: {ex.Message}");
                    }
                }

                // Уведомление об изменении коллекции
                OnPropertyChanged(nameof(Folders)); // Если используем INotifyPropertyChanged
            }
        }


        // Начало перетаскивания
        public void StartDrag(object item)
        {
            if (item == null) return;

            var data = new DataObject();
            if (item is FileItem fileItem)
            {
                data.SetData("FileItem", fileItem);
            }
            else if (item is FolderItem folderItem)
            {
                data.SetData("FolderItem", folderItem);
            }
            DragDrop.DoDragDrop(Application.Current.MainWindow, data, DragDropEffects.Move);
        }

        public void DropItem(FolderItem targetFolder, object droppedItem)
        {
            if (droppedItem == null) return;

            try
            {
                if (droppedItem is FileItem fileItem)
                {
                    MoveFile(targetFolder, fileItem);
                }
                else if (droppedItem is FolderItem folderItem)
                {
                    MoveFolder(targetFolder, folderItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при перемещении элемента: {ex.Message}");
            }
        }

        private void MoveFile(FolderItem targetFolder, FileItem fileItem)
        {
            string newFilePath;
            if (targetFolder == null)
            {
                // Перемещаем в папку MyFolders
                newFilePath = Path.Combine(RootDirectory, fileItem.FileName);
            }
            else
            {
                // Перемещаем в выбранную папку
                newFilePath = Path.Combine(targetFolder.FilePath, fileItem.FileName);
            }

            if (File.Exists(newFilePath))
            {
                MessageBox.Show("Файл с таким именем уже существует в целевой папке.");
                return;
            }

            try
            {
                File.Move(fileItem.FilePath, newFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения файла: {ex.Message}");
                return;
            }

            var sourceFolder = FindParentFolder(fileItem);
            if (sourceFolder != null)
            {
                sourceFolder.Files.Remove(fileItem);
            }
            else
            {
                Files.Remove(fileItem);
            }

            fileItem.FilePath = newFilePath;

            if (targetFolder == null)
            {
                Files.Add(fileItem);
            }
            else
            {
                targetFolder.Files.Add(fileItem);
            }

            // Обновляем TreeView
            OnPropertyChanged(nameof(Files));
            if (targetFolder != null)
            {
                OnPropertyChanged(nameof(targetFolder.Files));
            }
        }
        private void MoveFolder(FolderItem targetFolder, FolderItem folderItem)
        {
            var newFolderPath = targetFolder == null
                ? Path.Combine(Path.GetDirectoryName(folderItem.FilePath), folderItem.FileName)
                : Path.Combine(targetFolder.FilePath, folderItem.FileName);

            if (Directory.Exists(newFolderPath))
            {
                MessageBox.Show("Папка с таким именем уже существует в целевой папке.");
                return;
            }

            try
            {
                Directory.Move(folderItem.FilePath, newFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения папки: {ex.Message}");
                return;
            }

            var sourceFolder = FindParentFolder(folderItem);
            if (sourceFolder != null)
            {
                sourceFolder.SubFolders.Remove(folderItem);
            }
            else
            {
                Folders.Remove(folderItem);
            }

            folderItem.FilePath = newFolderPath;
            UpdateFolderPaths(folderItem, newFolderPath);

            if (targetFolder == null)
            {
                Folders.Add(folderItem);
            }
            else
            {
                targetFolder.SubFolders.Add(folderItem);
            }
        }

        private void UpdateFolderPaths(FolderItem folder, string newParentPath)
        {
            folder.FilePath = Path.Combine(newParentPath, folder.FileName);
            foreach (var subFolder in folder.SubFolders)
            {
                UpdateFolderPaths(subFolder, folder.FilePath);
            }
            foreach (var file in folder.Files)
            {
                file.FilePath = Path.Combine(folder.FilePath, file.FileName);
            }
        }

        private FolderItem FindParentFolder(object item)
        {
            foreach (var folder in Folders)
            {
                var found = FindParentFolderRecursive(folder, item);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private FolderItem FindParentFolderRecursive(FolderItem folder, object item)
        {
            if (folder.Files.Contains(item as FileItem) || folder.SubFolders.Contains(item as FolderItem))
            {
                return folder;
            }

            foreach (var subFolder in folder.SubFolders)
            {
                var found = FindParentFolderRecursive(subFolder, item);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        // Завершение перетаскивания
        public void DropFile(FolderItem targetFolder, FileItem fileItem)
        {
            if (targetFolder == null || fileItem == null) return;

            try
            {
                var newFilePath = Path.Combine(targetFolder.FilePath, fileItem.FileName);

                if (File.Exists(newFilePath))
                {
                    MessageBox.Show("Файл с таким именем уже существует в целевой папке.");
                    return;
                }

                File.Move(fileItem.FilePath, newFilePath);

                var sourceFolder = Folders.FirstOrDefault(f => f.Files.Contains(fileItem));
                if (sourceFolder != null)
                {
                    sourceFolder.Files.Remove(fileItem);
                }
                else
                {
                    Files.Remove(fileItem);
                }

                targetFolder.Files.Add(fileItem);
                fileItem.FilePath = newFilePath;

                MessageBox.Show("Файл перемещен.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при перемещении файла: {ex.Message}");
            }
        }

    }
}