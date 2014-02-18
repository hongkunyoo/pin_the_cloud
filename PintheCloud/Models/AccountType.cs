using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    class AccountType
    {
        // Account Type
        public const string NORMAL_ACCOUNT_TYPE = "Normal";


        public string id { get; set; }

        [JsonProperty(PropertyName = "account_type_name")]
        public string account_type_name { get; set; }

        [JsonProperty(PropertyName = "account_type_max_size")]
        public double account_type_max_size { get; set; }


        public AccountType(string account_type_name, double account_type_max_size)
        {
            this.account_type_name = account_type_name;
            this.account_type_max_size = account_type_max_size;
        }
    }
}
