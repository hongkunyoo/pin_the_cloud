using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class StorageAccountType
    {
        // Account Type
        private const string NORMAL_ACCOUNT_TYPE = "Normal";
        private const string NORMAL_ACCOUNT_TYPE_ID = "1";
        private const double DEFAULT_MAX_SIZE = 3072.0;


        public string AccountTypeName { get; set; }

        public double MaxSize { get; set; }

        public string Id { get; set; }


        public StorageAccountType()
        {
            AccountTypeName = NORMAL_ACCOUNT_TYPE;
            Id = NORMAL_ACCOUNT_TYPE_ID;
            MaxSize = DEFAULT_MAX_SIZE;
        }


        public static MSAccountType ConvertToMSAccountType(StorageAccountType sat)
        {
            MSAccountType msat = new MSAccountType();
            msat.account_type_id = sat.Id;
            msat.account_type_name = sat.AccountTypeName;
            msat.account_type_max_size = sat.MaxSize;
            return msat;
        }


        private static StorageAccountType ConvertToStorageAccountType(MSAccountType msat)
        {
            StorageAccountType sat = new StorageAccountType();
            sat.Id = msat.account_type_id;
            sat.AccountTypeName = msat.account_type_name;
            sat.MaxSize = msat.account_type_max_size;
            return sat;
        }
    }


    /// <summary>
    /// Mobile Service AccountType
    /// </summary>
    public class MSAccountType
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "account_type_name")]
        public string account_type_name { get; set; }

        [JsonProperty(PropertyName = "account_type_max_size")]
        public double account_type_max_size { get; set; }
        [JsonProperty(PropertyName = "account_type_id")]
        public string account_type_id { get; set; }
    }
}
