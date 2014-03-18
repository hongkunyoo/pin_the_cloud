using Newtonsoft.Json;
using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class StorageAccount
    {
        // Application Account Setting Key
        public enum StorageAccountType { ONE_DRIVE, DROPBOX, GOOGLE_DRIVE }
        public const string LOCATION_ACCESS_CONSENT_KEY = "LOCATION_ACCESS_CONSENT_KEY";
        public const string ACCOUNT_DEFAULT_SPOT_NAME_KEY = "ACCOUNT_DEFAULT_SPOT_NAME_KEY";

        public string Id { get; set; }
        public string StorageName { get; set; }

        public string UserName { get; set; }

        public double UsedSize { get; set; }

        public StorageAccount()
        {

        }
        public StorageAccount(string id, StorageAccount.StorageAccountType type, string userName, double usedSize)
        {
            this.Id = id;
            this.StorageName = type.ToString();
            this.UserName = userName;
            this.UsedSize = usedSize;
        }

        public static MSStorageAccount ConvertToMSStorageAccount(StorageAccount sa)
        {
            MSStorageAccount mssa = new MSStorageAccount(sa.Id, sa.StorageName, sa.UserName, sa.UsedSize);
            return mssa;
        }

        public static StorageAccount ConvertToStorageAccount(MSStorageAccount mssa)
        {
            StorageAccount sa = new StorageAccount();
            sa.Id = mssa.account_platform_id;
            sa.StorageName = mssa.account_platform_id_type;
            sa.UserName = mssa.account_name;
            sa.UsedSize = mssa.account_used_size;
            return sa;
        }
    }

    /// <summary>
    /// Mobile Service Storage Account
    /// This class will be stored in the mobile service table
    /// </summary>
    public class MSStorageAccount
    {
        public enum StorageAccountType { ONE_DRIVE, DROPBOX, GOOGLE_DRIVE }
        public const string ACCOUNT_MAIN_PLATFORM_TYPE_KEY = "ACCOUNT_MAIN_PLATFORM_TYPE_KEY";
        public const string LOCATION_ACCESS_CONSENT_KEY = "LOCATION_ACCESS_CONSENT_KEY";
        public const string ACCOUNT_DEFAULT_SPOT_NAME_KEY = "ACCOUNT_DEFAULT_SPOT_NAME_KEY";


        public string id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id")]
        public string account_platform_id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id_type")]
        public string account_platform_id_type { get; set; }

        [JsonProperty(PropertyName = "account_name")]
        public string account_name { get; set; }

        [JsonProperty(PropertyName = "account_used_size")]
        public double account_used_size { get; set; }
        [JsonProperty(PropertyName = "ptc_account_id")]
        public string ptc_account_id { get; set; }

        public MSStorageAccount(string account_platform_id, string account_platform_id_type, string account_name,
            double account_used_size)
        {
            this.account_platform_id = account_platform_id;
            this.account_platform_id_type = account_platform_id_type;
            this.account_name = account_name;
            this.account_used_size = account_used_size;
            this.ptc_account_id = "";
        }
    }
}
