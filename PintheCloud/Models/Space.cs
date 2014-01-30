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
        public string space_name;
        public string Space_name
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
                    NotifyPropertyChanged("Space_name");
                }
            }
        }

        [JsonProperty(PropertyName = "space_latitude")]
        public double space_latitude;
        public double Space_latitude
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
                    NotifyPropertyChanged("Space_latitude");
                }
            }
        }

        [JsonProperty(PropertyName = "space_longtitude")]
        public double space_longtitude;
        public double Space_longtitude
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
                    NotifyPropertyChanged("Space_longtitude");
                }
            }
        }

        [JsonProperty(PropertyName = "account_id")]
        public string account_id;
        public string Account_id
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
                    NotifyPropertyChanged("Account_id");
                }
            }
        }

        [JsonProperty(PropertyName = "space_like_number")]
        public int space_like_number;
        public int Space_like_number
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
                    NotifyPropertyChanged("Space_like_number");
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
