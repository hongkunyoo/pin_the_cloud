using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public class EventHelper
    {
        public const string SPLASH_PAGE = "/Pages/SplashPage.xaml";
        public const string EXPLORER_PAGE = "/Pages/ExplorerPage.xaml";
        public const string SETTINGS_PAGE = "/Pages/SettingsPage.xaml";
        public const string FILE_LIST_PAGE = "/Pages/FileListPage.xaml";

        public const string POPUP_CLOSE = "POP_UP_CLOSE";

        public const int PICK_PIVOT = 0;
        public const int PIN_PIVOT = 1;
        public const int NO_PIVOT = 2;

        private static Dictionary<string, Context> Map = new Dictionary<string, Context>();

        private static Dictionary<string, Queue<Context.TriggerEvent>> eventMap = new Dictionary<string, Queue<Context.TriggerEvent>>();

        public static void AddEventHandler(string key, Context.TriggerEvent _event)
        {
            if (eventMap.ContainsKey(key))
            {
                eventMap[key].Enqueue(_event);
            }
            else
            {
                eventMap[key] = new Queue<Context.TriggerEvent>();
                eventMap[key].Enqueue(_event);
            }
        }
        public static void TriggerEvent(string key)
        {
            if (eventMap.ContainsKey(key))
            {
                foreach (Context.TriggerEvent _event in eventMap[key])
                {
                    _event();
                }
            }
        }
        public static Context GetContext(string currentPage)
        {
            if (Map.ContainsKey(currentPage))
                return Map[currentPage];
            Context con = new Context();
            Map.Add(currentPage, con);
            return con;
        }


        public static void FireEvent(string current, string previous, int pivot)
        {
            if (!Map.ContainsKey(current)) return;
            Dictionary<string, Dictionary<int, Context.TriggerEvent>> m = Map[current].GetContextMap();
            if (!m.ContainsKey(previous)) return;
            if (m[previous].ContainsKey(NO_PIVOT))
            {
                m[previous][NO_PIVOT]();
                return;
            }
            if (!m[previous].ContainsKey(pivot)) return;
            Debug.WriteLine("firing : {0} {1} {2}", current, previous, pivot == PIN_PIVOT ? "PIN" : "PICK");
            m[previous][pivot]();
        }
    }

    public class Context
    {
        private Dictionary<string, Dictionary<int, TriggerEvent>> contextMap = new Dictionary<string, Dictionary<int, TriggerEvent>>();
        public delegate void TriggerEvent();


        public void HandleEvent(string previous, int pivot, TriggerEvent _event)
        {
            if (!contextMap.ContainsKey(previous))
                contextMap[previous] = new Dictionary<int, TriggerEvent>();
            contextMap[previous][pivot] = _event;
        }


        public void HandleEvent(string previous, TriggerEvent _event)
        {
            if (!contextMap.ContainsKey(previous))
                contextMap[previous] = new Dictionary<int, TriggerEvent>();
            contextMap[previous][EventHelper.NO_PIVOT] = _event;
        }


        public Dictionary<string, Dictionary<int, TriggerEvent>> GetContextMap()
        {
            return this.contextMap;
        }
    }
}
