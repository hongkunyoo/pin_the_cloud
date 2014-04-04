using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Text.RegularExpressions;
using PintheCloud.Models;
using PintheCloud.Managers;
using System.Net.NetworkInformation;
using PintheCloud.Resources;
using Microsoft.WindowsAzure.MobileServices;

namespace PintheCloud.Pages
{
    public partial class ProfilePage : PtcPage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationService.RemoveBackEntry();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private async void ui_create_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                ui_name.Text = ui_name.Text.Trim();
                ui_email.Text = ui_email.Text.Trim();
                ui_password.Password = ui_password.Password.Trim();

                // Email Check
                if (!Regex.IsMatch(ui_email.Text,
                        @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))"
                        + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
                        RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                {
                    MessageBox.Show(AppResources.BadEmailAddressMessage, AppResources.BadEmailAddressCaption, MessageBoxButton.OK);
                    ui_password.Password = String.Empty;
                    return;
                }


                base.SetProgressIndicator(true, AppResources.CreateProfile);

                ui_name.IsEnabled = false;
                ui_email.IsEnabled = false;
                ui_password.IsEnabled = false;
                ui_create_btn.IsEnabled = false;
                ui_sign_in_btn.IsEnabled = false;

                PtcAccount ptcAccount = new PtcAccount();
                ptcAccount.Name = ui_name.Text;
                ptcAccount.Email = ui_email.Text;
                ptcAccount.ProfilePassword = ui_password.Password;

                try
                {
                    if (await App.AccountManager.CreateNewPtcAccountAsync(ptcAccount))
                    {
                        NavigationService.Navigate(new Uri(EventHelper.SIGNIN_STORAGE_PAGE, UriKind.Relative));
                    }
                    else // IF there is a duplicated Email address, it fails.
                    {
                        ui_name.IsEnabled = true;
                        ui_email.IsEnabled = true;
                        ui_password.IsEnabled = true;
                        ui_password.Password = String.Empty;
                        MessageBox.Show(AppResources.DuplicateEmailAddressMessage, AppResources.DuplicateEmailAddressCaption, MessageBoxButton.OK);
                    }
                }
                catch (MobileServiceInvalidOperationException)
                {
                    ui_name.IsEnabled = true;
                    ui_email.IsEnabled = true;
                    ui_password.IsEnabled = true;
                    ui_password.Password = String.Empty;
                    MessageBox.Show(AppResources.BadCreateProfileMessage, AppResources.BadCreateProfileCaption, MessageBoxButton.OK);
                }
                base.SetProgressIndicator(false);
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void ui_sign_in_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(EventHelper.SIGNIN_PAGE, UriKind.Relative));
        }


        private void ui_name_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ui_create_btn.IsEnabled = !IsTextBoxEmpty();
        }


        private void ui_email_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ui_create_btn.IsEnabled = !IsTextBoxEmpty();
        }


        private void ui_password_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            ui_create_btn.IsEnabled = !IsTextBoxEmpty();
        }


        private bool IsTextBoxEmpty()
        {
            if (!ui_name.Text.Trim().Equals(String.Empty) && !ui_email.Text.Trim().Equals(String.Empty) &&
                !ui_password.Password.Trim().Equals(String.Empty))
                return false;
            else
                return true;
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            MessageBoxResult result = MessageBox.Show(AppResources.CloseAppMessage, AppResources.CloseAppCaption, MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
                e.Cancel = true;
        }
    }
}