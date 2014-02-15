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
using PintheCloud.Workers;
using PintheCloud.Resources;
using System.Windows.Media.Imaging;
using System.Net.NetworkInformation;
using PintheCloud.ViewModels;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        // Instances
        private const int MY_SPACES_PIVOT = 1;
        public SpaceViewModel MySpaceViewModel = new SpaceViewModel();


        public SettingsPage()
        {
            InitializeComponent();
        }


        // Construct pivot item by page index
        private async void uiSettingPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly,
            // One time loading.
            switch (uiSettingPivot.SelectedIndex)
            {
                case MY_SPACES_PIVOT:
                    ApplicationBar.IsVisible = true;

                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                        if (!MySpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetMySpacePivotAsync(AppResources.Loading);
                    break;
                default:
                    ApplicationBar.IsVisible = false;
                    break;
            }
        }



        /*** Profile ***/

        // Logout
        private void uiLogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(AppResources.SignOutMessage, AppResources.SignOutCaption, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                App.CloudManager = App.SkyDriveManager;
                App.CloudManager.SignOut();
                NavigationService.Navigate(new Uri(PtcPage.SPLASH_PAGE, UriKind.Relative));
            }
        }



        /*** My Spot ***/

        // List select event
        private void uiMySpaceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            SpaceViewItem spaceViewItem = uiMySpaceList.SelectedItem as SpaceViewItem;

            // If selected item isn't null, goto File list page.
            if (spaceViewItem != null)
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }

            // Set selected item to null for next selection of list item. 
            uiMySpaceList.SelectedItem = null;
        }


        // Refresh space list.
        private async void uiAppBarRefreshMenuItem_Click(object sender, System.EventArgs e)
        {
            // If Internet available, Set space list
            if (NetworkInterface.GetIsNetworkAvailable())
                await this.SetMySpacePivotAsync(AppResources.Loading);
        }


        private void uiAppBarRemoveButton_Click(object sender, System.EventArgs e)
        {
            // TODO: 여기에 구현된 이벤트 처리기를 추가하십시오.
        }


        private async Task SetMySpacePivotAsync(string message)
        {
            // Show progress indicator 
            base.SetListUnableAndShowMessage(uiMySpaceList, message, uiMySpaceMessage);
            PtcPage.SetProgressIndicator(this, true);

            // Before go load, set mutex to true.
            MySpaceViewModel.IsDataLoading = true;

            // If there is my spaces, Clear and Add spaces to list
            // Otherwise, Show none message.
            MobileServiceCollection<Space, Space> spaces = await App.SpaceManager.GetMySpaceViewItemsAsync();

            if (spaces != null)  // There are my spaces
            {
                this.MySpaceViewModel.SetItems(spaces);
                uiMySpaceList.DataContext = this.MySpaceViewModel;
                uiMySpaceList.Visibility = Visibility.Visible;
                uiMySpaceMessage.Visibility = Visibility.Collapsed;
            }
            else  // No my spaces
            {
                base.SetListUnableAndShowMessage(uiMySpaceList, AppResources.NoMySpaceMessage, uiMySpaceMessage);
                MySpaceViewModel.IsDataLoading = false;  // Mutex
            }

            // Hide progress indicator
            PtcPage.SetProgressIndicator(this, false);
        }


        private void skyDriveAppBarButton_Click(object sender, EventArgs e)
        {
            uiCurrentCloudKindText.Text = AppResources.SkyDrive;
        }


        private void googleDriveAppBarButton_Click(object sender, EventArgs e)
        {
            uiCurrentCloudKindText.Text = AppResources.GoogleDrive;
        }
    }
}