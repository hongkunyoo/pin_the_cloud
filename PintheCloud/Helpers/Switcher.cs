using PintheCloud.Managers;
using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Helpers
{
    public static class Switcher
    {
        private static IStorageManager CurrentManager = null;
        private static Dictionary<string, IStorageManager> map = new Dictionary<string, IStorageManager>();
        private static string MAIN_PLATFORM_TYPE_KEY = "MAIN_PLATFORM_TYPE_KEY";
        private static string DEFAULT_STORAGE = AppResources.OneDrive;

        public static void AddStorage(string key, IStorageManager value)
        {
            if (!map.ContainsKey(key))
                map.Add(key, value);
        }
        public static void SetStorageTo(string key)
        {
            if (map.ContainsKey(key))
                CurrentManager = map[key];
        }
        public static IStorageManager GetCurrentStorage()
        {
            return CurrentManager;
        }

        public static void SetMainPlatform(string key)
        {
            App.ApplicationSettings[MAIN_PLATFORM_TYPE_KEY] = key;
            App.ApplicationSettings.Save();
        }
        public static void SetStorageToMainPlatform()
        {
            if(App.ApplicationSettings.Contains(MAIN_PLATFORM_TYPE_KEY))
                SetStorageTo((string)App.ApplicationSettings[MAIN_PLATFORM_TYPE_KEY]);
            else
                SetStorageTo(DEFAULT_STORAGE);
        }
    }
}
