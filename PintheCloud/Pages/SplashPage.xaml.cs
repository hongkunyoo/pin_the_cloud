using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Resources;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Models;
using PintheCloud.Managers;
using PintheCloud.Utilities;
using Windows.Storage;
using System.Xml;
using System.IO;
using System.Threading;

namespace PintheCloud.Pages
{
    public partial class SplashPage : PtcPage
    {
        // 생성자
        public SplashPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check main platform at frist login.
            if (!App.ApplicationSettings.Contains(Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY))
            {
                App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY] = Account.StorageAccountType.ONE_DRIVE;
                App.ApplicationSettings.Save();
            }

            // Check nick name at frist login.
            if (!App.ApplicationSettings.Contains(Account.ACCOUNT_NICK_NAME_KEY))
            {
                App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY] = AppResources.AtHere;
                App.ApplicationSettings.Save();
            }

            // Check location access consent at frist login.
            if (!App.ApplicationSettings.Contains(Account.LOCATION_ACCESS_CONSENT_KEY))
            {
                App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = false;
                App.ApplicationSettings.Save();
            }


            // SIgn in
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                for (int i = 0; i < App.IStorageManagers.Length; i++)
                {
                    // If main platform is signed in, process it.
                    // Otherwise, ignore and go to explorer page.
                    if (App.IStorageManagers[i].IsSignIn())
                        App.TaskHelper.AddSignInTask(App.IStorageManagers[i].GetStorageName(), App.IStorageManagers[i].SignIn());
                }
            }
            NavigationService.Navigate(new Uri(EventHelper.EXPLORER_PAGE, UriKind.Relative));
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}