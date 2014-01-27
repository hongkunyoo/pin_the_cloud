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
                // If internet is good, update account information.
                // Otherwise, get account information with old one.
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    // TODO get new information from Internet,
                }
                else
                {
                    // TODO get old information from local sqlite.
                }



                await Task.Delay(TimeSpan.FromSeconds(1));
                NavigationService.Navigate(new Uri(PtcPage.EXPLORER_PAGE, UriKind.Relative));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void uiMicrosoftLoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet is good, go login, 
            // otherwise show no internet message box.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show progress indicator, progress login, hide indicator
                uiMicrosoftLoginButton.Content = AppResources.Wait;
                uiMicrosoftLoginButton.IsEnabled = false;
                bool loginResult = await App.AccountManager.LoginMicrosoftSingleSignOnAsync();

                // Move page or show fail message box by login result
                if (loginResult)
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
                MessageBox.Show(AppResources.NoInternetMessage, AppResources.NoInternetCaption, MessageBoxButton.OKCancel);
            }
        }
    }
}