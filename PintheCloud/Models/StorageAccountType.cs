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
        public const string NORMAL_ACCOUNT_TYPE = "Normal";

        public string AccountTypeName { get; set; }

        public double MaxSize { get; set; }

        public string Id { get; set; }

        public StorageAccountType()
        {
            AccountTypeName = NORMAL_ACCOUNT_TYPE;
            MaxSize = 3000;
            Id = "1";
        }
    }
}
