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
using System.Windows.Media.Imaging;
using PintheCloud.Utilities;
using System.Windows.Media;
using PintheCloud.Models;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.System;
using PintheCloud.Managers;
using System.IO;
using Microsoft.Live;
using System.Net.NetworkInformation;
using PintheCloud.Resources;

namespace PintheCloud.Pages
{
    public partial class FileListPage : PtcPage
    {
        // Const Instances
        private const string UPLOADING_FILE_IMAGE_URI = "UPLOADING_FILE_IMAGE_URI";
        private const string UPLOAD_FAIL_IMAGE_URI = "UPLOAD_FAIL_IMAGE_URI";
        
        private const string DELETING_FILE_IMAGE_URI = "DELETING_FILE_IMAGE_URI";
        private const string DELET_FAIL_IMAGE_URI = "DELET_FAIL_IMAGE_URI";

        private const string DOWNLOADING_FILE_IMAGE_URI = "DOWNLOADING_FILE_IMAGE_URI";
        private const string DOWNLOAD_FAIL_IMAGE_URI = "DOWNLOAD_FAIL_IMAGE_URI";

        private const string EDIT_IMAGE_URI = "/Assets/pajeon/png/general_edit.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/png/general_view.png";
        private const string DELETE_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_bar_delete.png";
        private const string PICK_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/png/general_download.png";

        // Instances
        private string SpotId = null;
        private string SpotName = null;
        private string AccountId = null;
        private string AccountName = null;
        private int PlatformIndex = 0;

        private ApplicationBarIconButton DeleteAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarIconButton PickAppBarButton = new ApplicationBarIconButton();
        private FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();
        private List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();


        public FileListPage()
        {
            InitializeComponent();

            // Set delete app bar button and pick app bar button
            this.DeleteAppBarButton.Text = AppResources.Pin;
            this.DeleteAppBarButton.IconUri = new Uri(DELETE_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.DeleteAppBarButton.IsEnabled = false;
            this.DeleteAppBarButton.Click += DeleteAppBarButton_Click;

            this.PickAppBarButton.Text = AppResources.Pick;
            this.PickAppBarButton.IconUri = new Uri(PICK_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PickAppBarButton.IsEnabled = false;
            this.PickAppBarButton.Click += PickAppBarButton_Click;

            // Set datacontext
            uiFileList.DataContext = this.FileObjectViewModel;

            Context con = EventManager.GetContext(EventManager.FILE_LIST_PAGE);

            con.HandleEvent(EventManager.EXPLORER_PAGE, EventManager.PICK, () => {
                uiFileListEditViewButton.Visibility = Visibility.Collapsed;
                SETTINGS_and_EXPLORE_PICK();
            });

            con.HandleEvent(EventManager.EXPLORER_PAGE, EventManager.PIN, () =>
            {
                uiFileListEditViewButton.Visibility = Visibility.Visible;
                EXPLORER_PIN();
            });

            con.HandleEvent(EventManager.SETTINGS_PAGE, () =>
            {
                uiFileListEditViewButton.Visibility = Visibility.Visible;
            });

            con.HandleEvent(EventManager.SETTINGS_PAGE, SETTINGS_and_EXPLORE_PICK);
        }
        private void SETTINGS_and_EXPLORE_PICK()
        {
            this.SpotId = NavigationContext.QueryString["spaceId"];
            this.SpotName = NavigationContext.QueryString["spaceName"];
            this.AccountId = NavigationContext.QueryString["accountId"];
            this.AccountName = NavigationContext.QueryString["accountName"];


            // Set Binding Instances to UI
            uiSpotName.Text = this.SpotName;
            uiAccountName.Text = this.AccountName;
            uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.LIGHT);

            // If internet is on, refresh
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
                this.RefreshAsync(AppResources.Loading);
            else
                base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
        }

        private void EXPLORER_PIN()
        {
            // Get parameters
            this.PlatformIndex = (int)PhoneApplicationService.Current.State[PLATFORM_KEY];
            IStorageManager iStorageManager = App.IStorageManagers[this.PlatformIndex];
            Account account = iStorageManager.GetAccount();
            this.SpotName = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY];
            this.AccountId = account.account_platform_id;
            this.AccountName = account.account_name;

            // Set Binding Instances to UI
            uiSpotName.Text = this.SpotName;
            uiAccountName.Text = this.AccountName;
            uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.BOLD);

