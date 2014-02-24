using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Managers;
using System.Diagnostics;
using System.Collections.ObjectModel;
using PintheCloud.ViewModels;
using PintheCloud.Models;

namespace PintheCloud.Pages
{
    public partial class UploadPickerPopup : UserControl
    {
        private IStorageManager StorageManager = null;
        private IStorageManager[] StorageList = null;

        private Stack<FileObjectViewItem> FolderTree = new Stack<FileObjectViewItem>();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();

        public UploadPickerPopup()
        {
            InitializeComponent();

            this.StorageList = App.IStorageManagers;
            uiListPicker.ItemsSource = new ObservableCollection<IStorageManager>(this.StorageList);
        }

        private async void uiListPicker_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            if (e.AddedItems == null || e.AddedItems.Count != 1) return;
            
            this.StorageManager = (IStorageManager)e.AddedItems[0];

            FileObject root = await this.StorageManager.GetRootFolderAsync();
        }

        private void uiFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }

        private void uiCurrentPath_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            Debug.WriteLine(this.StorageManager.GetStorageName());
        }
    }
    public class TestObject
    {
        public TestObject()
        {
            //this.Name = "test";
        }
        public string Name { get; set; }
    }
}
