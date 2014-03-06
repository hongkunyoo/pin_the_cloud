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

namespace PintheCloud.Pages
{
    public partial class NewSpotPage : PtcPage
    {
        // Const Instances
        private const int PIN_INFO_APP_BAR_BUTTON_INDEX = 0;

        // Instances
        private bool DeleteFileButton = false;

        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();
        


        public NewSpotPage()
        {
            InitializeComponent();

            // Set Pin info app bar button
            this.PinInfoAppBarButton = (ApplicationBarIconButton)ApplicationBar.Buttons[PIN_INFO_APP_BAR_BUTTON_INDEX];

            // Set name and datacontext
            uiSpotNameTextBox.Text = (string)App.ApplicationSettings[Account.ACCOUNT_DEFAULT_SPOT_NAME_KEY];
            uiNewSpotFileList.DataContext = this.FileObjectViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set list
            foreach (FileObjectViewItem fileObjectViewItem in (List<FileObjectViewItem>)PhoneApplicationService.Current.State[SELECTED_FILE_KEY])
                this.FileObjectViewModel.Items.Add(fileObjectViewItem);
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
        }


        private void uiSpotNameSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
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
        }


        private void uiPrivateModePasswordSetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            uiPrivateModePasswordTextBox.Text = uiPrivateModePasswordTextBox.Text.Trim();
        }



        /*** Client Logic ***/

        private void uiNewSpotFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Object
            FileObjectViewItem fileObjectViewItem = uiNewSpotFileList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiNewSpotFileList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If it is clicked from delete button, delete file
                if (this.DeleteFileButton)
                {
                    this.FileObjectViewModel.Items.Remove(fileObjectViewItem);
                    this.DeleteFileButton = false;
                    if (this.FileObjectViewModel.Items.Count < 1)
                    {
                        uiNewSpotFileListMessage.Visibility = Visibility.Visible;
                        uiNewSpotFileListMessage.Text = AppResources.NoSelectedFileMessage;
                        this.PinInfoAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        private void uiFileDeleteImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DeleteFileButton = true;
        }


        private void uiAppBarPinInfoButton_Click(object sender, System.EventArgs e)
        {
            string spotName = uiSpotNameTextBox.Text.Trim();
            if (!spotName.Equals(String.Empty))  // Spot name is not null
            {
                if (uiPrivateModePasswordGrid.Visibility == Visibility.Visible)  // Private is checked
                {
                    string password = uiPrivateModePasswordTextBox.Text.Trim();
                    if (!password.Equals(String.Empty))  // Password is not null
                    {
                        if(!password.Equals(NULL_PASSWORD))  // Password is not "null"
                            this.NavigateToFileListPage(spotName, true, password);
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
                    this.NavigateToFileListPage(spotName, false);
                }
            }
            else  // Spot name is null
            {
                MessageBox.Show(AppResources.NoSpotNameMessage, AppResources.NoSpotNameCaption, MessageBoxButton.OK);
            }
        }


        private void NavigateToFileListPage(string spotName, bool isPrivate, string password = NULL_PASSWORD)
        {
            // Check whether GPS is on or not
            if (App.Geolocator.LocationStatus != PositionStatus.Disabled)  // GPS is on
            {
                PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.FileObjectViewModel.Items.ToList<FileObjectViewItem>();
                NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE + "?spotName=" + spotName + "&private=" + isPrivate + "&password=" + AESHelper.Encrypt(password), UriKind.Relative));
            }
            else  // GPS is off
            {
                MessageBox.Show(AppResources.NoLocationServiceMessage, AppResources.NoLocationServiceCaption, MessageBoxButton.OK);
            }
        }
    }
}