            // If internet is on, upload file given from previous page.
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
                this.InitialPinSpotAndUploadFileAsync();
            else
                base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get platform and pivot from previous page.
            // Set edit view button by pivot and previous page.
            //this.PlatformIndex = Convert.ToInt32(NavigationContext.QueryString["platform"]);
            //int pivot = ExplorerPage.PICK_PIVOT_INDEX;
            //if (PREVIOUS_PAGE.Equals(EXPLORER_PAGE))
            //{
            //    pivot = Convert.ToInt32(NavigationContext.QueryString["pivot"]);
            //    if (pivot == ExplorerPage.PICK_PIVOT_INDEX)  // Pick state
                    
            //    else  // Pin state
                    
            //}
            //else if (PREVIOUS_PAGE.Equals(SETTINGS_PAGE))
            //{
            //    uiFileListEditViewButton.Visibility = Visibility.Visible;
            //}

            // if Setting & Explorer
            // Set diffetent by pivot state
            //if (pivot == ExplorerPage.PICK_PIVOT_INDEX)  // Pick state
            //{
            //    // Get parameters
            //    this.SpaceId = NavigationContext.QueryString["spaceId"];
            //    this.SpaceName = NavigationContext.QueryString["spaceName"];
            //    this.AccountId = NavigationContext.QueryString["accountId"];
            //    this.AccountName = NavigationContext.QueryString["accountName"];


            //    // Set Binding Instances to UI
            //    uiSpaceName.Text = this.SpaceName;
            //    uiAccountName.Text = this.AccountName;
            //    uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.LIGHT);


            //    // If internet is on, refresh
            //    // Otherwise, show internet unavailable message.
            //    if (NetworkInterface.GetIsNetworkAvailable())
            //        this.Refresh(AppResources.Loading);
            //    else
            //        base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
            //}

            //else  // Pin state
            //{
            //    // Get parameters
            //    IStorageManager iStorageManager = App.IStorageManagers[this.PlatformIndex];
            //    Account account = iStorageManager.GetAccount();
            //    this.SpaceName = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY];
            //    this.AccountId = account.account_platform_id;
            //    this.AccountName = account.account_name;


            //    // Set Binding Instances to UI
            //    uiSpaceName.Text = this.SpaceName;
            //    uiAccountName.Text = this.AccountName;
            //    uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.BOLD);


            //    // If internet is on, upload file given from previous page.
            //    // Otherwise, show internet unavailable message.
            //    if (NetworkInterface.GetIsNetworkAvailable())
            //        this.InitialPinSpotAndUploadFileAsync();
            //    else
            //        base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
            //}
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }



        /*** Self Methods ***/

