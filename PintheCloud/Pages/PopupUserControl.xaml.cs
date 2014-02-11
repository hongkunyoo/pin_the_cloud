using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.ViewModels;
using PintheCloud.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using PintheCloud.Utilities;
using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class PopupUserControl : UserControl
    {


        // Instances
        public FileObjectViewModel SkyDriveFileObjectViewModel = new FileObjectViewModel();

        PhoneApplicationPage currentPage;
        PhoneApplicationFrame currentFrame;

        private FileObject skydriveRootFolder;
        private Stack<FileObject> folderTree { get; set; }
        public ObservableCollection<FileObject> fileList { get; set; }

        bool mustRestoreApplicationBar = false;
        bool mustRestoreSystemTray = false;

        public event OnSkyDriveCompleted OnDismiss;
        public delegate void OnSkyDriveCompleted(List<FileObject> files);


        //public string uiCurrentPath;
        public Popup uiRootPopup;
        //public UserControl uiCurrentPath;

        public PopupUserControl()
        {
            InitializeComponent();

            // TODO
            // Get file list using API
            SkyDriveFileObjectViewModel.Items = null; // this is the ObservableCollection List input
            uiSkyDriveFileList.DataContext = SkyDriveFileObjectViewModel;
            Initialize();
        }

        public async void Initialize()
        {
            //this.localRootFolder = await App.LocalStorageManager.GetSkyDriveStorageFolderAsync();
            this.skydriveRootFolder = await App.SkyDriveManager.GetRootFolderAsync();

            LayoutRoot.Width = Application.Current.Host.Content.ActualWidth;
            LayoutRoot.Height = Application.Current.Host.Content.ActualHeight;

            this.uiRootPopup = this.Parent as Popup;
            this.folderTree = new Stack<FileObject>();
            this.fileList = new ObservableCollection<FileObject>();

            uiCurrentPath.Text = "/";
            GetTreeForFolder(this.skydriveRootFolder);
        }

        async void GetTreeForFolder(FileObject folder)
        {
            fileList.Clear();
            List<FileObject> files = await App.SkyDriveManager.GetFilesFromFolderAsync(folder.Id);

            foreach (FileObject file in files)
            {
                fileList.Add(file);
            }

            if (!folderTree.Contains(folder))
                folderTree.Push(folder);

            ObservableCollection<FileObjectViewItem> fovilist = new ObservableCollection<FileObjectViewItem>();
            foreach (FileObject fo in fileList)
            {
                FileObjectViewItem f = new FileObjectViewItem();
                f.FileName = fo.Name;
                f.FileThumnail = new Uri("", UriKind.Relative);
                fovilist.Add(f);
                MyDebug.WriteLine(f.FileName);
            }
            SkyDriveFileObjectViewModel.Items = fovilist;
            uiSkyDriveFileList.DataContext = SkyDriveFileObjectViewModel;
            uiCurrentPath.Text = folderTree.First().ParentId;
        }

        public void TreeUp(object sender, RoutedEventArgs e)
        {
            if (folderTree.Count > 1)
            {
                folderTree.Pop();
                GetTreeForFolder(folderTree.First());
            }
        }

        public void Show()
        {
            //Application.Current.RootVisual
            currentFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            currentPage = currentFrame.Content as PhoneApplicationPage;

            if (SystemTray.IsVisible)
            {
                mustRestoreSystemTray = true;
                SystemTray.IsVisible = false;
            }


            if (currentPage.ApplicationBar != null)
            {
                if (currentPage.ApplicationBar.IsVisible)
                    mustRestoreApplicationBar = true;

                currentPage.ApplicationBar.IsVisible = false;
            }

            if (currentPage != null)
            {
                currentPage.BackKeyPress += OnBackKeyPress;
            }

            uiRootPopup.IsOpen = true;
        }

        void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Dismiss(null);
        }

        private void Dismiss(FileObject file)
        {
            if (currentPage != null)
            {
                currentPage.BackKeyPress -= OnBackKeyPress;
            }

            uiRootPopup.IsOpen = false;

            if (mustRestoreApplicationBar)
                currentPage.ApplicationBar.IsVisible = true;

            if (mustRestoreSystemTray)
                SystemTray.IsVisible = true;

            if (OnDismiss != null)
                OnDismiss(null);
        }

        private void treeup_btn_clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: Add event handler implementation here.

            MyDebug.WriteLine("Treeup Btn Clicked!");
        }
    }
}
