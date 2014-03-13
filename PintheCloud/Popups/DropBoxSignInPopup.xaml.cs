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

namespace PintheCloud.Popups
{
    public partial class DropBoxSignInPopup : UserControl
    {
        private Popup Popup = null;


        public DropBoxSignInPopup(Popup popup, string uri)
        {
            InitializeComponent();
            this.Popup = popup;
            uiWebBrowser.Width = Application.Current.Host.Content.ActualWidth;
            uiWebBrowser.Height = Application.Current.Host.Content.ActualHeight;
            uiWebBrowser.Margin = new Thickness(0, 34, 0, 0);
            uiWebBrowser.IsScriptEnabled = true;
            uiWebBrowser.Navigate(new Uri(uri, UriKind.RelativeOrAbsolute));
        }


        private async void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("http://")
                && e.Uri.ToString().Contains("http://54.214.19.198"))
            {
                this.Popup.IsOpen = false;
                await uiWebBrowser.ClearCookiesAsync();
            }
        }
    }
}