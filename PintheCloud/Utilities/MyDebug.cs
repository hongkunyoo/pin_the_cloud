using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public abstract class MyDebug
    {
        public abstract bool IsEnable();

        public bool Enable = false;
        public void WriteLine(object obj)
        {
            if (IsEnable()) System.Diagnostics.Debug.WriteLine(obj);
        }
        public void WriteLine(string message)
        {
            if (IsEnable()) System.Diagnostics.Debug.WriteLine(message);
        }
        public void WriteLine(string format, object[] args)
        {
            if (IsEnable()) System.Diagnostics.Debug.WriteLine(format, args);
        }
    }
}
