using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Resources;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Models;
using PintheCloud.Managers;
using PintheCloud.Workers;
using PintheCloud.Utilities;
using Windows.Storage;
using System.Xml;
using System.IO;

namespace PintheCloud.Pages
{
    public partial class SplashPage : PtcPage
    {
        // 생성자
        public SplashPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // DEBUG MODE SETTING
            StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\user.xml");
            using (XmlReader reader = XmlReader.Create(await file.OpenStreamForReadAsync()))
            {
                reader.Read();
                reader.Read();
                reader.Read();
                reader.Read();
                GlobalKeys.USER = reader.Value.ToString().Trim();
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Check if it has backstacks, remove all
            int backStackCount = NavigationService.BackStack.Count();
            for (int i = 0; i < backStackCount; i++)
                NavigationService.RemoveBackEntry();

            // Get bool variable whether this account have logined or not.
            bool accountIsLogin = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_LOGIN, out accountIsLogin);
            if (!accountIsLogin)  // First Login, Show Login Button.
            {
                uiSplashLogo.Visibility = Visibility.Collapsed;
                uiLoginStackPanel.Visibility = Visibility.Visible;
                
                // It makes ERROR
                //await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalStorageManager.SKYDRIVE_FOLDER, CreationCollisionOption.FailIfExists);
                //await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalStorageManager.BLOBSTORAGE_FOLDER, CreationCollisionOption.FailIfExists);
            }
            else  // Second or more Login, Goto Explorer Page after some secconds.
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Get different Account Worker by internet state.
                if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                {
                    App.CurrentAccountManager.SetAccountWorker(new AccountInternetAvailableWorker());

                    // If Internet is good, get new information from Internet,
                    // Otherwise get old information from local storage.
                    if (await App.CurrentAccountManager.SetLiveConnectSessionAsync())  // Get session success
                    {

                        // Show progress indicator
                        base.SetSystemTray(true);
                        base.SetProgressIndicator(true, AppResources.Loading);

                        // If it success to register live connect session,
                        // Otherwise, Hide indicator, Show login fail message box.
                        if (await App.CurrentAccountManager.SetProfileResultAsync())
                        {
                            // If online progress login failed, retry with local storage information.
                            if (await App.CurrentAccountManager.LoginMicrosoftAccountSingleSignOnAsync())  // Login succeed
                            {
                                NavigationService.Navigate(new Uri(PtcPage.EXPLORER_PAGE, UriKind.Relative));
                            }
                            else  // Login fail
                            {
                                // Retry. If it succeed, Move to explorer page.
                                // Otherwise, Hide indicator, Show login fail message box.
                                App.CurrentAccountManager.SetAccountWorker(new AccountInternetUnavailableWorker());
                                if (await App.CurrentAccountManager.LoginMicrosoftAccountSingleSignOnAsync())
                                {
                                    NavigationService.Navigate(new Uri(PtcPage.EXPLORER_PAGE, UriKind.Relative));
                                }
                                else
                                {
                                    uiSplashLogo.Visibility = Visibility.Collapsed;
                                    uiLoginStackPanel.Visibility = Visibility.Visible;
                                    MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                                }
                            }
                        }
                        else
                        {
                            uiSplashLogo.Visibility = Visibility.Collapsed;
                            uiLoginStackPanel.Visibility = Visibility.Visible;
                            MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                        }

                        // Hide progress indicator
                        base.SetSystemTray(false);
                        base.SetProgressIndicator(false);
                    }
                    else  // Get session fail
                    {
                        uiSplashLogo.Visibility = Visibility.Collapsed;
                        uiLoginStackPanel.Visibility = Visibility.Visible;
                    }
                }

                else  // Internet unavailable
                {
                    App.CurrentAccountManager.SetAccountWorker(new AccountInternetUnavailableWorker());

                    // Login with local storage information and Move to explorer page.
                    if (await App.CurrentAccountManager.LoginMicrosoftAccountSingleSignOnAsync())
                    {
                        NavigationService.Navigate(new Uri(PtcPage.EXPLORER_PAGE, UriKind.Relative));
                    }
                    else
                    {
                        uiSplashLogo.Visibility = Visibility.Collapsed;
                        uiLoginStackPanel.Visibility = Visibility.Visible;
                        MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Other manager allocation
            App.SkyDriveManager = new SkyDriveManager(App.CurrentAccountManager.GetLiveConnectSession());
            App.BlobManager = new BlobManager();
        }

        private async void uiMicrosoftLoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Get different Account Worker by internet state.
            // If internet is good, progress login.
            // Otherwise, show warning message box.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                App.CurrentAccountManager.SetAccountWorker(new AccountInternetAvailableWorker());

                // If it success to register live connect session,
                if (await App.CurrentAccountManager.SetLiveConnectSessionAsync())
                {
                    // Show progress indicator, progress login
                    base.SetSystemTray(true);
                    base.SetProgressIndicator(true, AppResources.Loading);
                    uiMicrosoftLoginButton.IsEnabled = false;

                    // Get profile result and Login.
                    // If login succeed, Move to explorer page.
                    // Otherwise, Hide indicator, Show login fail message box.
                    if (await App.CurrentAccountManager.SetProfileResultAsync())
                    {
                        if (await App.CurrentAccountManager.LoginMicrosoftAccountSingleSignOnAsync())
                        {
                            NavigationService.Navigate(new Uri(PtcPage.EXPLORER_PAGE, UriKind.Relative));
                        }
                        else
                        {
                            uiMicrosoftLoginButton.IsEnabled = true;
                            MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        uiMicrosoftLoginButton.IsEnabled = true;
                        MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                    }

                    // Hide progress indicator
                    base.SetSystemTray(false);
                    base.SetProgressIndicator(false);
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.NoInternetCaption, MessageBoxButton.OK);
            }
        }
    }
}