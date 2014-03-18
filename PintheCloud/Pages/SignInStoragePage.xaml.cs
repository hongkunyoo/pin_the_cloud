using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Helpers;
using System.Collections.ObjectModel;
using PintheCloud.Managers;
using System.Diagnostics;
using PintheCloud.ViewModels;
using PintheCloud.Resources;
using System.Net.NetworkInformation;

namespace PintheCloud.Pages
{
    public partial class SignInStoragePage : PtcPage
    {
        private CloudModeViewModel CloudModeViewModel = new CloudModeViewModel(); 

        public SignInStoragePage()
        {
            InitializeComponent();


            ////////////////////////////////////////////////
            // TODO : SEUNGMIN
            // I don't know how to bind with Template Plz change this shit.
            ////////////////////////////////////////////////
            ObservableCollection<string> list = new ObservableCollection<string>();
            using (var itr = StorageHelper.GetStorageEnumerator())
            {
                while (itr.MoveNext())
                    list.Add(itr.Current.GetStorageName());
            }
            ui_storage_list.ItemsSource = list;
        }


        private void ui_storage_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                string SelectedStorageName = ui_storage_list.SelectedItem as string;
                IStorageManager Storage = StorageHelper.GetStorageManager(SelectedStorageName);
                Switcher.SetMainPlatform(SelectedStorageName);
                TaskHelper.AddSignInTask(Storage.GetStorageName(), Storage.SignIn());
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void ui_skip_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri(EventHelper.SPOT_LIST_PAGE, UriKind.Relative));
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);


            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (iStorageManager.IsSigningIn())
            {
                e.Cancel = true;

                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(AppResources.CloseAppMessage, AppResources.CloseAppCaption, MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                    e.Cancel = true;
            }
        }
    }
}