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
using PintheCloud.Workers;

namespace PintheCloud.Pages
{
    public partial class SettingsPage : PtcPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        // Logout
        private void uiLogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.CurrentAccountManager.Logout();
            NavigationService.Navigate(new Uri(PtcPage.SPLASH_PAGE, UriKind.Relative));
        }
    }
}