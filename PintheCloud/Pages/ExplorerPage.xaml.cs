using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Managers;
using System.Threading.Tasks;
using PintheCloud.ViewModels;
using PintheCloud.Models;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using PintheCloud.Resources;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using PintheCloud.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using PintheCloud.Converters;
using System.Windows.Media;
using PintheCloud.Helpers;
using System.Windows.Controls.Primitives;
using PintheCloud.Popups;

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PinPage
    {
        // Const Instances
        private const string PICK_PIVOT_TITLE_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_pick_title.png";
        private const string PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING = "00A4BF";

        private const string PIN_PIVOT_TITLE_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_pin_title.png";
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/pin_the_cloud/png/general_bar_upload.png";

        // Instances
        private int CurrentPlatformIndex = 0;

        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;
        private Popup SubmitSpotPasswordParentPopup = new Popup();

        public SpotViewModel NearSpotViewModel = new SpotViewModel();
        public FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();


        // Constructer
        public ExplorerPage()
        {
            InitializeComponent();

            // Set pin app bar button
            this.PinInfoAppBarButton.Text = AppResources.Pin;
            this.PinInfoAppBarButton.IconUri = new Uri(PIN_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PinInfoAppBarButton.IsEnabled = false;
            this.PinInfoAppBarButton.Click += PinInfoAppBarButton_Click;

            // Set Cloud Setting selection.
            this.AppBarMenuItems = new ApplicationBarMenuItem[App.IStorageManagers.Length];
            for (int i = 0; i < this.AppBarMenuItems.Length; i++)
            {
                this.AppBarMenuItems[i] = new ApplicationBarMenuItem();
                this.AppBarMenuItems[i].Text = App.IStorageManagers[i].GetStorageName();
                this.AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }

            // Check main platform and set current platform index.
            this.CurrentPlatformIndex = (int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY];

            // Set Datacontext
            uiNearSpotList.DataContext = this.NearSpotViewModel;
            uiPinInfoList.DataContext = this.FileObjectViewModel;

            // Set event by previous page
            Context currentContextPage = EventHelper.GetContext(EventHelper.EXPLORER_PAGE);
            currentContextPage.HandleEvent(EventHelper.FILE_LIST_PAGE, EventHelper.PIN_PIVOT, this.previous_FILE_LIST_pivot_PIN);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check if it is on the backstack from SplashPage and remove that.
            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();

            this.SetPickPivot(AppResources.Loading);
            base.SetPinPivot(App.IStorageManagers[this.CurrentPlatformIndex], uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, false);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        // Construct pivot item by index
        private void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly. One time loading.
            PhoneApplicationService.Current.State[PIVOT_KEY] = uiExplorerPivot.SelectedIndex;
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EventHelper.PICK_PIVOT:
                    // Set Pick Pivot UI
                    ApplicationBar.Buttons.Remove(this.PinInfoAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Remove(this.AppBarMenuItems[i]);
                    uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING));
                    uiPivotTitleImage.Source = new BitmapImage(new Uri(PICK_PIVOT_TITLE_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleIndicator.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    uiCurrentCloudModeImage.Visibility = Visibility.Collapsed;

                    this.SetPickPivot(AppResources.Loading);
                    break;


                case EventHelper.PIN_PIVOT:
                    // Set Pin Pivot UI
                    IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                    ApplicationBar.Buttons.Add(this.PinInfoAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
                    uiPivotTitleGrid.Background = new SolidColorBrush(
                        ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
                    uiPivotTitleImage.Source = new BitmapImage(new Uri(PIN_PIVOT_TITLE_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleIndicator.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    uiCurrentCloudModeImage.Source = new BitmapImage(
                        new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
                    uiCurrentCloudModeImage.Visibility = Visibility.Visible;

                    base.SetPinPivot(iStorageManager, uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                        uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, false);
                    break;
            }
        }


        private void SetPickPivot(string message)
        {
            // If Internet available, Set spot list
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!this.NearSpotViewModel.IsDataLoaded)  // Mutex check
                    this.SetNearSpotListAsync(message);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.InternetUnavailableMessage);
            }
        }


        private void previous_FILE_LIST_pivot_PIN()
        {
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;

            foreach (FileObjectViewItem fileObjectViewItem in this.FileObjectViewModel.Items)
            {
                if (!fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER)
                    && fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
            }
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EventHelper.PICK_PIVOT:
                    this.NearSpotViewModel.IsDataLoaded = false;
                    this.SetPickPivot(AppResources.Refreshing);
                    break;


                case EventHelper.PIN_PIVOT:
                    this.FileObjectViewModel.IsDataLoaded = false;
                    base.SetPinPivot(App.IStorageManagers[this.CurrentPlatformIndex], uiPinInfoList, uiPinInfoMessage, AppResources.Refreshing,
                        uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, true);
                    break;
            }
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
            PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
            EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            NavigationService.Navigate(new Uri(EventHelper.SETTINGS_PAGE, UriKind.Relative));
        }



        /*** Pick Pivot ***/

        // List select event
        private void uiNearSpotList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Spot View Item
            SpotViewItem spotViewItem = uiNearSpotList.SelectedItem as SpotViewItem;

            // Set selected item to null for next selection of list item. 
            uiNearSpotList.SelectedItem = null;

            // If it is private mode, get password from user and check it.
            // Otherwise, goto File list page.
            if (spotViewItem != null)  // Go to FIle List Page
            {
                if (spotViewItem.IsPrivateImage.Equals(FileObjectViewModel.IS_PRIVATE_IMAGE_URI))
                {
                    SubmitSpotPasswordPopup submitSpotPasswordPopup = 
                        new SubmitSpotPasswordPopup(this.SubmitSpotPasswordParentPopup, spotViewItem.SpotId, spotViewItem.SpotPassword, 
                            uiPickPivot.ActualWidth, uiPickPivot.ActualHeight, uiPivotTitleGrid.ActualHeight);
                    this.SubmitSpotPasswordParentPopup.Child = submitSpotPasswordPopup;
                    this.SubmitSpotPasswordParentPopup.IsOpen = true;
                    this.SubmitSpotPasswordParentPopup.Closed += (senderObject, args) =>
                    {
                        if (submitSpotPasswordPopup.result)
                        {
                            PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
                            PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                            string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                            NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE + parameters, UriKind.Relative));
                        }
                    };
                }
                else
                {
                    PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
                    PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                    string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                    NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE + parameters, UriKind.Relative));
                }
            }
        }


        private async void SetNearSpotListAsync(string message)
        {
            // Show progress indicator
            base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, message);
            base.SetProgressIndicator(true);

            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.Geolocator.LocationStatus != PositionStatus.Disabled)  // GPS is on
                {
                    // Check whether GPS works well or not
                    Geoposition currentGeoposition = await App.Geolocator.GetGeopositionAsync();
                    if (currentGeoposition != null)  // GPS works well
                    {
                        // If there is near spots, Clear and Add spots to list
                        // Otherwise, Show none message.
                        List<Spot> spots = await App.SpotManager.GetNearSpotViewItemsAsync(currentGeoposition);

                        if (spots != null)
                        {
                            if (spots.Count > 0)  // There are near spots
                            {
                                base.Dispatcher.BeginInvoke(() =>
                                {
                                    this.NearSpotViewModel.IsDataLoaded = true;
                                    uiNearSpotList.Visibility = Visibility.Visible;
                                    uiNearSpotMessage.Visibility = Visibility.Collapsed;
                                    this.NearSpotViewModel.SetItems(spots);
                                });
                            }
                            else  // No near spots
                            {
                                this.NearSpotViewModel.IsDataLoaded = true;
                                base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.NoNearSpotMessage);
                            }
                        }
                        else
                        {
                            base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.BadLoadingSpotMessage);
                        }
                    }
                    else  // GPS works bad
                    {
                        base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.BadLocationServiceMessage);
                    }
                }
                else  // GPS is off
                {
                    base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.NoLocationServiceMessage);
                }
            }
            else  // First or not consented of access in location information.
            {
                base.SetListUnableAndShowMessage(uiNearSpotList, uiNearSpotMessage, AppResources.NoLocationAcessConsentMessage);
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }



        /*** Pin Pivot ***/

        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;
            int currentPlatformIndex = base.GetPlatformIndexFromString(appBarMenuItem.Text);

            // If it is not in current cloud mode, change it.
            if (this.CurrentPlatformIndex != currentPlatformIndex && !App.IStorageManagers[this.CurrentPlatformIndex].IsSigningIn())
            {
                // Kill previous job.
                IStorageManager iStorageManager = App.IStorageManagers[currentPlatformIndex];
                uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
                uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
                this.CurrentPlatformIndex = currentPlatformIndex;

                this.FileObjectViewModel.IsDataLoaded = false;
                base.SetPinPivot(iStorageManager, uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                    uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, false);
            }
        }


        private void PinInfoAppBarButton_Click(object sender, EventArgs e)
        {
            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
                PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
                PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                NavigationService.Navigate(new Uri(EventHelper.NEW_SPOT_PAGE, UriKind.Relative));
            }
            else  // First or not consented of access in location information.
            {
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (iStorageManager.IsSigningIn())
            {
                e.Cancel = true;

                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            }
            else if (this.SubmitSpotPasswordParentPopup.IsOpen)
            {
                e.Cancel = true;
                this.SubmitSpotPasswordParentPopup.IsOpen = false;
            }
            else
            {
                // If some files in list, back tree.
                if (uiExplorerPivot.SelectedIndex == EventHelper.PIN_PIVOT)
                {
                    if (iStorageManager.GetFolderRootTree().Count > 1)
                    {
                        e.Cancel = true;
                        base.TreeUp(iStorageManager, uiPinInfoMessage, uiPinInfoCurrentPath, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile);
                        return;
                    }
                }

                MessageBoxResult result = MessageBox.Show(AppResources.CloseAppMessage, AppResources.CloseAppCaption, MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                    e.Cancel = true;
            }
        }


        private void uiPinInfoListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (iStorageManager.GetFolderRootTree().Count > 1)
                base.TreeUp(iStorageManager, uiPinInfoMessage, uiPinInfoCurrentPath, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile);
        }


        // Pin file selection event.
        private void uiPinInfoList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Object
            FileObjectViewItem fileObjectViewItem = uiPinInfoList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiPinInfoList.SelectedItem = null;


            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER))
                {
                    base.SetPinInfoListAsync(App.IStorageManagers[this.CurrentPlatformIndex], uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                        uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, fileObjectViewItem, true);
                }
                else  // Do selection if file
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PinInfoAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count < 1)
                            this.PinInfoAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                base.SetListUnableAndShowMessage(uiPinInfoList, uiPinInfoMessage, AppResources.DoingSignIn);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInPanel.Visibility = Visibility.Collapsed;
                });

                // Sign in and await that task.
                IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                if (!iStorageManager.IsSigningIn())
                    App.TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());
                bool result = await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (result)
                {
                    base.SetPinInfoListAsync(App.IStorageManagers[this.CurrentPlatformIndex], uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                        uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, null, true);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInPanel.Visibility = Visibility.Visible;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }
    }
}