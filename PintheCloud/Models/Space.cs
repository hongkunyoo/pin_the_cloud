using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    class Space
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "space_latitude")]
        public double space_latitude { get; set; }

        [JsonProperty(PropertyName = "space_longtitude")]
        public double space_longtitude { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string account_id { get; set; }

        [JsonProperty(PropertyName = "space_like_number")]
        public int account_like_number { get; set; }


        public Space(double space_latitude, double space_longtitude, string account_id, int space_like_number)
        {
            this.space_latitude = space_latitude;
            this.space_longtitude = space_longtitude;
            this.account_id = account_id;
            this.account_like_number = account_like_number;
        }
    }
}
