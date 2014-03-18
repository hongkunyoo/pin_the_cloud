using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Models;
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using Windows.Devices.Geolocation;
using PintheCloud.Managers;
using PintheCloud.Helpers;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class NewSpotPage : PtcPage
    {
        // Instances
        private string SpotId = null;


        public NewSpotPage()
        {
            InitializeComponent();

            // Set name and datacontext
            uiSpotNameTextBox.Hint = (string)App.ApplicationSettings[StorageAccount.ACCOUNT_DEFAULT_SPOT_NAME_KEY];
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }



        /*** Spot Name UI Event Handler ***/

        private void uiSpotNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (uiSpotNameTextBox.Text.Trim().Length > 0)
                uiSpotNameSetButton.IsEnabled = true;
            else
                uiSpotNameSetButton.IsEnabled = false;
        }


        private void uiSpotNameTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            uiSpotNameSetButton.Visibility = Visibility.Visible;
        }

        private void uiSpotNameTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            uiSpotNameSetButton.Visibility = Visibility.Collapsed;
            uiSpotNameTextBox.Text = uiSpotNameTextBox.Text.Trim();
        }


        private void uiSpotNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (uiSpotNameSetButton.IsEnabled)
                uiSpotNameTextBox.Text = uiSpotNameTextBox.Text.Trim();
        }



        /*** Private Mode Password UI Event Handler ***/

        private void uiPrivateModeToggleSwitchButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            uiPrivateModePasswordGrid.Visibility = Visibility.Visible;
        }


        private void uiPrivateModeToggleSwitchButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            uiPrivateModePasswordGrid.Visibility = Visibility.Collapsed;
        }


        private void uiPrivateModePasswordTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (uiPrivateModePasswordTextBox.Text.Trim().Length > 0)
                uiPrivateModePasswordSetButton.IsEnabled = true;
            else
                uiPrivateModePasswordSetButton.IsEnabled = false;
        }


        private void uiPrivateModePasswordTextBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            uiPrivateModePasswordSetButton.Visibility = Visibility.Visible;
        }


        private void uiPrivateModePasswordTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            uiPrivateModePasswordSetButton.Visibility = Visibility.Collapsed;
            uiPrivateModePasswordTextBox.Text = uiPrivateModePasswordTextBox.Text.Trim();
        }


        private void uiPrivateModePasswordSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (uiPrivateModePasswordSetButton.IsEnabled)
                uiPrivateModePasswordTextBox.Text = uiPrivateModePasswordTextBox.Text.Trim();
        }



        /*** Client Logic ***/

        private void uiAppBarMakeSpotButton_Click(object sender, System.EventArgs e)
        {
            // Get spot name from text or hint which is default spot name.
            uiSpotNameTextBox.Text = uiSpotNameTextBox.Text.Trim();
            string spotName = uiSpotNameTextBox.Text;
            if (spotName.Equals(String.Empty))
                spotName = uiSpotNameTextBox.Hint;

            // If Private is checked, get password and go to upload.
            // Otherwise, go upload.
            if (uiPrivateModePasswordGrid.Visibility == Visibility.Visible)
            {
                uiPrivateModePasswordTextBox.Text = uiPrivateModePasswordTextBox.Text.Trim();
                string password = uiPrivateModePasswordTextBox.Text;
                if (!password.Equals(String.Empty))  // Password is not null
                {
                    if (!password.Equals(NULL_PASSWORD))  // Password is not "null"
                        this.MakeNewSpot(spotName, true, password);
                    else  // Password is "null"
                        MessageBox.Show(AppResources.PasswordNullMessage, AppResources.PasswordNullCaption, MessageBoxButton.OK);
                }
                else  // Password is null
                {
                    MessageBox.Show(AppResources.NoPasswordMessage, AppResources.NoPasswordCption, MessageBoxButton.OK);
                }
            }
            else  // private is not checked
            {
                this.MakeNewSpot(spotName, false);
            }
        }


        private async void MakeNewSpot(string spotName, bool isPrivate, string spotPassword = NULL_PASSWORD)
        {
            StorageAccount account = Switcher.GetCurrentStorage().GetStorageAccount();
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Check whether GPS is on or not
                if (App.Geolocator.LocationStatus != PositionStatus.Disabled)  // GPS is on
                {
                    if (await this.PinSpotAsync(spotName, account.Id, account.StorageName, isPrivate, spotPassword))
                    {
                        string parameters = "?spotId=" + this.SpotId + "&spotName=" + spotName + "&accountId=" + account.Id + "&accountName=" + account.StorageName;
                        NavigationService.Navigate(new Uri(EventHelper.EXPLORER_PAGE + parameters, UriKind.Relative));
                    }
                }
                else
                {
                    MessageBox.Show(AppResources.NoLocationServiceMessage, AppResources.NoLocationServiceCaption, MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async Task<bool> PinSpotAsync(string spotName, string accountId, string accountName, bool isPrivate, string spotPassword)
        {
            // Show Pining message and Progress Indicator
            base.Dispatcher.BeginInvoke(() =>
            {
                uiNewSpotMessage.Text = AppResources.PiningSpot;
                uiNewSpotMessage.Visibility = Visibility.Visible;
            });
            
            base.SetProgressIndicator(true);

            // Pin spot
            Geoposition geo = await App.Geolocator.GetGeopositionAsync();
            //Spot spot = new Spot(spotName, geo.Coordinate.Latitude, geo.Coordinate.Longitude, accountId, accountName, 0, isPrivate, spotPassword);
            SpotObject spotObject = new SpotObject(spotName, geo.Coordinate.Latitude, geo.Coordinate.Longitude, accountId, accountName, 0, isPrivate, spotPassword, DateTime.Now.ToString());
            //bool result = await App.SpotManager.PinSpotAsync(spot);
            bool result = await App.SpotManager.CreateSpotAsync(spotObject);
            if (result)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    ((SpotViewModel)PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY]).IsDataLoaded = false;
                    this.SpotId = spotObject.Id;
                });
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiNewSpotMessage.Text = AppResources.BadPinSpotMessage;
                });
            }

            // Hide progress message
            base.SetProgressIndicator(false);
            return result;
        }
    }
}