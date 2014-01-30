using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class Space : INotifyPropertyChanged
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "space_name")]
        public string space_name { get; set; }
        public string SpaceName
        {
            get
            {
                return space_name;
            }
            set
            {
                if (space_name != value)
                {
                    space_name = value;
                    NotifyPropertyChanged("SpaceName");
                }
            }
        }

        [JsonProperty(PropertyName = "space_latitude")]
        public double space_latitude { get; set; }
        public double SpaceLatitude
        {
            get
            {
                return space_latitude;
            }
            set
            {
                if (space_latitude != value)
                {
                    space_latitude = value;
                    NotifyPropertyChanged("SpaceLatitude");
                }
            }
        }

        [JsonProperty(PropertyName = "space_longtitude")]
        public double space_longtitude { get; set; }
        public double SpaceLongtitude
        {
            get
            {
                return space_longtitude;
            }
            set
            {
                if (space_longtitude != value)
                {
                    space_longtitude = value;
                    NotifyPropertyChanged("SpaceLongtitude");
                }
            }
        }

        [JsonProperty(PropertyName = "account_id")]
        public string account_id { get; set; }
        public string AccountId
        {
            get
            {
                return account_id;
            }
            set
            {
                if (account_id != value)
                {
                    account_id = value;
                    NotifyPropertyChanged("AccountId");
                }
            }
        }

        [JsonProperty(PropertyName = "space_like_number")]
        public int space_like_number { get; set; }
        public int SpaceLikeNumber
        {
            get
            {
                return space_like_number;
            }
            set
            {
                if (space_like_number != value)
                {
                    space_like_number = value;
                    NotifyPropertyChanged("SpaceLikeNumber");
                }
            }
        }


        public Space(string space_name, double space_latitude, double space_longtitude, string account_id, int space_like_number)
        {
            this.space_name = space_name;
            this.space_latitude = space_latitude;
            this.space_longtitude = space_longtitude;
            this.account_id = account_id;
            this.space_like_number = space_like_number;
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
