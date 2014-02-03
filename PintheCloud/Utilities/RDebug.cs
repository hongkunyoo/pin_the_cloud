using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public class RDebug : MyDebug
    {
        public override bool IsEnable()
        {
            return GlobalKeys.USER.Equals("rfrost");
        }
    }
}
