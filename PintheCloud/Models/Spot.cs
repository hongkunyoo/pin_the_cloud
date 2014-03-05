using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class Spot
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "spot_name")]
        public string spot_name { get; set; }

        [JsonProperty(PropertyName = "spot_latitude")]
        public double spot_latitude { get; set; }

        [JsonProperty(PropertyName = "spot_longtitude")]
        public double spot_longtitude { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string account_id { get; set; }

        [JsonProperty(PropertyName = "account_name")]
        public string account_name { get; set; }

        [JsonProperty(PropertyName = "spot_distance")]
        public double spot_distance { get; set; }

        [JsonProperty(PropertyName = "spot_password")]
        public string spot_password { get; set; }

        [JsonProperty(PropertyName = "is_private")]
        public bool is_private { get; set; }


        public Spot(string spot_name, double spot_latitude, double spot_longtitude, 
            string account_id, string account_name, double spot_distance, bool is_private, string spot_password)
        {
            this.spot_name = spot_name;
            this.spot_latitude = spot_latitude;
            this.spot_longtitude = spot_longtitude;
            this.account_id = account_id;
            this.account_name = account_name;
            this.spot_distance = spot_distance;
            this.is_private = is_private;
            this.spot_password = spot_password;
        }
    }
}
