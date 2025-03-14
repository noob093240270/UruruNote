﻿#pragma checksum "..\..\..\..\Views\MainWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "5CE179CC22DF49D92EDB4BE08EF0A30408B5090F"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using MaterialDesignThemes.MahApps;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using UruruNote.ViewsModels;
using UruruNotes.Models;


namespace UruruNotes.Views {
    
    
    /// <summary>
    /// MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 66 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonOpen;
        
        #line default
        #line hidden
        
        
        #line 72 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonClose;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Сalendar;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SearchTextBox;
        
        #line default
        #line hidden
        
        
        #line 95 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid MainGrid;
        
        #line default
        #line hidden
        
        
        #line 102 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid TreeViewGrid;
        
        #line default
        #line hidden
        
        
        #line 103 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MaterialDesignThemes.Wpf.Card LeftMenu;
        
        #line default
        #line hidden
        
        
        #line 177 "..\..\..\..\Views\MainWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Frame PageFrame;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/UruruNotes;component/views/mainwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\MainWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.ButtonOpen = ((System.Windows.Controls.Button)(target));
            
            #line 66 "..\..\..\..\Views\MainWindow.xaml"
            this.ButtonOpen.Click += new System.Windows.RoutedEventHandler(this.ToggleVisibilityButton_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ButtonClose = ((System.Windows.Controls.Button)(target));
            
            #line 72 "..\..\..\..\Views\MainWindow.xaml"
            this.ButtonClose.Click += new System.Windows.RoutedEventHandler(this.ToggleVisibilityButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Сalendar = ((System.Windows.Controls.Button)(target));
            return;
            case 4:
            this.SearchTextBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 87 "..\..\..\..\Views\MainWindow.xaml"
            this.SearchTextBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.SearchTextBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 90 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.Image)(target)).MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.SettingsIcon_MouseUp);
            
            #line default
            #line hidden
            return;
            case 6:
            this.MainGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 7:
            this.TreeViewGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 8:
            this.LeftMenu = ((MaterialDesignThemes.Wpf.Card)(target));
            return;
            case 9:
            
            #line 119 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).SelectedItemChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.TreeView_SelectedItemChanged);
            
            #line default
            #line hidden
            
            #line 120 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TreeView_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 121 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).DragEnter += new System.Windows.DragEventHandler(this.TreeView_DragEnter);
            
            #line default
            #line hidden
            
            #line 122 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).Drop += new System.Windows.DragEventHandler(this.TreeView_Drop);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 140 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).SelectedItemChanged += new System.Windows.RoutedPropertyChangedEventHandler<object>(this.TreeView_SelectedItemChanged);
            
            #line default
            #line hidden
            
            #line 141 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.TreeView_PreviewMouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 142 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).DragEnter += new System.Windows.DragEventHandler(this.TreeView_DragEnter);
            
            #line default
            #line hidden
            
            #line 143 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.TreeView)(target)).Drop += new System.Windows.DragEventHandler(this.TreeView_Drop);
            
            #line default
            #line hidden
            return;
            case 16:
            
            #line 166 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CreateNewFileButton_Click);
            
            #line default
            #line hidden
            return;
            case 17:
            
            #line 169 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CreateNewFolderButton_Click);
            
            #line default
            #line hidden
            return;
            case 18:
            this.PageFrame = ((System.Windows.Controls.Frame)(target));
            
            #line 178 "..\..\..\..\Views\MainWindow.xaml"
            this.PageFrame.Navigated += new System.Windows.Navigation.NavigatedEventHandler(this.PageFrame_Navigated);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 10:
            
            #line 130 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.AddFileMenuItem_Click);
            
            #line default
            #line hidden
            break;
            case 11:
            
            #line 131 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.DeleteFolderButton_Click);
            
            #line default
            #line hidden
            break;
            case 12:
            
            #line 132 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MoveToFolderMenuItem_Click);
            
            #line default
            #line hidden
            break;
            case 14:
            
            #line 150 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.DeleteFileButton_Click);
            
            #line default
            #line hidden
            break;
            case 15:
            
            #line 151 "..\..\..\..\Views\MainWindow.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MoveToFolderMenuItem_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

