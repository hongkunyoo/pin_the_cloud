using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Helpers
{
    public class LockHelper
    {
        private static string LOCK_KEY = "LOCK_KEY";
        public delegate void Block();
        public static void Mutex(Block b)
        {
            if(!App.ApplicationSettings.Contains(LOCK_KEY)){
                App.ApplicationSettings[LOCK_KEY] = true;
                b();
                App.ApplicationSettings.Remove(LOCK_KEY);
            }
        }
    }
}
