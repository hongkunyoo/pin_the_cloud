using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class Account
    {
        // Application Account Setting Key
        public static string ACCOUNT_SKY_DRIVE_IS_SIGN_IN = "ACCOUNT_SKY_DRIVE_IS_SIGN_IN";
        public static string ACCOUNT_DROPBOX_IS_SIGN_IN = "ACCOUNT_DROPBOX_IS_SIGN_IN";

        public static string ACCOUNT_SKY_DRIVE_ID = "ACCOUNT_SKY_DRIVE_ID";
        public static string ACCOUNT_DROPBOX_ID = "ACCOUNT_DROPBOX_ID";

        public static string ACCOUNT_MAIN_PLATFORM_TYPE = "ACCOUNT_MAIN_PLATFORM_TYPE";

        public static string ACCOUNT_SKY_DRIVE_USED_SIZE = "ACCOUNT_SKY_DRIVE_USED_SIZE";
        public static string ACCOUNT_DROPBOX_USED_SIZE = "ACCOUNT_DROPBOX_USED_SIZE";

        public static string ACCOUNT_SKY_DRIVE_TYPE_NAME = "ACCOUNT_SKY_DRIVE_TYPE_NAME";
        public static string ACCOUNT_DROPBOX_TYPE_NAME = "ACCOUNT_DROPBOX_TYPE_NAME";

        public static string LOCATION_ACCESS_CONSENT = "LOCATION_ACCESS_CONSENT";
        public static string ACCOUNT_NICK_NAME = "ACCOUNT_NICK_NAME";


        public string id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id")]
        public string account_platform_id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id_type")]
        public string account_platform_id_type { get; set; }

        [JsonProperty(PropertyName = "account_name")]
        public string account_name { get; set; }

        [JsonProperty(PropertyName = "account_first_name")]
        public string account_first_name { get; set; }

        [JsonProperty(PropertyName = "account_last_name")]
        public string account_last_name { get; set; }

        [JsonProperty(PropertyName = "account_locale")]
        public string account_locale { get; set; }

        [JsonProperty(PropertyName = "account_token")]
        public string account_token { get; set; }

        [JsonProperty(PropertyName = "account_used_size")]
        public double account_used_size { get; set; }

        [JsonProperty(PropertyName = "account_type_name")]
        public string account_type_name { get; set; }


        public Account(string account_platform_id, string account_platform_id_type, string account_name, 
            string account_first_name, string account_last_name, string account_locale,
            string account_token, double account_used_size, string account_type_name)
        {
            this.account_platform_id = account_platform_id;
            this.account_platform_id_type = account_platform_id_type;
            this.account_name = account_name;
            this.account_first_name = account_first_name;
            this.account_last_name = account_last_name;
            this.account_locale = account_locale;
            this.account_token = account_token;
            this.account_used_size = account_used_size;
            this.account_type_name = account_type_name;
        }
    }
}
