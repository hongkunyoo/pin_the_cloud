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
        //public const string ACCOUNT_MAIN_PLATFORM_TYPE_KEY = "ACCOUNT_MAIN_PLATFORM_TYPE_KEY";
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
    }
}
