using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public static class MyDebug
    {
        public static bool IsEnable()
        {
            return App.USER.Equals("hongkun");
        }

        public static bool Enable = false;
        public static void WriteLine(object obj)
        {
            if (IsEnable()) System.Diagnostics.Debug.WriteLine(obj);
        }
        public static void WriteLine(string message)
        {
            if (IsEnable()) System.Diagnostics.Debug.WriteLine(message);
        }
        public static void WriteLine(string format, object[] args)
        {
            if (IsEnable()) System.Diagnostics.Debug.WriteLine(format, args);
        }
    }
}
