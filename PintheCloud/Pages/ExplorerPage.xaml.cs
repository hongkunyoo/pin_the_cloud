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
using PintheCloud.Managers;
using System.Threading.Tasks;
using PintheCloud.Workers;
using PintheCloud.ViewModels;
using PintheCloud.Models;
using Microsoft.WindowsAzure.MobileServices;
using System.Collections.ObjectModel;

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Instances
        private const int EXPLORER_PIVOT = 0;
        private const int RECENT_PIVOT = 1;
        private const int MY_SPACES_PIVOT = 2;

        public SpaceViewModel CurrentSpaceViewModel = new SpaceViewModel();


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

            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {

            }
            else  // First or not consented of access in location information.
            {
                
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        // Construct pivot item by page index
        private async void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get different Space Worker by internet state.
            if (NetworkInterface.GetIsNetworkAvailable()) //  Internet available.
                App.CurrentSpaceManager.SetAccountWorker(new SpaceInternetAvailableWorker());
            else  // Internet bad.
                App.CurrentSpaceManager.SetAccountWorker(new SpaceInternetUnavailableWorker());
            
            // Set View model for dispaly,
            switch (uiExplorerPivot.SelectedIndex)
            { 
                case EXPLORER_PIVOT:
                    // TODO load near space use GPS information
                    break;

                case RECENT_PIVOT:
                    break;

                case MY_SPACES_PIVOT:

                    // If there is spaces, Clear and Add spaces to list
                    // Otherwise, Show none message.
                    base.SetProgressIndicator(true);
                    ObservableCollection<SpaceViewItem> items = await App.CurrentSpaceManager.GetMySpaceViewItemsAsync();
                    if (items != null)
                    {
                        uiMySpaceList.Visibility = Visibility.Visible;
                        uiNoMySpaceMessage.Visibility = Visibility.Collapsed;
                        CurrentSpaceViewModel.Items = items;
                        this.DataContext = CurrentSpaceViewModel;
                    }
                    else
                    {
                        uiMySpaceList.Visibility = Visibility.Collapsed;
                        uiNoMySpaceMessage.Visibility = Visibility.Visible;
                    }
                    base.SetProgressIndicator(false);
                    break;
            }
        }

        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri(PtcPage.SETTINGS_PAGE, UriKind.Relative));
        }
    }
}