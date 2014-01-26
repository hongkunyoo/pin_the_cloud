using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PhoneApplicationPage
    {
        public ExplorerPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Check if it is on the backstack from SplashPage and remove that.
            if (NavigationService.BackStack.Count() == 1)
                NavigationService.RemoveBackEntry();

            // If Internet is good, get new information from Internet,
            // Otherwise get old information from local sqlite.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // TODO get new information from Internet,
            }
            else
            {
                // TODO get old information from local sqlite.
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        // Move to Map Page
        private void uiAppBarMapButton_Click(object sender, System.EventArgs e)
        {
        	// TODO: 여기에 구현된 이벤트 처리기를 추가하십시오.
        }

        // Go to process of adding space and files
        private void uiAppBarAddButton_Click(object sender, System.EventArgs e)
        {
        	// TODO: 여기에 구현된 이벤트 처리기를 추가하십시오.
        }

        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(GlobalKeys.SETTINGS_PAGE, UriKind.Relative));
        }
    }
}