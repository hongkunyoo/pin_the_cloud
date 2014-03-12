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

namespace PintheCloud.Pages
{
    public partial class ProfilePage : PhoneApplicationPage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void ui_create_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.

            if (!_CheckValidation())
            {
                ////////////////////////
                // TODO : SEUNG_MIN
                // Show ERROR MESSAGE
                ////////////////////////
                return;
            }
                

            PtcAccount ptcAccount = new PtcAccount();
            ptcAccount.Name = ui_name.Text;
            ptcAccount.Email = ui_email.Text;
            ptcAccount.PhoneNumber = ui_phone_num.Text;
            ptcAccount.ProfilePassword = ui_password.Text;

            // Insert to Server
            await App.AccountManager.InsertPtcAccountAsync(ptcAccount);
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
    }
}