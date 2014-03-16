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

        public static ProfileObject ConvertToProfileObject(PtcAccount ptca)
        {
            ProfileObject po = new ProfileObject();
            po.Id = ptca.Email;
            po.Name = ptca.Name;
            po.PhoneNumber = ptca.PhoneNumber;
            po.Email = ptca.Email;
            return po;
        }
    }
}
