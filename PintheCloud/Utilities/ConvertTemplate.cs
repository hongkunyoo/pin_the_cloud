using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public class ConvertTemplate
    {
        public delegate T ConvertFirstToSecond<K, T>(K item);
    }
}
