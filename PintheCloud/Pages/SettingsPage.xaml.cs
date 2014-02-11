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

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        // Instances
        private const int MY_SPACES_PIVOT = 1;

        private const string EDIT_IMAGE_PATH = "/Assets/pajeon/png/general_edit.png";
        private const string VIEW_IMAGE_PATH = "/Assets/pajeon/png/general_view.png";

        public SpaceViewModel MySpaceViewModel = new SpaceViewModel();
        private bool IsLikeButtonClicked = false;

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
                    uiSpaceEditButton.Visibility = Visibility.Visible;

                    // If Internet available, Set space list
                    if (NetworkInterface.GetIsNetworkAvailable())
                        if (!MySpaceViewModel.IsDataLoading)  // Mutex check
                            await this.SetMySpacePivotAsync(AppResources.Loading);
                    break;
                default:
                    uiSpaceEditButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }



        /*** Profile ***/

        // Logout
        private void uiLogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(AppResources.LogoutMessage, AppResources.Logout, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                App.AccountManager.Logout();
                NavigationService.Navigate(new Uri(PtcPage.SPLASH_PAGE, UriKind.Relative));
            }
        }

        
        
        /*** My Space ***/

        // Process of editing spaces, especially deleting or not.
        private void uiSpaceEditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get information about image
            Button editButton = (Button)sender;
            Image editButtonImage = (Image)editButton.Content;
            BitmapImage editButtonImageBitmap = (BitmapImage)editButtonImage.Source;
            string editButtonImageUriSource = editButtonImageBitmap.UriSource.ToString();

            // If it is not view mode, go edit
            // Otherwise, go view mode
            if (editButtonImageUriSource.Equals(EDIT_IMAGE_PATH))  // It is View mode
            {
                editButtonImage.Source = new BitmapImage(new Uri(VIEW_IMAGE_PATH, UriKind.Relative));

                // TODO Chande mode to edit mode
            }
            else  // It is Edit mode
            {
                editButtonImage.Source = new BitmapImage(new Uri(EDIT_IMAGE_PATH, UriKind.Relative));

                // TODO Chande mode to view mode
            }
        }

        // List select event
        private async void uiMySpaceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Space View Item
            // Do selection changed event job.
            // Set selected item to null for next selection of list item. 
            SpaceViewItem spaceViewItem = uiMySpaceList.SelectedItem as SpaceViewItem;
            await this.uiSpaceList_SelectionChanged(spaceViewItem);
            uiMySpaceList.SelectedItem = null;
        }


        // Process Like or Not Like by current state
        private void uiSpaceLikeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set this is like button click
            // It is required to seperate between space button and like button
            this.IsLikeButtonClicked = true;
        }



        /*** Self Method ***/

        private async Task SetMySpacePivotAsync(string message)
        {
            // Get different Space Worker by internet state.
            if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
            {
                // Set worker and show loading message
                App.SpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
                App.SpaceManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());

                // Show progress indicator 
                uiMySpaceList.Visibility = Visibility.Collapsed;
                uiMySpaceMessage.Text = message;
                uiMySpaceMessage.Visibility = Visibility.Visible;
                base.SetProgressIndicator(true);

                // Before go load, set mutex to true.
                MySpaceViewModel.IsDataLoading = true;


                // If there is my spaces, Clear and Add spaces to list
                // Otherwise, Show none message.
                ObservableCollection<SpaceViewItem> items =
                                await App.SpaceManager.GetMySpaceViewItemsAsync();

                if (items != null)  // There are my spaces
                {
                    this.MySpaceViewModel.Items = items;
                    uiMySpaceList.DataContext = null;
                    uiMySpaceList.DataContext = this.MySpaceViewModel;
                    uiMySpaceList.Visibility = Visibility.Visible;
                    uiMySpaceMessage.Visibility = Visibility.Collapsed;
                }
                else  // No my spaces
                {
                    uiMySpaceList.Visibility = Visibility.Collapsed;
                    uiMySpaceMessage.Text = AppResources.NoMySpaceMessage;
                    uiMySpaceMessage.Visibility = Visibility.Visible;
                    MySpaceViewModel.IsDataLoading = false;  // Mutex
                }


                // Hide progress indicator
                base.SetProgressIndicator(false);
            }
        }


        // Do selection changed event job.
        private async Task uiSpaceList_SelectionChanged(SpaceViewItem spaceViewItem)
        {
            // If selected item isn't null and it doesn't come from like button, goto File list page.
            // Otherwise, Process Like or Not Like by current state
            if (spaceViewItem != null && !this.IsLikeButtonClicked)  // Go to FIle List Page
            {
                string parameters = App.SpaceManager.GetParameterStringFromSpaceViewItem(spaceViewItem);
                NavigationService.Navigate(new Uri(PtcPage.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }

            else if (this.IsLikeButtonClicked)  // Do like
            {
                // Get different Account Space Relation Worker by internet state.
                if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                {
                    App.AccountSpaceRelationManager.SetAccountSpaceRelationWorker(new AccountSpaceRelationInternetAvailableWorker());


                    // Get information about image
                    string spaceId = spaceViewItem.SpaceId;
                    string spaceLikeButtonImageUri = spaceViewItem.SpaceLikeButtonImage.ToString();


                    // Set Image first for good user experience.
                    // Like or Note LIke by current state
                    if (spaceLikeButtonImageUri.Equals(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH))  // Do Like
                    {
                        spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative);

                        // If like success, set like number ++
                        // If like fail, set image back to original
                        if (await App.AccountSpaceRelationManager.LikeAysnc(spaceId, true))
                        {
                            // TODO ++ like number
                        }
                        else
                        {
                            spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative);
                        }
                    }

                    else  // Do Not Like
                    {
                        spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative);

                        // If not like success, set like number --
                        // If not like fail, set image back to original
                        if (await App.AccountSpaceRelationManager.LikeAysnc(spaceId, false))
                        {
                            // TODO -- like number
                        }
                        else
                        {
                            spaceViewItem.SpaceLikeButtonImage = new Uri(SpaceViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative);
                        }
                    }

                }
                else  // Internet bad.
                {
                    MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
                }

                // Set like button clicked false for next like button click.
                this.IsLikeButtonClicked = false;
            }
        }
    }
}