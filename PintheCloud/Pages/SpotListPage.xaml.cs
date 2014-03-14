using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PintheCloud.Pages
{
    public partial class SpotList : PtcPage
    {
        private int x;
        public SpotList()
        {
            InitializeComponent();

            this.UISettings();
        }

        private void UISettings()
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        private void ui_spot_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }

        private void uiAppBarNewSpotButton_Click(object sender, System.EventArgs e)
        {
            // TODO: Add event handler implementation here.
        }

        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }

        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }

    }
}