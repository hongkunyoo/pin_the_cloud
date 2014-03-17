using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class LogObject
    {
        public string Id {get; set;}
        public string Who { get; set; }
        public string When { get; set; }
        public string What { get; set; }
        public string Verb { get; set; }


        public static MSLogObject ConvertToMSLogObject(LogObject lo)
        {
            return new MSLogObject(lo.Who, lo.When, lo.What, lo.Verb);
        }

        public static LogObject ConvertToLogObject(MSLogObject mslo)
        {
            LogObject lo = new LogObject();
            lo.Id = mslo.id;
            lo.Who = mslo.who;
            lo.When = mslo.when;
            lo.What = mslo.what;
            lo.Verb = mslo.verb;
            return lo;
        }
    }

    public class MSLogObject
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "who")]
        public string who { get; set; }

        [JsonProperty(PropertyName = "when")]
        public string when { get; set; }

        [JsonProperty(PropertyName = "what")]
        public string what { get; set; }

        [JsonProperty(PropertyName = "verb")]
        public string verb { get; set; }

        [JsonProperty(PropertyName = "spot_id")]
        public string spot_id { get; set; }

        public MSLogObject(string who, string when, string what, string verb)
        {
            this.who = who;
            this.what = what;
            this.when = when;
            this.verb = verb;
        }
    }
}
