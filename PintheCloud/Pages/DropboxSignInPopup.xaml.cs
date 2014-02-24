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

namespace PintheCloud.Pages
{
    public partial class DropBoxSignInPopup : UserControl
    {
        private Popup p = null;


        public DropBoxSignInPopup(Popup p, string uri)
        {
            InitializeComponent();
            this.p = p;
            webBrowser.IsScriptEnabled = true;
            webBrowser.Navigate(new Uri(uri,UriKind.RelativeOrAbsolute));
        }


        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("http://54.214") &&e.Uri.ToString().Contains("54.214.19.198"))
            {
                p.IsOpen = false;
            }
        }
    }
}
