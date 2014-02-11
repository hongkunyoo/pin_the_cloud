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

using Windows.Storage;
using PintheCloud.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using PintheCloud.Utilities;
using System.Windows.Media;



namespace PintheCloud.Pages
{
    public partial class SkyDrivePickerPage : PtcPage
    {

        // Instances
        public FileObjectViewModel SkyDriveFileObjectViewModel = new FileObjectViewModel();

        PhoneApplicationPage currentPage;
        PhoneApplicationFrame currentFrame;

        private FileObject skydriveRootFolder;
        private Stack<FileObject> folderTree { get; set; }
        public List<FileObject> selectedFile { get; set; }

        bool mustRestoreApplicationBar = false;
        bool mustRestoreSystemTray = false;

        public event OnSkyDriveCompleted OnDismiss;
        public delegate void OnSkyDriveCompleted(List<FileObject> files);

        //private ApplicationBar 
        //public string uiCurrentPath;
        //public Popup uiRootPopup;


        public SkyDrivePickerPage()
        {
            InitializeComponent();

            Initialize();
        }

        public async void Initialize()
        {
            //this.localRootFolder = await App.LocalStorageManager.GetSkyDriveStorageFolderAsync();
            this.skydriveRootFolder = await App.SkyDriveManager.GetRootFolderAsync();

            this.folderTree = new Stack<FileObject>();
            this.selectedFile = new List<FileObject>();
            currentFrame = Application.Current.RootVisual as PhoneApplicationFrame;
            currentPage = currentFrame.Content as PhoneApplicationPage;

            GetTreeForFolder(this.skydriveRootFolder);
        }

        async void GetTreeForFolder(FileObject folder)
        {
            List<FileObject> files = await App.SkyDriveManager.GetFilesFromFolderAsync(folder.Id);
            uiSkyDriveFileList.DataContext = new ObservableCollection<FileObject>(files);

            if (!this.folderTree.Contains(folder))
                this.folderTree.Push(folder);

            uiCurrentPath.Text = GetCurrentPath();
        }
        private string GetCurrentPath()
        {
            FileObject[] array = folderTree.Reverse<FileObject>().ToArray<FileObject>();
            
            string str = "";
            foreach(FileObject f in array)
            {
                str += f.Name + "/";
            }
            return str;
        }
        private void TreeUp()
        {
            if (folderTree.Count > 1)
            {
                folderTree.Pop();
                GetTreeForFolder(folderTree.First());
            }
        }
        /*
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
                //currentPage.BackKeyPress += OnBackKeyPress;
            }

            //uiRootPopup.IsOpen = true;
        }*/
        /*
        void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Dismiss(null);
        }
        */
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (this.folderTree.Count == 1)
            {
                e.Cancel = false;
                NavigationService.GoBack();                
            }
            else
            {
                TreeUp();
                e.Cancel = true; 
            }
            base.OnBackKeyPress(e);
        }
    
        /*
        private void Dismiss(FileObject file)
        {
            if (currentPage != null)
            {
                currentPage.BackKeyPress -= OnBackKeyPress;
            }

            //uiRootPopup.IsOpen = false;

            if (mustRestoreApplicationBar)
                currentPage.ApplicationBar.IsVisible = true;

            if (mustRestoreSystemTray)
                SystemTray.IsVisible = true;

            if (OnDismiss != null)
                OnDismiss(null);
        }
        */
        private void uiSkyDriveFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            if (e.AddedItems.Count > 1) return;

            FileObject file = (FileObject)e.AddedItems[0];
            if (file.Type.Equals("folder"))
            {
                GetTreeForFolder(file);
                MyDebug.WriteLine(file.Name);
            }
            // Do selection if file
            else
            {
                this.selectedFile.Add(file);
                MyDebug.WriteLine(file.Name);
            }
        }
        
        private void uiTreeupBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            TreeUp();
        }

        private void uiAppBarConfirmButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State["SELECTED_FILE"] = selectedFile;
            NavigationService.Navigate(new Uri("/Utilities/TestDrive.xaml", UriKind.Relative));
        }

    }
}