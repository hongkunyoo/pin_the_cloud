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
        private const string UPLOAD_FAIL_IMAGE_URI = "UPLOAD_FAIL_IMAGE_URI";
        private const string UPLOADING_FILE_IMAGE_URI = "UPLOADING_FILE_IMAGE_URI";
        private const string EDIT_IMAGE_URI = "/Assets/pajeon/png/general_edit.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/png/general_view.png";

        // Instances
        private string SpaceId = null;
        private string SpaceName = null;
        private string AccountId = null;
        private string AccountName = null;
        private int PlatformIndex = 0;

        private FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();


        public FileListPage()
        {
            InitializeComponent();


            // Set datacontext
            uiFileList.DataContext = this.FileObjectViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get platform and pivot from previous page.
            this.PlatformIndex = Convert.ToInt32(NavigationContext.QueryString["platform"]);
            int pivot = ExplorerPage.PICK_PIVOT_INDEX;
            if (PREVIOUS_PAGE.Equals(EXPLORER_PAGE))
                pivot = Convert.ToInt32(NavigationContext.QueryString["pivot"]);
            

            // Set diffetent by pivot state
            if (pivot == ExplorerPage.PICK_PIVOT_INDEX)  // Pick state
            {
                // Get parameters
                this.SpaceId = NavigationContext.QueryString["spaceId"];
                this.SpaceName = NavigationContext.QueryString["spaceName"];
                this.AccountId = NavigationContext.QueryString["accountId"];
                this.AccountName = NavigationContext.QueryString["accountName"];


                // Set Binding Instances to UI
                uiSpaceName.Text = this.SpaceName;
                uiAccountName.Text = this.AccountName;
                uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.LIGHT);


                // If internet is on, refresh
                // Otherwise, show internet unavailable message.
                if (NetworkInterface.GetIsNetworkAvailable())
                    this.Refresh(AppResources.Loading);
                else
                    base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
            }

            else  // Pin state
            {
                // Get parameters
                IStorageManager iStorageManager = App.IStorageManagers[this.PlatformIndex];
                Account account = iStorageManager.GetAccount();
                this.SpaceName = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY];
                this.AccountId = account.account_platform_id;
                this.AccountName = account.account_name;


                // Set Binding Instances to UI
                uiSpaceName.Text = this.SpaceName;
                uiAccountName.Text = this.AccountName;
                uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(StringToFontWeightConverter.BOLD);


                // If internet is on, upload file given from previous page.
                // Otherwise, show internet unavailable message.
                if (NetworkInterface.GetIsNetworkAvailable())
                    this.InitialPinSpotAndUploadFileAsync();
                else
                    base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }



        /*** Self Methods ***/

        private async void uiFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Obejct
            FileObject fileObject = uiFileList.SelectedItem as FileObject;

            // Set selected item to null for next selection of list item. 
            uiFileList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObject != null)
            {
                // TODO save local or cloud or chooser...

                // Do Something
                //StorageFile downloadFile = await App.LocalStorageManager.CreateFileToLocalBlobStorageAsync(fileObject.Name);
                //await App.BlobStorageManager.DownloadFileAsync(fileObject.Id, downloadFile);
                //await Launcher.LaunchFileAsync(downloadFile);

                await this.DownloadFilesAsync(fileObject.Id, fileObject.Name);
            }
        }


        private async void Refresh(string message)
        {
            // Show Refresh message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, message, uiFileListMessage);
            base.SetProgressIndicator(true);

            // If file exists, show it.
            // Otherwise, show no file in spot message.
            List<FileObject> fileList = await App.BlobStorageManager.GetFilesFromSpotAsync(this.AccountId, this.SpaceId);
            if (fileList.Count > 0)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;
                    this.FileObjectViewModel.SetItems(fileList);
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
        private async Task<string> PinSpot()
        {
            // Show Pining message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, AppResources.PiningSpot, uiFileListMessage);
            base.SetProgressIndicator(true);


            Geoposition geo = await App.GeoCalculateManager.GetCurrentGeopositionAsync();
            Space space = new Space(DateTime.Now.ToString(), geo.Coordinate.Latitude, geo.Coordinate.Longitude, this.AccountId, this.AccountName, 0);
            string spaceId = null;
            if (await App.SpaceManager.PinSpaceAsync(space))
                spaceId = space.id;


            // Hide Progress Indicator and return
            base.SetProgressIndicator(false);
            return spaceId;
        }


        // Upload. have to wait it.
        private async void UploadFileAsync(FileObjectViewItem file)
        {
            // Show Uploading message and file for good UX
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                file.SelectCheckImage = UPLOADING_FILE_IMAGE_URI;
                this.FileObjectViewModel.Items.Add(file);

                uiFileListMessage.Text = AppResources.Uploading;
                uiFileList.Visibility = Visibility.Visible;
                uiFileListMessage.Visibility = Visibility.Visible;
            });


            // Upload
            IStorageManager iStorageManager = App.IStorageManagers[this.PlatformIndex];
            Stream stream = await iStorageManager.DownloadFileStreamAsync(file.Id);
            if (stream != null)
            {
                string uploadPath = await App.BlobStorageManager.UploadFileStreamAsync(this.AccountId, this.SpaceId, file.Name, stream);
                if (uploadPath != null)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        file.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        file.SelectCheckImage = UPLOAD_FAIL_IMAGE_URI;
                    });
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    file.SelectCheckImage = UPLOAD_FAIL_IMAGE_URI;
                });
            }

            // Hide progress message
            base.SetProgressIndicator(false);
            base.Dispatcher.BeginInvoke(() =>
            {
                uiFileListMessage.Visibility = Visibility.Collapsed;
            });
        }


        private async void InitialPinSpotAndUploadFileAsync()
        {
            // Get selected file list from previous page before pin spot.
            List<FileObjectViewItem> fileList = (List<FileObjectViewItem>)PhoneApplicationService.Current.State[ExplorerPage.SELECTED_FILE_KEY];
            FileObjectViewItem[] fileArray = fileList.ToArray<FileObjectViewItem>();

            // If pin spot success, upload files.
            // Otherwise, show warning message.
            string spaceId = await this.PinSpot();
            if (spaceId != null)
            {
                // Register space id
                // Get selected files from previous page, Upload each files in order.
                this.SpaceId = spaceId;
                foreach (FileObjectViewItem file in fileArray)
                    this.UploadFileAsync(file);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiFileList, AppResources.BadPinSpotMessage, uiFileListMessage);
            }
        }


        private async Task<bool> DownloadFilesAsync(string fildObjectId, string fileObjectName)
        {
            IStorageManager iStorageManager = App.IStorageManagers[this.PlatformIndex];
            FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
            if (!await iStorageManager.UploadFileStreamAsync(rootFolder.Id, fileObjectName, await App.BlobStorageManager.DownloadFileStreamAsync(fildObjectId)))
                return false;
            else
                return true;
        }

        private void uiFileListEditViewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string currentEditViewMode = ((BitmapImage)uiFileListEditViewButtonImage.Source).UriSource.ToString();
            if (currentEditViewMode.Equals(EDIT_IMAGE_URI))
                uiFileListEditViewButtonImage.Source = new BitmapImage(new Uri(VIEW_IMAGE_URI, UriKind.Relative));
            else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))
                uiFileListEditViewButtonImage.Source = new BitmapImage(new Uri(EDIT_IMAGE_URI, UriKind.Relative));
        }

        private void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            // If internet is on, refresh
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
                this.Refresh(AppResources.Refreshing);
            else
                base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
        }

        private void uiAppBarPinInfoButton_Click(object sender, System.EventArgs e)
        {
            // TODO Have to add pin pop up.
        }
    }
}