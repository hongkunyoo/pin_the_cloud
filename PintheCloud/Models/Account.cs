using Newtonsoft.Json;
using PintheCloud.Resources;
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
        public enum StorageAccountType { SKY_DRIVE, DROPBOX, GOOGLE_DRIVE }
        //public static string[] PLATFORM_NAMES = { AppResources.SkyDrive, AppResources.Dropbox, AppResources.GoogleDrive };

        public const string ACCOUNT_MAIN_PLATFORM_TYPE_KEY = "ACCOUNT_MAIN_PLATFORM_TYPE_KEY";
        public const string LOCATION_ACCESS_CONSENT_KEY = "LOCATION_ACCESS_CONSENT_KEY";
        public const string ACCOUNT_NICK_NAME_KEY = "ACCOUNT_NICK_NAME_KEY";


        public string id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id")]
        public string account_platform_id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id_type")]
        public string account_platform_id_type { get; set; }

        [JsonProperty(PropertyName = "account_name")]
        public string account_name { get; set; }

        [JsonProperty(PropertyName = "account_used_size")]
        public double account_used_size { get; set; }

        [JsonProperty(PropertyName = "account_business_type")]
        public string account_business_type { get; set; }


        public Account(string account_platform_id, Account.StorageAccountType account_platform_id_type, string account_name, 
            double account_used_size, string account_business_type)
        {
            this.account_platform_id = account_platform_id;
            this.account_platform_id_type = account_platform_id_type.ToString();
            this.account_name = account_name;
            this.account_used_size = account_used_size;
            this.account_business_type = account_business_type;
        }

        public Account(string account_platform_id, Account.StorageAccountType account_platform_id_type, string account_name,
            double account_used_size)
        {
            this.account_platform_id = account_platform_id;
            this.account_platform_id_type = account_platform_id_type.ToString();
            this.account_name = account_name;
            this.account_used_size = account_used_size;
            this.account_business_type = AccountType.NORMAL_ACCOUNT_TYPE;
        }
    }
}
