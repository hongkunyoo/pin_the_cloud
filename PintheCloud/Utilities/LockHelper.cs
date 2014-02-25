using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public class LockHelper
    {
        private static string LOCK_KEY = "LOCK_KEY";
        public static string LAUNCHER_LOCK = "LAUNCHER_LOCK";
        public delegate void Block();
        public static void Mutex(Block b)
        {
            if(!App.ApplicationSettings.Contains(LOCK_KEY)){
                App.ApplicationSettings[LOCK_KEY] = true;
                b();
                App.ApplicationSettings.Remove(LOCK_KEY);
            }
        }

        //public static void LauncherMutex(Block b)
        //{
        //    if (!App.ApplicationSettings.Contains(LAUNCHER_LOCK))
        //    {
        //        App.ApplicationSettings[LAUNCHER_LOCK] = true;
        //        b();
        //    }
        //}

    }
}
