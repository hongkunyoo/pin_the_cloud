using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class NoteObject
    {
        public string Id { get; set; }

        public ProfileObject Owner { get; set; }
        public String Title { get; set; }
        public String Body { get; set; }
        public string CreateAt { get; set; }
        public string SpotId { get; set; }

        public static MSNoteObject ConvertToMSNoteObject(NoteObject no)
        {
            return new MSNoteObject(no.Owner.Id, no.Owner.Name, no.Title, no.Body, no.SpotId, no.CreateAt);
        }
        public static NoteObject ConvertToNoteObject(MSNoteObject msno)
        {
            NoteObject no = new NoteObject();
            no.Id = msno.id;
            no.Owner = new ProfileObject(msno.owner_account_id, msno.owner_account_name);
            no.Title = msno.title;
            no.Body = msno.body;
            no.CreateAt = msno.create_at;
            no.SpotId = msno.spot_id;
            return no;
        }

    }

    public class MSNoteObject
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "owner_account_id")]
        public string owner_account_id { get; set; }

        [JsonProperty(PropertyName = "owner_account_name")]
        public string owner_account_name { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string title { get; set; }

        [JsonProperty(PropertyName = "body")]
        public string body { get; set; }

        [JsonProperty(PropertyName = "create_at")]
        public string create_at { get; set; }

        [JsonProperty(PropertyName = "spot_id")]
        public string spot_id { get; set; }

        public MSNoteObject(string owner_account_id, string owner_account_name, string title, string body, string spot_id, string create_at = null)
        {
            this.owner_account_id = owner_account_id;
            this.owner_account_name = owner_account_name;
            this.title = title;
            this.body = body;
            this.spot_id = spot_id;
            this.create_at = create_at;
            
            if (create_at == null || string.Empty.Equals(create_at)) create_at = DateTime.Now.ToString();
        }
    }
}
