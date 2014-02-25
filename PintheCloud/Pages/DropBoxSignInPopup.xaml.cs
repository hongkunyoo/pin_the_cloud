﻿using System;
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

namespace PintheCloud.Pages
{
    public partial class DropBoxSignInPopup : UserControl
    {
        private Popup popup = null;


        public DropBoxSignInPopup(Popup popup, string uri)
        {
            InitializeComponent();
            this.popup = popup;
            uiWebBrowser.Width = Application.Current.Host.Content.ActualWidth;
            uiWebBrowser.Height = Application.Current.Host.Content.ActualHeight;
            uiWebBrowser.Margin = new Thickness(0, 30, 0, 0);
            uiWebBrowser.IsScriptEnabled = true;
            uiWebBrowser.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
        }


        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(DropboxManager.DROPBOX_AUTH_URI.Split('.').First())
                && e.Uri.ToString().Contains(DropboxManager.DROPBOX_AUTH_URI.Split('/').Last()))
                this.popup.IsOpen = false;
        }
    }
}
