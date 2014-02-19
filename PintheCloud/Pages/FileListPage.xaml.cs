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
        // Instances
        private string SpaceId;
        private string SpaceName;
        private string AccountId;
        private string AccountName;

        private Progress<LiveOperationProgress> ProgressHandler;


        public FileListPage()
        {
            InitializeComponent();


            // Make progress handler
            this.ProgressHandler = new Progress<LiveOperationProgress>((progress) =>
            {
                uiFileListProgressBar.Value = progress.ProgressPercentage;
                double percentage = Math.Round(uiFileListProgressBar.Value * 10.0) / 10.0;
                uiFileListProgressPercentageText.Text = percentage.ToString();
            });
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get pivot state
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
                {
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;
                    this.Refresh();
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
                }
            }

            else if (pivot == ExplorerPage.PIN_INFO_PIVOT_INDEX)  // Pin state
            {
                // Get parameters
                Account account = App.IStorageManager.GetCurrentAccount();
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
                {
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;

                    string spaceId = await this.PinSpot();
                    if (spaceId != null)
                        if (await this.UploadFilesAsync(spaceId))
                            this.Refresh();
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
                }
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


        private async void Refresh()
        {
            // Show Refresh message and Progress Indicator
            base.SetListUnableAndShowMessage(uiFileList, AppResources.Refreshing, uiFileListMessage);
            base.SetProgressIndicator(true);

            // TODO Does it get all files every time?
            ObservableCollection<FileObject> list = new ObservableCollection<FileObject>
                (await App.BlobStorageManager.GetFilesFromFolderAsync(this.AccountId, this.SpaceId));

            // If file exists, show it.
            // Otherwise, show no file in spot message.
            if (list.Count > 0)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiFileList.DataContext = list;
                    uiFileList.Visibility = Visibility.Visible;
                    uiFileListMessage.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                base.SetListUnableAndShowMessage(uiFileList, AppResources.NoFileInSpotMessage, uiFileListMessage);
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }

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


        private async Task<bool> UploadFilesAsync(string spaceId)
        {
            // Show Uploading message and progress bar
            base.Dispatcher.BeginInvoke(() =>
            {
                uiFileListProgressBar.Visibility = Visibility.Visible;
                uiFileListProgressPercentagePanel.Visibility = Visibility.Visible;
            });


            // Register space id and get selected files from previous page.
            this.SpaceId = spaceId;
            List<FileObject> list = (List<FileObject>)PhoneApplicationService.Current.State[ExplorerPage.SELECTED_FILE_KEY];

            // Upload each files in order.
            foreach (FileObject file in list)
            {
                // Show Uploading message and reset percentage to 0.
                base.SetListUnableAndShowMessage(uiFileList, AppResources.Uploading + "  " + file.Name + "...", uiFileListMessage);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiFileListProgressPercentageText.Text = "0";
                });

                // TODO if it failed, show error message.
                // Upload.
                Stream stream = await App.IStorageManager.DownloadFileThroughStreamAsync(file.Id, this.ProgressHandler);
                await App.BlobStorageManager.UploadFileThroughStreamAsync(this.AccountId, this.SpaceId, file.Name, stream);
            }


            // Hide progress bar
            base.Dispatcher.BeginInvoke(() =>
            {
                uiFileListProgressBar.Visibility = Visibility.Collapsed;
                uiFileListProgressPercentagePanel.Visibility = Visibility.Collapsed;
            });
            return true;
        }


        private async Task<bool> DownloadFilesAsync(string fildObjectId, string fileObjectName)
        {
            FileObject rootFolder = await App.IStorageManager.GetRootFolderAsync();
            if (!await App.IStorageManager.UploadFileThroughStreamAsync
                    (rootFolder.Id, fileObjectName, await App.BlobStorageManager.DownloadFileThroughStreamAsync(fildObjectId)))
                return false;
            else
                return true;
        }
    }
}