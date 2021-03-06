﻿using System;
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
using PintheCloud.Helpers;

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

            // Check if it is on the backstack from SplashPage and remove that.
            for (int i = 0; i <= NavigationService.BackStack.Count(); i++)
                NavigationService.RemoveBackEntry();

            // Check nick name at frist login.
            if (!App.ApplicationSettings.Contains(Switcher.MAIN_PLATFORM_TYPE_KEY))
            {
                App.ApplicationSettings[Switcher.MAIN_PLATFORM_TYPE_KEY] = AppResources.OneDrive;
                App.ApplicationSettings.Save();
            }

            // Check nick name at frist login.
            if (!App.ApplicationSettings.Contains(StorageAccount.ACCOUNT_DEFAULT_SPOT_NAME_KEY))
            {
                App.ApplicationSettings[StorageAccount.ACCOUNT_DEFAULT_SPOT_NAME_KEY] = AppResources.AtHere;
                App.ApplicationSettings.Save();
            }

            // Check location access consent at frist login.
            if (!App.ApplicationSettings.Contains(StorageAccount.LOCATION_ACCESS_CONSENT_KEY))
            {
                App.ApplicationSettings[StorageAccount.LOCATION_ACCESS_CONSENT_KEY] = false;
                App.ApplicationSettings.Save();
            }


            // Do Signin work of each cloud storage.
            if (App.AccountManager.IsSignIn())
            {
                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    TaskHelper.AddTask(App.AccountManager.GetPtcId(), App.AccountManager.SignIn());
                    using (var itr = StorageHelper.GetStorageEnumerator())
                    {
                        while (itr.MoveNext())
                            if (itr.Current.IsSignIn())
                                TaskHelper.AddSignInTask(itr.Current.GetStorageName(), itr.Current.SignIn());
                    }
                }
                NavigationService.Navigate(new Uri(EventHelper.SPOT_LIST_PAGE, UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri(EventHelper.PROFILE_PAGE, UriKind.Relative));
            }
        }
    }
}