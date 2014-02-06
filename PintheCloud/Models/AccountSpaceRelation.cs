using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class AccountSpaceRelation
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "account_id")]
        public string account_id { get; set; }

        [JsonProperty(PropertyName = "space_id")]
        public string space_id { get; set; }


        public AccountSpaceRelation(string account_id, string space_id)
        {
            this.account_id = account_id;
            this.space_id = space_id;
        }
    }
}
