using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public static class MySpaceManager
    {
        public static Task<Space> InsertSpace(Space space)
        {
            return new Task<Space>(GetSpace);
        }

        public static Space GetSpace()
        {
            Space s = new Space("space_name",0.01,10.02,"account_id",0);
            s.id = Guid.NewGuid().ToString();
            return s;
        }
    }
}
