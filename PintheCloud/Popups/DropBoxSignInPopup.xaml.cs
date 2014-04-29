using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using DropNet.Models;
using PintheCloud.Managers;
using PintheCloud.Pages;
using System.Threading.Tasks;

namespace PintheCloud.Popups
{
    public partial class DropBoxSignInPopup : UserControl
    {
        private Popup Popup;
        private int count;


        public DropBoxSignInPopup(Popup popup, string uri)
        {
            InitializeComponent();
            this.Popup = popup;
            this.count = 0;
            uiWebBrowser.Width = Application.Current.Host.Content.ActualWidth;
            uiWebBrowser.Height = Application.Current.Host.Content.ActualHeight;
            uiWebBrowser.Margin = new Thickness(0, PtcPage.STATUS_BAR_HEIGHT, 0, 0);
            uiWebBrowser.IsScriptEnabled = true;
            uiWebBrowser.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        public async Task ClearCache()
        {
            await uiWebBrowser.ClearInternetCacheAsync();
            await uiWebBrowser.ClearCookiesAsync();
        }


        private async void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            count++;
            //if (e.Uri.ToString().StartsWith("http://")
            //    && e.Uri.ToString().Contains(DropboxManager.DROPBOX_AUTH_URI))
            if(count == 3)
            {
                this.Popup.IsOpen = false;
                await uiWebBrowser.ClearCookiesAsync();
            }
        }
    }
}