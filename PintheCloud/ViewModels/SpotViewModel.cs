using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.Resources;
using PintheCloud.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace PintheCloud.ViewModels
{
    public class SpotViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SpotViewItem> Items { get; set; }

        // Mutex
        public bool IsDataLoaded { get; set; }


        public SpotViewModel()
        {
            this.Items = new ObservableCollection<SpotViewItem>();
        }


        public void SetItems(JArray spots)
        {
            // If items have something, clear.
            this.Items.Clear();

            // Convert jarray spots to spot view items and set to view model
            foreach (JObject jSpot in spots)
            {
                // Set new spot view item
                SpotViewItem spotViewItem = new SpotViewItem();
                spotViewItem.SpotName = (string)jSpot["spot_name"];
                spotViewItem.AccountId = (string)jSpot["account_id"];
                spotViewItem.AccountName = (string)jSpot["account_name"];
                spotViewItem.SpotId = (string)jSpot["id"];
                spotViewItem.SpotDistance = (double)jSpot["spot_distance"];
                spotViewItem.DeleteImage = FileObjectViewModel.DELETE_IMAGE_URI;
                spotViewItem.DeleteImagePress = true;
                spotViewItem.SpotNameInitialImage = spotViewItem.SpotName.Substring(0, 1);
                spotViewItem.SpotPassword = (string)jSpot["spot_password"];

                if ((bool)jSpot["is_private"])
                    spotViewItem.IsPrivateImage = FileObjectViewModel.IS_PRIVATE_IMAGE_URI;
                else
                    spotViewItem.IsPrivateImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;

                this.Items.Add(spotViewItem);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
