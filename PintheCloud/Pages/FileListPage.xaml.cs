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

namespace PintheCloud.Pages
{
    public partial class FileListPage : PtcPage
    {
        private string SpaceId;
        private string SpaceName;
        private string AccountId;
        private string AccountName;
        private string AccountNameFontWeight;
        private string SpaceLikeNumberColor;

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
                this.AccountNameFontWeight = NavigationContext.QueryString["accountIdFontWeight"];

                // Set Binding Instances to UI
                uiSpaceName.Text = this.SpaceName;
                uiAccountName.Text = this.AccountName;
                uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(this.AccountNameFontWeight);

                await Refresh();
            }
            else
            {
                // Test name of space, it will be nick name set by user in setting view.
                this.SpaceName = "Nickname";

                Account account = App.CloudManager.GetCurrentAccount();
                this.AccountId = account.account_platform_id;
                this.AccountName = account.account_name;
                this.AccountNameFontWeight = StringToFontWeightConverter.BOLD;

                // Set Binding Instances to UI
                uiSpaceName.Text = this.SpaceName;
                uiAccountName.Text = this.AccountName;
                uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(this.AccountNameFontWeight);

                await UploadFilesAsync();
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private async Task UploadFilesAsync()
        {
            // Test Name of space, now time.
            Geoposition geo = await App.GeoCalculateManager.GetCurrentGeopositionAsync();
            Space space = new Space(DateTime.Now.ToString(), geo.Coordinate.Latitude, geo.Coordinate.Longitude, this.AccountId, this.AccountName, 0);
                     
            await App.MobileService.GetTable<Space>().InsertAsync(space);
            this.SpaceId = space.id;

            List<FileObject> list = (List<FileObject>)PhoneApplicationService.Current.State["SELECTED_FILE"];
            foreach (FileObject file in list)
            {
                await App.BlobStorageManager.UploadFileThroughStreamAsync(this.AccountId, this.SpaceId, file.Name, await App.SkyDriveManager.DownloadFileThroughStreamAsync(file.Id));
                await Refresh();
            }
        }


        private async Task Refresh()
        {
            ObservableCollection<FileObject> list = new ObservableCollection<FileObject>(await App.BlobStorageManager.GetFilesFromFolderAsync(this.AccountId, this.SpaceId));
            uiFileList.DataContext = list;
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
                // Do Something
                //StorageFile downloadFile = await App.LocalStorageManager.CreateFileToLocalBlobStorageAsync(fileObject.Name);
                //await App.BlobStorageManager.DownloadFileAsync(fileObject.Id, downloadFile);
                //await Launcher.LaunchFileAsync(downloadFile);

                FileObject rootFolder = await App.SkyDriveManager.GetRootFolderAsync();
                await App.SkyDriveManager.UploadFileThroughStreamAsync(rootFolder.Id, fileObject.Name, await App.BlobStorageManager.DownloadFileThroughStreamAsync(fileObject.Id));
            }
        }
    }
}