        private async void uiFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Obejct
            FileObjectViewItem fileObjectViewItem = uiFileList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiFileList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If it is view mode, click is preview.
                // If it is edit mode, click is selection.
                string currentEditViewMode = ((BitmapImage)uiFileListEditViewButtonImage.Source).UriSource.ToString();
                if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View mode
                {
                    if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                    {
                        // TODO Launch files to other reader app.
                        //StorageFile downloadFile = await App.LocalStorageManager.CreateFileToLocalBlobStorageAsync(fileObjectViewItem.Name);
                        //await App.BlobStorageManager.DownloadFileAsync(fileObjectViewItem.Id, downloadFile);
                        //await Launcher.LaunchFileAsync(downloadFile);
                    }
                }
                else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit mode
                {
                    if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PickAppBarButton.IsEnabled = true;
                        this.DeleteAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count < 1)
                        {
                            this.PickAppBarButton.IsEnabled = false;
                            this.DeleteAppBarButton.IsEnabled = false;
                        }
                    }
                }
            }
        }


        // Download files.
        private async void PickAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (App.IStorageManagers[this.PlatformIndex].IsSignIn())
                {
                    // Wait tasks
                    await App.TaskManager.WaitSignInTask(this.PlatformIndex);
                    await App.TaskManager.WaitSignOutTask(this.PlatformIndex);

                    if (App.IStorageManagers[this.PlatformIndex].GetAccount() != null)
                    {
                        foreach (FileObjectViewItem fileObjectViewItem in this.SelectedFile)
                            this.PickFileAsync(fileObjectViewItem);
                        this.SelectedFile.Clear();
                        this.PickAppBarButton.IsEnabled = false;
                        this.DeleteAppBarButton.IsEnabled = false;
                    }
                    else
                    {
                        MessageBox.Show(AppResources.NoSignedInMessage, App.IStorageManagers[this.PlatformIndex].GetStorageName(), MessageBoxButton.OK);    
                    }
                }
                else
                {
                    MessageBox.Show(AppResources.NoSignedInMessage, App.IStorageManagers[this.PlatformIndex].GetStorageName(), MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void PickFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Downloading message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectCheckImage = DOWNLOADING_FILE_IMAGE_URI;
            });

            // Download
            Stream stream = await App.BlobStorageManager.DownloadFileStreamAsync(fileObjectViewItem.Id);
            if (stream != null)
            {
                IStorageManager iStorageManager = App.IStorageManagers[this.PlatformIndex];
                FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
                if (await iStorageManager.UploadFileStreamAsync(rootFolder.Id, fileObjectViewItem.Name, stream))
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        string currentEditViewMode = ((BitmapImage)uiFileListEditViewButtonImage.Source).UriSource.ToString();
                        if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View Mode
                            fileObjectViewItem.SelectCheckImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                        else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit Mode
                            fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.SelectCheckImage = DOWNLOAD_FAIL_IMAGE_URI;
                    });  
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectCheckImage = DOWNLOAD_FAIL_IMAGE_URI;
                }); 
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);            
        }


        // Delete files.
        private void DeleteAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                foreach (FileObjectViewItem fileObjectViewItem in this.SelectedFile)
                    this.DeleteFileAsync(fileObjectViewItem);
                this.SelectedFile.Clear();
                this.PickAppBarButton.IsEnabled = false;
                this.DeleteAppBarButton.IsEnabled = false;
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void DeleteFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Deleting message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectCheckImage = DELETING_FILE_IMAGE_URI;
            });

            // Delete
            if (await App.BlobStorageManager.DeleteFileAsync(fileObjectViewItem.Id))
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.FileObjectViewModel.Items.Remove(fileObjectViewItem);
                });
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectCheckImage = DELET_FAIL_IMAGE_URI;
                });
            }

            // Hide Progress Indicator
            // If file list is empty, show empty message.
            base.SetProgressIndicator(false);
            if (this.FileObjectViewModel.Items.Count-1 < 1)
                base.SetListUnableAndShowMessage(uiFileList, AppResources.NoFileInSpotMessage, uiFileListMessage);
        }


        private async void RefreshAsync(string message)
        {
            // Show Refresh message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, message, uiFileListMessage);
            base.SetProgressIndicator(true);

            // If file exists, show it.
            // Otherwise, show no file in spot message.
            List<FileObject> fileList = await App.BlobStorageManager.GetFilesFromSpotAsync(this.AccountId, this.SpotId);
            if (fileList.Count > 0)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;
                    this.FileObjectViewModel.SetItems(fileList, false);
                });
            }
            else
            {
                base.SetListUnableAndShowMessage(uiFileList, AppResources.NoFileInSpotMessage, uiFileListMessage);
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }


        // Pin spot
        private async Task<string> PinSpotAsync()
        {
            // Show Pining message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, AppResources.PiningSpot, uiFileListMessage);
            base.SetProgressIndicator(true);

            // Pin spot
            Geoposition geo = await App.GeoCalculateManager.GetCurrentGeopositionAsync();
            Spot spot = new Spot(this.SpotName, geo.Coordinate.Latitude, geo.Coordinate.Longitude, this.AccountId, this.AccountName, 0);
            string spotId = null;
            if (await App.SpotManager.PinSpotAsync(spot))
            {
                spotId = spot.id;
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                base.SetListUnableAndShowMessage(uiFileList, AppResources.BadPinSpotMessage, uiFileListMessage);
            }

            // Hide Progress Indicator and return
            base.SetProgressIndicator(false);
            return spotId;
        }


        // Upload. have to wait it.
        private async void UploadFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Uploading message and file for good UX
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectCheckImage = UPLOADING_FILE_IMAGE_URI;
                this.FileObjectViewModel.Items.Add(fileObjectViewItem);
            });

            // Upload
            Stream stream = await App.IStorageManagers[this.PlatformIndex].DownloadFileStreamAsync(fileObjectViewItem.Id);
            if (stream != null)
            {
                string uploadPath = await App.BlobStorageManager.UploadFileStreamAsync(this.AccountId, this.SpotId, fileObjectViewItem.Name, stream);
                if (uploadPath != null)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        string currentEditViewMode = ((BitmapImage)uiFileListEditViewButtonImage.Source).UriSource.ToString();
                        if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View Mode
                            fileObjectViewItem.SelectCheckImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                        else if(currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit Mode
                            fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.SelectCheckImage = UPLOAD_FAIL_IMAGE_URI;
                    });
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectCheckImage = UPLOAD_FAIL_IMAGE_URI;
                });
            }

            // Hide progress message
            base.SetProgressIndicator(false);
        }


        private async void InitialPinSpotAndUploadFileAsync()
        {
            // Get selected file list from previous page before pin spot.
            List<FileObjectViewItem> fileList = (List<FileObjectViewItem>)PhoneApplicationService.Current.State[ExplorerPage.SELECTED_FILE_KEY];
            FileObjectViewItem[] fileArray = fileList.ToArray<FileObjectViewItem>();

            // If pin spot success, upload files.
            // Otherwise, show warning message.
            string spotId = await this.PinSpotAsync();
            if (spotId != null)
            {
                // Register spot id
                // Get selected files from previous page, Upload each files in order.
                this.SpotId = spotId;

                for (int i = 0; i < fileArray.Length; i++)
                {
                    FileObjectViewItem fileObjectViewItem = new FileObjectViewItem(fileArray[i]);
                    this.UploadFileAsync(fileObjectViewItem);
                }
            }
        }


        private void uiFileListEditViewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Change edit view mode
            string currentEditViewMode = ((BitmapImage)uiFileListEditViewButtonImage.Source).UriSource.ToString();
            if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // To View mode
            {
                // Change mode image and remove app bar buttons.
                if (this.SelectedFile.Count > 0)
                {
                    this.SelectedFile.Clear();
                    this.PickAppBarButton.IsEnabled = false;
                    this.DeleteAppBarButton.IsEnabled = false;
                }
                ApplicationBar.Buttons.Remove(this.PickAppBarButton);
                ApplicationBar.Buttons.Remove(this.DeleteAppBarButton);
                uiFileListEditViewButtonImage.Source = new BitmapImage(new Uri(EDIT_IMAGE_URI, UriKind.Relative));
                
                // Change select check image of each file object view item.
                foreach (FileObjectViewItem fileObjectViewItem in this.FileObjectViewModel.Items)
                {
                    if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI)
                        || fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                }
            }

            else if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // To Edit mode
            {
                // Change mode image and remove app bar buttons.
                ApplicationBar.Buttons.Add(this.PickAppBarButton);
                ApplicationBar.Buttons.Add(this.DeleteAppBarButton);
                uiFileListEditViewButtonImage.Source = new BitmapImage(new Uri(VIEW_IMAGE_URI, UriKind.Relative));

                // Change select check image of each file object view item.
                foreach (FileObjectViewItem fileObjectViewItem in this.FileObjectViewModel.Items)
                {
                    if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                }
            }
        }


        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // If internet is on, refresh
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
                this.RefreshAsync(AppResources.Refreshing);
            else
                base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
        }


        private void uiAppBarPinInfoButton_Click(object sender, System.EventArgs e)
        {
            // TODO Have to add pin pop up.
        }
    }
}