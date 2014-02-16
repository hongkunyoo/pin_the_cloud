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


        public FileListPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get parameters
            if ((int)PhoneApplicationService.Current.State["PIVOT"] == ExplorerPage.PICK_PIVOT)
            {
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
                    await this.Refresh();
                }
                else
                { 
                    base.SetListUnableAndShowMessage(uiFileList, AppResources.InternetUnavailableMessage, uiFileListMessage);
                }
            }
            else
            {
                this.SpaceName = (string)App.ApplicationSettings[Account.ACCOUNT_NICK_NAME];

                Account account = App.IStorageManager.GetCurrentAccount();
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
                    if (await this.UploadFilesAsync())
                        await this.Refresh();
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


        private async Task Refresh()
        {
            // TODO Does it get all files every time?
            ObservableCollection<FileObject> list = new ObservableCollection<FileObject>
                (await App.BlobStorageManager.GetFilesFromFolderAsync(this.AccountId, this.SpaceId));

            // If file exists, show it.
            // Otherwise, show no file in spot message.
            if (list.Count > 0)
                uiFileList.DataContext = list;
            else
                base.SetListUnableAndShowMessage(uiFileList, AppResources.NoFileInSpotMessage, uiFileListMessage);
        }


        private async Task<bool> UploadFilesAsync()
        {
            bool result = false;
            Geoposition geo = await App.GeoCalculateManager.GetCurrentGeopositionAsync();
            Space space = new Space(DateTime.Now.ToString(), geo.Coordinate.Latitude, geo.Coordinate.Longitude, this.AccountId, this.AccountName, 0);

            if (await App.SpaceManager.PinSpaceAsync(space))
            {
                this.SpaceId = space.id;
                List<FileObject> list = (List<FileObject>)PhoneApplicationService.Current.State["SELECTED_FILE"];
                foreach (FileObject file in list)
                {
                    ProgressBar progressBar = new ProgressBar();
                    progressBar.Value = 0;
                    Progress<LiveOperationProgress> progressHandler
                        = new Progress<LiveOperationProgress>((progress) => { progressBar.Value = progress.ProgressPercentage; });

                    Stream stream = await App.IStorageManager.DownloadFileThroughStreamAsync(file.Id, progressHandler);
                    await App.BlobStorageManager.UploadFileThroughStreamAsync(this.AccountId, this.SpaceId, file.Name, stream);
                }
                result = true;
            }
            return result;
        }


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

                FileObject rootFolder = await App.IStorageManager.GetRootFolderAsync();
                await App.IStorageManager.UploadFileThroughStreamAsync
                    (rootFolder.Id, fileObject.Name, await App.BlobStorageManager.DownloadFileThroughStreamAsync(fileObject.Id));
            }
        }
    }
}