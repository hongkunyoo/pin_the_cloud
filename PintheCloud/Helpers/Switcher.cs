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
        private static string MAIN_PLATFORM_TYPE_KEY = "MAIN_PLATFORM_TYPE_KEY";
        

        public static void SetStorageTo(string key)
        {
            CurrentManager = StorageHelper.GetStorageManager(key);
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


        public static IStorageManager GetMainStorage()
        {
            //IStorageManager retStorage = StorageHelper.GetStorageManager(MAIN_PLATFORM_TYPE_KEY);
            IStorageManager retStorage = StorageHelper.GetStorageManager((string)App.ApplicationSettings[MAIN_PLATFORM_TYPE_KEY]);
            //if (!retStorage.IsSignIn())
            //{
            //    List<IStorageManager> list = StorageHelper.GetStorageList();
            //    for (var i = 0; i < list.Count; i++)
            //        if (list[i].IsSignIn()) return list[i];
            //}
            return retStorage;
        }


        public static void SetStorageToMainPlatform()
        {
            if (App.ApplicationSettings.Contains(MAIN_PLATFORM_TYPE_KEY))
                SetStorageTo((string)App.ApplicationSettings[MAIN_PLATFORM_TYPE_KEY]);
            else
                SetStorageTo("DEFAULT");
        }


        public static int GetCurrentIndex()
        {
            return StorageHelper.GetStorageList().IndexOf(CurrentManager);
        }


        public static int GetStorageIndex(string key)
        {
            return StorageHelper.GetStorageList().IndexOf(StorageHelper.GetStorageManager(key));
        }
    }
}
