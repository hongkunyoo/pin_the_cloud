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

            // Check if it has backstacks, remove all
            int backStackCount = NavigationService.BackStack.Count();
            for (int i = 0; i < backStackCount; i++)
                NavigationService.RemoveBackEntry();

            // Get bool variable whether this account have logined or not.
            bool accountIsLogin = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.ACCOUNT_IS_LOGIN, out accountIsLogin);
            if (!accountIsLogin)  // First Login, Show Login Button.
            {
                uiMicrosoftLoginButton.Visibility = Visibility.Visible;
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
                        base.SetProgressIndicator(true);

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
                                    base.SetSystemTray(false);
                                    base.SetProgressIndicator(false);
                                    MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                                    uiMicrosoftLoginButton.Visibility = Visibility.Visible;
                                }
                            }
                        }
                        else
                        {
                            base.SetSystemTray(false);
                            base.SetProgressIndicator(false);
                            MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                            uiMicrosoftLoginButton.Visibility = Visibility.Visible;
                        }
                    }
                    else  // Get session fail
                    {
                        uiMicrosoftLoginButton.Visibility = Visibility.Visible;
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
                        MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                        uiMicrosoftLoginButton.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
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
                    uiMicrosoftLoginButton.IsEnabled = false;
                    uiMicrosoftLoginButton.Content = AppResources.Loading;

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
                            uiMicrosoftLoginButton.Content = AppResources.Login;
                            MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                        }
                    }
                    else
                    {
                        uiMicrosoftLoginButton.IsEnabled = true;
                        uiMicrosoftLoginButton.Content = AppResources.Login;
                        MessageBox.Show(AppResources.BadLoginMessage, AppResources.BadLoginCaption, MessageBoxButton.OK);
                    }
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.NoInternetCaption, MessageBoxButton.OK);
            }
        }
    }
}