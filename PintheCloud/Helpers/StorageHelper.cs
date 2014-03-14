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
        private static List<IStorageManager> list = new List<IStorageManager>();
        private static Dictionary<string, IStorageManager> map = new Dictionary<string, IStorageManager>();
        private static string DEFAULT_STORAGE = AppResources.OneDrive;

        public static void AddStorageManager(string key, IStorageManager value)
        {
            if (!map.ContainsKey(key))
            {
                map.Add(key, value);
                list.Add(value);
            }
        }
        public static IStorageManager GetStorageManager(string key)
        {
            if (map.ContainsKey(key))
                return map[key];
            else
                return map[DEFAULT_STORAGE];
        }
        public static List<IStorageManager> GetStorageList()
        {
            return list;
        }
        public static IEnumerator<IStorageManager> GetStorageEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
