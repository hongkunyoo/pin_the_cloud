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
using PintheCloud.Managers;
using System.Net.NetworkInformation;
using PintheCloud.Resources;

namespace PintheCloud.Pages
{
    public partial class SignInPage : PtcPage
    {
        public SignInPage()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private async void ui_signin_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                ui_signin_email.Text = "mark8625@daum.net";
                ui_signin_email.Text = ui_signin_email.Text.Trim();
                ui_signin_password.Password = ui_signin_password.Password.Trim();

                base.SetProgressIndicator(true, AppResources.DoingSignIn);
                try
                {
                    PtcAccount account = await App.AccountManager.GetPtcAccountAsync(ui_signin_email.Text, ui_signin_password.Password);
                    if (account != null)
                    {
                        App.AccountManager.SavePtcId(account.Email, account.ProfilePassword);
                        NavigationService.Navigate(new Uri(EventHelper.SIGNIN_STORAGE_PAGE, UriKind.Relative));
                    }
                    else
                    {
                        MessageBox.Show(AppResources.NoEmailAddressMessage, AppResources.NoEmailAddressCaption, MessageBoxButton.OK);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                }
                base.SetProgressIndicator(false);
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void ui_signin_email_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ui_signin_btn.IsEnabled = !IsTextBoxEmpty();
        }


        private void ui_signin_password_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            ui_signin_btn.IsEnabled = !IsTextBoxEmpty();
        }


        private bool IsTextBoxEmpty()
        {
            if (!ui_signin_email.Text.Trim().Equals(String.Empty) && !ui_signin_password.Password.Trim().Equals(String.Empty))
                return false;
            else
                return true;
        }
    }
}