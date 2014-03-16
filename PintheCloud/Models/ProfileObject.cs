using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class ProfileObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        //public string SpotId { get; ;set; }

        public ProfileObject()
        {

        }
        public ProfileObject(string Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
        }

        public static ProfileObject ConvertToProfileObject(MSProfileObject mspo)
        {
            ProfileObject po = new ProfileObject();
            po.Id = mspo.id;
            po.Name = mspo.name;
            po.PhoneNumber = mspo.phone_number;
            po.Email = mspo.email;
            return po;
        }
        public MSProfileObject ToMSObject()
        {
            return new MSProfileObject(this.Name, this.PhoneNumber, this.Email, null);
        }
    }

    public class MSProfileObject
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string phone_number { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "spot_id")]
        public string spot_id { get; set; }

        public MSProfileObject(string name, string phone_number, string email, string spot_id)
        {
            this.name = name;
            this.phone_number = phone_number;
            this.email = email;
            this.spot_id = spot_id;
        }
    }
}
