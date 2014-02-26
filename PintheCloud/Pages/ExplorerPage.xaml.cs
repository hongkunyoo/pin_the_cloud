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

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Const Instances
        private const string PICK_PIVOT_TITLE_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_pick_title.png";
        private const string PIN_PIVOT_TITLE_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_pin_title.png";
        private const string PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING = "00A4BF";
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/pin_the_cloud/png/general_bar_upload.png";

        // Instances
        private int MainPlatformIndex = 0;
        private int CurrentPlatformIndex = 0;
        //private bool isSignIning = false;

        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;

        public SpotViewModel NearSpotViewModel = new SpotViewModel();
        public FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();

        private Stack<List<FileObject>> FoldersTree = new Stack<List<FileObject>>();
        private Stack<FileObjectViewItem> FolderRootTree = new Stack<FileObjectViewItem>();
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
            this.MainPlatformIndex = (int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY];
            this.CurrentPlatformIndex = this.MainPlatformIndex;
            uiCurrentCloudModeImage.Source = new BitmapImage(
                new Uri(App.IStorageManagers[this.MainPlatformIndex].GetStorageImageUri(), UriKind.Relative));

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

            this.SetPickPivot();
            this.SetPinPivot();
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
                    uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHex(PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING));
                    uiPivotTitleImage.Source = new BitmapImage(new Uri(PICK_PIVOT_TITLE_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleIndicator.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    uiCurrentCloudModeImage.Visibility = Visibility.Collapsed;
                    this.SetPickPivot();
                    break;


                case EventHelper.PIN_PIVOT:
                    // Set Pin Pivot UI
                    ApplicationBar.Buttons.Add(this.PinInfoAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
                    uiPivotTitleGrid.Background = new SolidColorBrush(
                        ColorHexStringToBrushConverter.GetColorFromHex(App.IStorageManagers[this.CurrentPlatformIndex].GetStorageColorHexString()));
                    uiPivotTitleImage.Source = new BitmapImage(new Uri(PIN_PIVOT_TITLE_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleIndicator.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    uiCurrentCloudModeImage.Visibility = Visibility.Visible;
                    this.SetPinPivot();
                    break;
            }
        }


        private void SetPickPivot()
        {
            // If Internet available, Set spot list
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!this.NearSpotViewModel.IsDataLoaded)  // Mutex check
                    this.SetNearSpotListAsync(AppResources.Loading);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiNearSpotList, AppResources.InternetUnavailableMessage, uiNearSpotMessage);
            }
        }


        private void SetPinPivot()
        {
            // If it wasn't already signed in, show signin button.
            // Otherwise, load files
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (!iStorageManager.IsSignIn())  // wasn't signed in.
            {
                this.FolderRootTree.Clear();
                this.FoldersTree.Clear();
                this.SelectedFile.Clear();
                this.PinInfoAppBarButton.IsEnabled = false;

                uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                uiPinInfoSignInGrid.Visibility = Visibility.Visible;
            }
            else  // already signed in.
            {
                uiPinInfoListGrid.Visibility = Visibility.Visible;
                uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (!this.FileObjectViewModel.IsDataLoaded)
                        this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }
        }


        private void previous_FILE_LIST_pivot_PIN()
        {
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;

            foreach (FileObjectViewItem fileObjectViewItem in this.FileObjectViewModel.Items)
            {
                if (!fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER)
                    && fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
            }
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EventHelper.PICK_PIVOT:
                    if (NetworkInterface.GetIsNetworkAvailable())
                        this.SetNearSpotListAsync(AppResources.Refreshing);
                    else
                        base.SetListUnableAndShowMessage(uiNearSpotList, AppResources.InternetUnavailableMessage, uiNearSpotMessage);
                    break;


                case EventHelper.PIN_PIVOT:
                    // If it wasn't already signed in, show signin button.
                    // Otherwise, load files
                    IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                    if (!iStorageManager.IsSignIn())  // wasn't signed in.
                    {
                        this.FolderRootTree.Clear();
                        this.FoldersTree.Clear();
                        this.SelectedFile.Clear();
                        this.PinInfoAppBarButton.IsEnabled = false;

                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    }
                    else  // already signed in.
                    {
                        uiPinInfoListGrid.Visibility = Visibility.Visible;
                        uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                        if (NetworkInterface.GetIsNetworkAvailable())
                        {
                            if (this.FolderRootTree.Count > 0)
                                this.SetPinInfoListAsync(this.FolderRootTree.First(), AppResources.Refreshing, iStorageManager);
                            else
                                this.SetPinInfoListAsync(null, AppResources.Refreshing, iStorageManager);
                        }
                        else
                        {
                            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                        }
                    }
                    break;
            }
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
            PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
            NavigationService.Navigate(new Uri(EventHelper.SETTINGS_PAGE, UriKind.Relative));
        }


        public bool GetLocationAccessConsent()
        {
            if (!((bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY]))  // First or not consented of access in location information.
            {
                MessageBoxResult result = MessageBox.Show(AppResources.LocationAccessMessage, AppResources.LocationAccessCaption, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = true;
                else
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = false;
                App.ApplicationSettings.Save();
            }
            return (bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY];
        }



        /*** Pick Pivot ***/

        // List select event
        private void uiNearSpotList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected Spot View Item
            SpotViewItem spotViewItem = uiNearSpotList.SelectedItem as SpotViewItem;

            // Set selected item to null for next selection of list item. 
            uiNearSpotList.SelectedItem = null;


            // If selected item isn't null and it doesn't come from like button, goto File list page.
            // Otherwise, Process Like or Not Like by current state
            if (spotViewItem != null)  // Go to FIle List Page
            {
                PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
                PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                string parameters = base.GetParameterStringFromSpotViewItem(spotViewItem);
                NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE + parameters, UriKind.Relative));
            }
        }


        private async void SetNearSpotListAsync(string message)
        {
            // Show progress indicator
            base.SetListUnableAndShowMessage(uiNearSpotList, message, uiNearSpotMessage);
            base.SetProgressIndicator(true);

            // Check whether user consented for location access.
            if (this.GetLocationAccessConsent())  // Got consent of location access.
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
                        JArray spots = await App.SpotManager.GetNearSpotViewItemsAsync(currentGeoposition);

                        if (spots != null)  // There are near spots
                        {
                            base.Dispatcher.BeginInvoke(() =>
                            {
                                this.NearSpotViewModel.IsDataLoaded = true;
                                uiNearSpotList.Visibility = Visibility.Visible;
                                uiNearSpotMessage.Visibility = Visibility.Collapsed;
                                this.NearSpotViewModel.SetItems(spots, false);
                            });
                        }
                        else  // No near spots
                        {
                            this.NearSpotViewModel.IsDataLoaded = true;
                            base.SetListUnableAndShowMessage(uiNearSpotList, AppResources.NoNearSpotMessage, uiNearSpotMessage);
                        }
                    }
                    else  // GPS works bad
                    {
                        base.SetListUnableAndShowMessage(uiNearSpotList, AppResources.BadLocationServiceMessage, uiNearSpotMessage);
                    }
                }
                else  // GPS is off
                {
                    base.SetListUnableAndShowMessage(uiNearSpotList, AppResources.NoLocationServiceMessage, uiNearSpotMessage);
                }
            }
            else  // First or not consented of access in location information.
            {
                base.SetListUnableAndShowMessage(uiNearSpotList, AppResources.NoLocationAcessConsentMessage, uiNearSpotMessage);
            }

            // Hide progress indicator
            base.SetProgressIndicator(false);
        }



        /*** Pin Pivot ***/

        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;
            int platformIndex = base.GetPlatformIndex(appBarMenuItem.Text);


            // If it is not in current cloud mode, change it.
            if (this.CurrentPlatformIndex != platformIndex && !this.FileObjectViewModel.IsDataLoading && !App.IStorageManagers[this.CurrentPlatformIndex].IsSigningIn())
            {
                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHex(iStorageManager.GetStorageColorHexString()));
                uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
                this.CurrentPlatformIndex = platformIndex;

                // If it is already signin, load files.
                // Otherwise, show signin button.
                if (!iStorageManager.IsSignIn())
                {
                    this.FolderRootTree.Clear();
                    this.FoldersTree.Clear();
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;

                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    // Show Loading message and save is login true for pivot moving action while sign in.
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;

                    if (NetworkInterface.GetIsNetworkAvailable())
                        this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
                    else
                        base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }
        }


        private void PinInfoAppBarButton_Click(object sender, EventArgs e)
        {
            // Check whether user consented for location access.
            if (this.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.Geolocator.LocationStatus != PositionStatus.Disabled)  // GPS is on
                {
                    PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
                    PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
                    PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                    PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                    NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE, UriKind.Relative));
                }
                else  // GPS is off
                {
                    MessageBox.Show(AppResources.NoLocationServiceMessage, AppResources.NoLocationServiceCaption, MessageBoxButton.OK);
                }
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
            if (App.IStorageManagers[this.CurrentPlatformIndex].IsSigningIn())
            {
                // If it is popup, close popup.
                if (App.IStorageManagers[this.CurrentPlatformIndex].IsPopup())
                {
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                // If some files in list, back tree.
                if (uiExplorerPivot.SelectedIndex == EventHelper.PIN_PIVOT)
                {
                    if (this.FolderRootTree.Count > 1)
                    {
                        e.Cancel = true;
                        this.TreeUp();
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
            if (this.FolderRootTree.Count > 1)
                this.TreeUp();
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
                    this.SetPinInfoListAsync(fileObjectViewItem, AppResources.Loading, App.IStorageManagers[this.CurrentPlatformIndex]);
                }
                else  // Do selection if file
                {
                    if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PinInfoAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectCheckImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectCheckImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count < 1)
                            this.PinInfoAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        private string GetCurrentPath()
        {
            FileObjectViewItem[] array = this.FolderRootTree.Reverse<FileObjectViewItem>().ToArray<FileObjectViewItem>();
            string str = String.Empty;
            foreach (FileObjectViewItem f in array)
                str += f.Name + "/";
            return str;
        }


        private void TreeUp()
        {
            // If message is visible, set collapsed.
            if (uiPinInfoMessage.Visibility == Visibility.Visible)
                uiPinInfoMessage.Visibility = Visibility.Collapsed;

            // Clear trees.
            this.FolderRootTree.Pop();
            this.FoldersTree.Pop();
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;

            // Set previous files to list.
            this.FileObjectViewModel.SetItems(this.FoldersTree.First(), true);
            uiPinInfoCurrentPath.Text = this.GetCurrentPath();
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.DoingSignIn, uiPinInfoMessage);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
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
                    this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void SetPinInfoListAsync(FileObjectViewItem folder, string message, IStorageManager iStorageManager)
        {
            // Set Mutex true and Show Process Indicator
            this.FileObjectViewModel.IsDataLoading = true;
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            base.SetProgressIndicator(true);

            // Clear selected file and set pin button false.
            this.SelectedFile.Clear();
            base.Dispatcher.BeginInvoke(() =>
            {
                this.PinInfoAppBarButton.IsEnabled = false;
            });

            // Wait task
            await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
            await App.TaskHelper.WaitSignOutTask(iStorageManager.GetStorageName());

            // If it wasn't signed out, set list.
            // Othersie, show sign in grid.
            if (iStorageManager.GetAccount() != null)  // Wasn't signed out.
            {
                // If folder null, set root.
                if (folder == null)
                {
                    this.FolderRootTree.Clear();
                    this.FoldersTree.Clear();
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoCurrentPath.Text = "";
                    });

                    FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
                    folder = new FileObjectViewItem();
                    folder.Id = rootFolder.Id;
                }


                // Get files and push to stack tree.
                List<FileObject> files = await iStorageManager.GetFilesFromFolderAsync(folder.Id);
                if (!message.Equals(AppResources.Refreshing))
                {
                    this.FolderRootTree.Push(folder);
                    this.FoldersTree.Push(files);
                }

                // Set file list visible and current path.
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.FileObjectViewModel.IsDataLoaded = true;
                    uiPinInfoList.Visibility = Visibility.Visible;
                    uiPinInfoCurrentPath.Text = this.GetCurrentPath();
                    this.FileObjectViewModel.SetItems(files, true);
                });

                // If there exists file, show it.
                // Otherwise, show no file message.
                if (files.Count > 0)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoMessage.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoMessage.Text = AppResources.NoFileInFolderMessage;
                    });
                }
            }
            else  // Was signed out.
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                });
            }


            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
            this.FileObjectViewModel.IsDataLoading = false;
        }
    }
}