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
using System.Diagnostics;
using PintheCloud.Converters;
using PintheCloud.Helpers;

namespace PintheCloud.Pages
{
    public partial class FileListPage : PtcPage
    {
        // Const Instances
        private const string PICK_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/at_here/png/general_bar_download.png";
        private const string DELETE_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/pin_the_cloud/png/general_bar_delete.png";

        private const string EDIT_IMAGE_URI = "/Assets/pajeon/at_here/png/list_edit.png";
        private const string EDIT_PRESS_IMAGE_URI = "/Assets/pajeon/at_here/png/list_edit_p.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/at_here/png/list_view.png";
        private const string VIEW_PRESS_IMAGE_URI = "/Assets/pajeon/at_here/png/list_view_p.png";

        // Instances
        private string SpotId = null;
        private string SpotName = null;
        private string AccountId = null;
        private string AccountName = null;
        private int PlatformIndex = 0;
        private bool LaunchLock = false;
        private bool IsPrivate = false;
        private string SpotPassword = null;

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


            // Set event by previous page
            Context con = EventHelper.GetContext(EventHelper.FILE_LIST_PAGE);
            con.HandleEvent(EventHelper.EXPLORER_PAGE, EventHelper.PICK_PIVOT, this.SETTINGS_and_EXPLORE_PICK);
            con.HandleEvent(EventHelper.NEW_SPOT_PAGE, this.EXPLORER_PIN);
            con.HandleEvent(EventHelper.SETTINGS_PAGE, this.SETTINGS_and_EXPLORE_PICK);
            con.HandleEvent(EventHelper.ADD_FILE_PAGE, this.ADD_FILE);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.LaunchLock = false;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void SETTINGS_and_EXPLORE_PICK()
        {
            //this.NavigationContext
            this.SpotId = NavigationContext.QueryString["spotId"];
            this.SpotName = NavigationContext.QueryString["spotName"];
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
                base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.InternetUnavailableMessage);
        }


        private void EXPLORER_PIN()
        {
            // Remove New Spot Page from backstack
            NavigationService.RemoveBackEntry();

            // Get parameters
            //this.PlatformIndex = (int)PhoneApplicationService.Current.State[PLATFORM_KEY];
            StorageAccount account = Switcher.GetCurrentStorage().GetStorageAccount();
            this.SpotName = NavigationContext.QueryString["spotName"];
            this.AccountId = account.Id;
            this.AccountName = account.StorageName;
            if (NavigationContext.QueryString["private"].Equals("True"))
                this.IsPrivate = true;
            else
                this.IsPrivate = false;
            this.SpotPassword = NavigationContext.QueryString["password"];
            
            // Set Binding Instances to UI
            uiSpotName.Text = this.SpotName;
            uiAccountName.Text = this.AccountName;
            uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.BOLD);

