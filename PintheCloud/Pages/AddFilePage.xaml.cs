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
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using System.Windows.Media;
using PintheCloud.Converters;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Models;
using PintheCloud.Helpers;

namespace PintheCloud.Pages
{
    public partial class AddFilePage : PinPage
    {
        // Const Instances
        private int PIN_INFO_APP_BAR_BUTTON_INDEX = 1;

        // Instances
        private int CurrentPlatformIndex = 0;
        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;

        public FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();



        public AddFilePage()
        {
            InitializeComponent();

            // Set Pin Infor app bar button and Cloud Setting selection.
            this.PinInfoAppBarButton = (ApplicationBarIconButton)ApplicationBar.Buttons[PIN_INFO_APP_BAR_BUTTON_INDEX];
            this.AppBarMenuItems = new ApplicationBarMenuItem[App.StorageManagerNames.Length];
            for (int i = 0; i < this.AppBarMenuItems.Length; i++)
            {
                this.AppBarMenuItems[i] = new ApplicationBarMenuItem();
                this.AppBarMenuItems[i].Text = App.StorageManagerNames[i];
                this.AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }

            // Check main platform and set current platform index.
            //this.CurrentPlatformIndex = (int)App.ApplicationSettings[StorageAccount.ACCOUNT_MAIN_PLATFORM_TYPE_KEY];


            // Set Datacontext
            uiPinInfoList.DataContext = this.FileObjectViewModel;

        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set Pin Pivot UI
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            for (int i = 0; i < AppBarMenuItems.Length; i++)
                ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
            uiPivotTitleGrid.Background = new SolidColorBrush(
                ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
            uiCurrentCloudModeImage.Source = new BitmapImage(
                new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
            base.SetPinPivot(iStorageManager, uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, false);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void uiAppBarPinInfoButton_Click(object sender, System.EventArgs e)
        {
            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                //PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                NavigationService.GoBack();
            }
            else  // First or not consented of access in location information.
            {
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            this.FileObjectViewModel.IsDataLoaded = false;
            base.SetPinPivot(Switcher.GetCurrentStorage(), uiPinInfoList, uiPinInfoMessage, AppResources.Refreshing,
                uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, true);
        }


        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;
            //int currentPlatformIndex = base.GetPlatformIndexFromString(appBarMenuItem.Text);
            if (Switcher.GetCurrentStorage().GetStorageName().Equals(appBarMenuItem.Text)) return;

            Switcher.SetStorageTo(appBarMenuItem.Text);

            // If it is not in current cloud mode, change it.

            // Kill previous job.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
            uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
            //this.CurrentPlatformIndex = currentPlatformIndex;

            this.FileObjectViewModel.IsDataLoaded = false;
            base.SetPinPivot(Switcher.GetCurrentStorage(), uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
                uiPinInfoCurrentPath, uiPinInfoListGrid, uiPinInfoSignInPanel, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile, false);
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (iStorageManager.IsSigningIn())
            {
                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                {
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                // If some files in list, back tree.
                if (iStorageManager.GetFolderRootTree().Count > 1)
                {
                    e.Cancel = true;
                    base.TreeUp(iStorageManager, uiPinInfoMessage, uiPinInfoCurrentPath, this.PinInfoAppBarButton, this.FileObjectViewModel, this.SelectedFile);
                }
                else
                {
                    this.SelectedFile.Clear();
                    PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                    //PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                }
            }
        }


        private void uiPinInfoListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
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
                    base.SetPinInfoListAsync(Switcher.GetCurrentStorage(), uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
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
                IStorageManager iStorageManager = Switcher.GetCurrentStorage();
                if (!iStorageManager.IsSigningIn())
                    App.TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());
                bool result = await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (result)
                {
                    base.SetPinInfoListAsync(iStorageManager, uiPinInfoList, uiPinInfoMessage, AppResources.Loading,
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