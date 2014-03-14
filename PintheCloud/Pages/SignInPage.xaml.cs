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

namespace PintheCloud.Pages
{
    public partial class SignInPage : PtcPage
    {
        public SignInPage()
        {
            InitializeComponent();
        }

        private async void ui_signin_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            ui_signin_email.Text = "mark8625@daum.net";
            ui_signin_password.Password = "Check Profile";


            string id = ui_signin_email.Text;
            string password = ui_signin_password.Password;
            PtcAccount account = await App.AccountManager.GetPtcAccountAsync(id, password);

            if (account != null)
            {
                App.AccountManager.SavePtcId(account.Email, account.ProfilePassword);
                NavigationService.Navigate(new Uri(EventHelper.SIGNIN_STORAGE_PAGE, UriKind.Relative));
            }

            ////////////////////////
            // TODO : SEUNGMIN
            // Show ERROR message
            ////////////////////////

        }
    }
}