            // If internet is on, upload file given from previous page.
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
                this.InitialPinSpotAndUploadFileAsync();
            else
                base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.InternetUnavailableMessage);
        }


        private void ADD_FILE()
        {
            // If internet is on, upload file given from previous page.
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Get selected file list from previous page before pin spot.
                List<FileObjectViewItem> fileList = new List<FileObjectViewItem>();
                foreach (FileObjectViewItem fileObjectViewItem in (List<FileObjectViewItem>)PhoneApplicationService.Current.State[SELECTED_FILE_KEY])
                    fileList.Add(fileObjectViewItem);

                // Upload each files in order.
                foreach (FileObjectViewItem fileObjectViewItem in fileList)
                    this.UploadFileAsync(new FileObjectViewItem(fileObjectViewItem));
            }
            else
            {
                base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.InternetUnavailableMessage);
            }
        }


        private void uiFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
                string currentEditViewMode = uiFileListEditViewImageButton.ImageSource;
                if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View mode
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                    {
                        // Launch files to other reader app.
                        if (!this.LaunchLock)
                        {
                            this.LaunchLock = true;
                            this.LaunchFileAsync(fileObjectViewItem);
                        }
                    }
                }
                else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit mode
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PickAppBarButton.IsEnabled = true;
                        this.DeleteAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count < 1)
                        {
                            this.PickAppBarButton.IsEnabled = false;
                            this.DeleteAppBarButton.IsEnabled = false;
                        }
                    }
                }
            }

        }


        private async void LaunchFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Downloading message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.DOWNLOADING_IMAGE_URI;
            });

            // Download file and Launch files to other reader app.
            StorageFile downloadFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileObjectViewItem.Name, CreationCollisionOption.ReplaceExisting);
            if (await App.BlobStorageManager.DownloadFileAsync(fileObjectViewItem.Id, downloadFile) != null)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                });
                await Launcher.LaunchFileAsync(downloadFile);
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });  
            }
            
            // Hide Progress Indicator
            base.SetProgressIndicator(false);    
        }


        // Download files.
        private void PickAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                IStorageManager iStr = Switcher.GetCurrentStorage();
                if (iStr.IsSignIn())
                {
                    foreach (FileObjectViewItem fileObjectViewItem in this.SelectedFile)
                        this.PickFileAsync(fileObjectViewItem);
                    this.SelectedFile.Clear();
                    this.PickAppBarButton.IsEnabled = false;
                    this.DeleteAppBarButton.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show(AppResources.NoSignedInMessage, iStr.GetStorageName(), MessageBoxButton.OK);
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
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.DOWNLOADING_IMAGE_URI;
            });

            // Download
            Stream stream = await App.BlobStorageManager.DownloadFileStreamAsync(fileObjectViewItem.Id);
            if (stream != null)
            {
                IStorageManager iStorageManager = Switcher.GetCurrentStorage();
                FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
                if (await iStorageManager.UploadFileStreamAsync(rootFolder.Id, fileObjectViewItem.Name, stream))
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        ((FileObjectViewModel)PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                        string currentEditViewMode = uiFileListEditViewImageButton.ImageSource;
                        if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View Mode
                            fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                        else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit Mode
                            fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                    });  
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
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
                MessageBoxResult result = MessageBox.Show(AppResources.DeleteFileMessage, AppResources.DeleteFileMessage, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    foreach (FileObjectViewItem fileObjectViewItem in this.SelectedFile)
                        this.DeleteFileAsync(fileObjectViewItem);
                    this.SelectedFile.Clear();
                    this.PickAppBarButton.IsEnabled = false;
                    this.DeleteAppBarButton.IsEnabled = false;
                }
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
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.DELETING_IMAGE_URI;
            });

            // Delete
            if (await App.BlobStorageManager.DeleteFileAsync(fileObjectViewItem.Id))
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.FileObjectViewModel.Items.Remove(fileObjectViewItem);
                    if (this.FileObjectViewModel.Items.Count < 1)
                        base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.NoFileInSpotMessage);
                });
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }


        private async void RefreshAsync(string message)
        {
            // Show Refresh message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, message);
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
                base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.NoFileInSpotMessage);
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }


        private async void InitialPinSpotAndUploadFileAsync()
        {
            // Get selected file list from previous page before pin spot.
            List<FileObjectViewItem> fileList = new List<FileObjectViewItem>();
            foreach (FileObjectViewItem fileObjectViewItem in (List<FileObjectViewItem>)PhoneApplicationService.Current.State[SELECTED_FILE_KEY])
                fileList.Add(fileObjectViewItem);

            // Pin Spot
            bool result = await this.PinSpotAsync();

            // If Pin Spot successes, Upload each files in order.
            if (result)
                foreach (FileObjectViewItem fileObjectViewItem in fileList)
                    this.UploadFileAsync(new FileObjectViewItem(fileObjectViewItem));    
        }

        private async Task<bool> PinSpotAsync()
        {
            // Show Pining message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.PiningSpot);
            base.SetProgressIndicator(true);

            // Pin spot
            Geoposition geo = await App.Geolocator.GetGeopositionAsync();
            Spot spot = new Spot(this.SpotName, geo.Coordinate.Latitude, geo.Coordinate.Longitude,
                this.AccountId, this.AccountName, 0, this.IsPrivate, this.SpotPassword);
            bool result = await App.SpotManager.PinSpotAsync(spot);
            if (result)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    ((SpotViewModel)PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;
                    this.SpotId = spot.id;
                });
            }
            else
            {
                base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.BadPinSpotMessage);
            }

            // Hide progress message
            base.SetProgressIndicator(false);
            return result;
        }
        

        // Upload. have to wait it.
        private async void UploadFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Uploading message and file for good UX
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.UPLOADING_IMAGE_URI;
                this.FileObjectViewModel.Items.Add(fileObjectViewItem);
            });

            // Upload
            Stream stream = await Switcher.GetCurrentStorage().DownloadFileStreamAsync((fileObjectViewItem.DownloadUrl == null ? fileObjectViewItem.Id : fileObjectViewItem.DownloadUrl));
            if (stream != null)
            {
                string blobId = await App.BlobStorageManager.UploadFileStreamAsync(this.AccountId, this.SpotId, fileObjectViewItem.Name, stream);
                if (blobId != null)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.Id = blobId;
                        string currentEditViewMode = uiFileListEditViewImageButton.ImageSource;
                        if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View Mode
                            fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                        else if(currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit Mode
                            fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                    });
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide progress message
            base.SetProgressIndicator(false);
        }


        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // If internet is on, refresh
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
                this.RefreshAsync(AppResources.Refreshing);
            else
                base.SetListUnableAndShowMessage(uiFileList, uiFileListMessage, AppResources.InternetUnavailableMessage);
        }


        private void uiFileListEditViewImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Change edit view mode
            string currentEditViewMode = uiFileListEditViewImageButton.ImageSource;
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
                uiFileListEditViewImageButton.ImageSource = EDIT_IMAGE_URI;
                uiFileListEditViewImageButton.ImagePressedSource = EDIT_PRESS_IMAGE_URI;

                // Change select check image of each file object view item.
                foreach (FileObjectViewItem fileObjectViewItem in this.FileObjectViewModel.Items)
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI)
                        || fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                }
            }

            else if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // To Edit mode
            {
                // Change mode image and remove app bar buttons.
                ApplicationBar.Buttons.Add(this.PickAppBarButton);
                ApplicationBar.Buttons.Add(this.DeleteAppBarButton);
                uiFileListEditViewImageButton.ImageSource = VIEW_IMAGE_URI;
                uiFileListEditViewImageButton.ImagePressedSource = VIEW_PRESS_IMAGE_URI;

                // Change select check image of each file object view item.
                foreach (FileObjectViewItem fileObjectViewItem in this.FileObjectViewModel.Items)
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                }
            }
        }


        private void uiFileListEditViewButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button editViewButton = (Button)sender;
            Uri buttonImageUri = ((BitmapImage)((Image)editViewButton.Content).Source).UriSource;
            if (buttonImageUri.ToString().Equals(EDIT_IMAGE_URI))
                ((Image)editViewButton.Content).Source = new BitmapImage(new Uri(EDIT_PRESS_IMAGE_URI, UriKind.Relative));
            else if (buttonImageUri.ToString().Equals(VIEW_IMAGE_URI))
                ((Image)editViewButton.Content).Source = new BitmapImage(new Uri(VIEW_PRESS_IMAGE_URI, UriKind.Relative));
        }


        private void uiFileListEditViewButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button editViewButton = (Button)sender;
            Uri buttonImageUri = ((BitmapImage)((Image)editViewButton.Content).Source).UriSource;
            if (buttonImageUri.ToString().Equals(EDIT_PRESS_IMAGE_URI))
                ((Image)editViewButton.Content).Source = new BitmapImage(new Uri(EDIT_IMAGE_URI, UriKind.Relative));
            else if(buttonImageUri.ToString().Equals(VIEW_PRESS_IMAGE_URI))
                ((Image)editViewButton.Content).Source = new BitmapImage(new Uri(VIEW_IMAGE_URI, UriKind.Relative));
        }


        private void uiAppBarPinInfoButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(EventHelper.ADD_FILE_PAGE, UriKind.Relative));
        }
    }
}