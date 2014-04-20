using PintheCloud.Managers;
using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Helpers
{
    public static class StorageHelper
    {
        private static List<IStorageManager> List = new List<IStorageManager>();
        private static Dictionary<string, IStorageManager> Map = new Dictionary<string, IStorageManager>();
        private static string DEFAULT_STORAGE = AppResources.OneDrive;


        public static void AddStorageManager(string key, IStorageManager value)
        {
            if (!Map.ContainsKey(key))
            {
                Map.Add(key, value);
                List.Add(value);
            }
        }


        public static IStorageManager GetStorageManager(string key)
        {
            if (Map.ContainsKey(key))
                return Map[key];
            else
                return Map[DEFAULT_STORAGE];
        }


        public static List<IStorageManager> GetStorageList()
        {
            return List;
        }


        public static IEnumerator<IStorageManager> GetStorageEnumerator()
        {
            return List.GetEnumerator();
        }
    }
}
