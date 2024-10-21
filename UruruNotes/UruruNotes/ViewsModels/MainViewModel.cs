using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UruruNote.Models;
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using GalaSoft.MvvmLight.Command;





namespace UruruNote.ViewsModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }




        public UserSettings userSettings { get; set; }



        #region Settings


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
        public object SelectedTreeViewItem {  get; set; }

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

    }
}
