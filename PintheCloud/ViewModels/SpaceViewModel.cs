using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using PintheCloud.Models;
using PintheCloud.Resources;
using PintheCloud.Utilities;
using PintheCloud.Workers;
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
    public class SpaceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<SpaceViewItem> Items { get; set; }

        // Mutex
        public bool IsDataLoaded { get; set; }


        public SpaceViewModel()
        {
            this.Items = new ObservableCollection<SpaceViewItem>();
        }


        public void SetItems(JArray spaces)
        {
            // If items have something, clear.
            this.Items.Clear();

            // Convert jarray spaces to space view items and set to view model
            foreach (JObject jSpace in spaces)
            {
                // Set new space view item
                SpaceViewItem spaceViewItem = new SpaceViewItem();
                spaceViewItem.SpaceName = (string)jSpace["space_name"];
                spaceViewItem.AccountId = (string)jSpace["account_id"];
                spaceViewItem.AccountName = (string)jSpace["account_name"];
                spaceViewItem.SpaceId = (string)jSpace["id"];
                spaceViewItem.SpaceDistance = (double)jSpace["space_distance"];
                this.Items.Add(spaceViewItem);
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
