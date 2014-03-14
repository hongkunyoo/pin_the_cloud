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

            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();
        }

        private async void ui_create_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            ui_email.Text = "mark8625@daum.net";

            if (!_CheckValidation())
            {
                ////////////////////////
                // TODO : SEUNGMIN
                // Show ERROR Message
                ////////////////////////
                return;
            }

            PtcAccount ptcAccount = new PtcAccount();
            ptcAccount.Name = ui_name.Text;
            ptcAccount.Email = ui_email.Text;
            ptcAccount.PhoneNumber = ui_phone_num.Text;
            ptcAccount.ProfilePassword = ui_password.Text;

            // Insert to Server
            bool result = await App.AccountManager.CreateNewPtcAccountAsync(ptcAccount);

            ////////////////////////
            // TODO : SEUNGMIN
            // Show Progress Status
            ////////////////////////

            if (result)
            {
                NavigationService.Navigate(new Uri(EventHelper.SIGNIN_STORAGE_PAGE, UriKind.Relative));
            }
            else // IF there is a duplicated Email address, it fails.
            {
                ////////////////////////
                // TODO : SEUNGMIN
                // Show ERROR message
                ////////////////////////
            }
        }

        private bool _CheckValidation()
        {
            if (string.Empty.Equals(ui_name.Text)) return false;
            if (!Regex.IsMatch(ui_email.Text,
                @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)))
                return false;
            if (string.Empty.Equals(ui_phone_num.Text)) return false;
            if (string.Empty.Equals(ui_password.Text)) return false;

            return true;
        }

        private void ui_sign_in_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            NavigationService.Navigate(new Uri(EventHelper.SIGNIN_PAGE, UriKind.Relative));
        }
    }
}