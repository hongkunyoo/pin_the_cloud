using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Helpers;
using System.Collections.ObjectModel;
using PintheCloud.Managers;
using System.Diagnostics;
using PintheCloud.ViewModels;
using PintheCloud.Resources;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace PintheCloud.Pages
{
    public partial class SignInStoragePage : PtcPage
    {
        private CloudModeViewModel CloudModeViewModel = new CloudModeViewModel();
        private Button[] SignButtons = null;



        public SignInStoragePage()
        {
            InitializeComponent();
            this.SignButtons = new Button[] { uiOneDriveSignButton, uiDropboxSignButton, uiGoogleDriveSignButton };
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            for (int i = 0; i < NavigationService.BackStack.Count(); i++)
                NavigationService.RemoveBackEntry();

            // Set Sign buttons and Set Main buttons.
            for (var i = 0; i < StorageHelper.GetStorageList().Count; i++)
            {
                IStorageManager storage = StorageHelper.GetStorageList()[i];
                this.SetSignButtons(i, storage.IsSignIn(), storage);
            }
            
        }


        private async void CloudSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Set process indicator
                base.Dispatcher.BeginInvoke(() =>
                {
                    ui_skip_btn.IsEnabled = false;
                    uiCloudPanel.Visibility = Visibility.Collapsed;
                    uiCloudMessage.Visibility = Visibility.Visible;
                });

                // Get index
                Button signButton = (Button)sender;
                Switcher.SetStorageTo(signButton.Tag.ToString());

                // Sign in
                IStorageManager iStorageManager = Switcher.GetCurrentStorage();
                if (!iStorageManager.IsSigningIn())
                    TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (await TaskHelper.WaitSignInTask(iStorageManager.GetStorageName()))
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        Switcher.SetMainPlatform(signButton.Tag.ToString());
                        MessageBoxResult result = MessageBox.Show(AppResources.StartMessage, AppResources.StartCaption, MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                            NavigationService.Navigate(new Uri(EventHelper.SPOT_LIST_PAGE, UriKind.Relative));
                    });
                }
                else
                {
                    // Hide process indicator
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        ui_skip_btn.IsEnabled = true;
                        uiCloudPanel.Visibility = Visibility.Visible;
                        uiCloudMessage.Visibility = Visibility.Collapsed;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void SetSignButtons(int platformIndex, bool isSignIn, IStorageManager iStorageManager)
        {
            this.SignButtons[platformIndex].Click += this.CloudSignInButton_Click;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void ui_skip_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(AppResources.StartMessage, AppResources.StartCaption, MessageBoxButton.OK);
            if (result == MessageBoxResult.OK)
                NavigationService.Navigate(new Uri(EventHelper.SPOT_LIST_PAGE, UriKind.Relative));
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);


            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (iStorageManager.IsSigningIn())
            {
                e.Cancel = true;

                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(AppResources.CloseAppMessage, AppResources.CloseAppCaption, MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                    e.Cancel = true;
            }
        }
    }
}