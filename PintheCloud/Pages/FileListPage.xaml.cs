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

namespace PintheCloud.Pages
{
    public partial class FileListPage : PtcPage
    {
        private string SpaceId;
        private string SpaceName;
        private string AccountName;
        private string SpaceLikeNumber;
        private string SpaceLikeNumberColor;

        private string SpaceAccountId;

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
                this.AccountName = NavigationContext.QueryString["accountName"];
                this.SpaceLikeNumber = NavigationContext.QueryString["spaceLikeNumber"];
                this.SpaceLikeNumberColor = NavigationContext.QueryString["spaceLikeNumberColor"];

                this.SpaceAccountId = (App.AccountManager.GetCurrentAcccount()).account_platform_id;
                // Set Binding Instances to UI
                uiSpaceName.Text = this.SpaceName;
                uiAccountName.Text = this.AccountName;
                uiSpaceLikeNumber.Text = this.SpaceLikeNumber;

                Brush likeColor = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHex(this.SpaceLikeNumberColor));
                uiSpaceLikeNumber.Foreground = likeColor;
                uiSpaceLikeText.Foreground = likeColor;
                await Refresh();
            }
            else
            {
                this.SpaceId = "";
                this.SpaceName = "";
                this.AccountName = App.AccountManager.GetCurrentAcccount().account_name;
                this.SpaceLikeNumber = "0";
                this.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;

                // Set Binding Instances to UI
                uiSpaceName.Text = this.SpaceName;
                uiAccountName.Text = this.AccountName;
                uiSpaceLikeNumber.Text = this.SpaceLikeNumber;

                Brush likeColor = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHex(this.SpaceLikeNumberColor));
                uiSpaceLikeNumber.Foreground = likeColor;
                uiSpaceLikeText.Foreground = likeColor;

                await UploadFilesAsync();
            }
            
        }

        private async Task UploadFilesAsync()
        {
       
            Geoposition geo = await App.GeoCalculateManager.GetCurrentGeopositionAsync();
            Account account = App.AccountManager.GetCurrentAcccount();
            Space space = new Space("DEFAULT_SPACE_NAME", geo.Coordinate.Latitude, geo.Coordinate.Longitude, account.account_platform_id, account.account_name, 0, 0);

            this.SpaceAccountId = account.account_platform_id;
            await App.MobileService.GetTable<Space>().InsertAsync(space);
            this.SpaceId = space.id;
            List<FileObject> list = (List<FileObject>)PhoneApplicationService.Current.State["SELECTED_FILE"];
            foreach (FileObject file in list)
            {
                await App.BlobStorageManager.UploadFileThroughStreamAsync(SpaceAccountId, this.SpaceId + "/" + file.Name, await App.SkyDriveManager.DownloadFileThroughStreamAsync(file.Id));
                await Refresh();
            }
        }

        private async Task Refresh()
        {
            ObservableCollection<FileObject> list = new ObservableCollection<FileObject>(await App.BlobStorageManager.GetFilesFromFolderAsync(this.SpaceAccountId,this.SpaceId));
            uiFileList.DataContext = list;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void uiFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Obejct
            FileObject fileObject = uiFileList.SelectedItem as FileObject;

            // Set selected item to null for next selection of list item. 
            uiFileList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObject != null) 
            {
                // Do Something
            }
        }
    }
}