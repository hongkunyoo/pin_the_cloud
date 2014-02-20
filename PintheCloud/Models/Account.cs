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
        public enum StorageAccountType { SKY_DRIVE, DROP_BOX, GOOGLE_DRIVE }
        public StorageAccountType AccountType;
        public object RawAccount;


        // Application Account Setting Key
        public static string[] ACCOUNT_IS_SIGN_IN_KEYS = { "ACCOUNT_SKY_DRIVE_IS_SIGN_IN_KEY", "ACCOUNT_DROPBOX_IS_SIGN_IN_KEY" };
        public static string[] ACCOUNT_ID_KEYS = { "ACCOUNT_SKY_DRIVE_ID_KEY", "ACCOUNT_DROPBOX_ID_KEY" };

        public const string ACCOUNT_MAIN_PLATFORM_TYPE_KEY = "ACCOUNT_MAIN_PLATFORM_TYPE_KEY";

        public const string ACCOUNT_SKY_DRIVE_USED_SIZE_KEY = "ACCOUNT_SKY_DRIVE_USED_SIZE_KEY";
        public const string ACCOUNT_DROPBOX_USED_SIZE_KEY = "ACCOUNT_DROPBOX_USED_SIZE_KEY";

        public const string ACCOUNT_SKY_DRIVE_TYPE_NAME_KEY = "ACCOUNT_SKY_DRIVE_TYPE_NAME_KEY";
        public const string ACCOUNT_DROPBOX_TYPE_NAME_KEY = "ACCOUNT_DROPBOX_TYPE_NAME_KEY";

        public const string LOCATION_ACCESS_CONSENT_KEY = "LOCATION_ACCESS_CONSENT_KEY";
        public const string ACCOUNT_NICK_NAME_KEY = "ACCOUNT_NICK_NAME_KEY";


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

        public Account()
        {
        }
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
