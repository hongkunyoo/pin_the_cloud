using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    class ParseHelper
    {
        public enum Mode { DIRECTORY, FULL_PATH };
        public static string[] Parse(string path, ParseHelper.Mode mode, out string name)
        {
            List<string> list = new List<string>();
            if (path.StartsWith("/")) path = path.Substring(1, path.Length - 1);
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);

            while (path.Contains("/"))
            {
                list.Add(getToken(path, out path));
            }

            if (mode == Mode.DIRECTORY)
            {
                name = null;
                list.Add(path);
            }
            else
            {
                name = path;
            }
            return list.ToArray();
        }
        public static string[] SplitName(string name)
        {
            return name.Split(new char[] { '.' }, 2);
        }

        private static string getToken(string path, out string slicedPath)
        {
            slicedPath = path;
            if (path.StartsWith("/"))
                path = path.Substring(1, path.Length - 1);
            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);
            if (path.Contains("/"))
            {
                string[] strlist = path.Split(new Char[] { '/' }, 2);
                slicedPath = strlist[1];
                return strlist[0];
            }
            return null;
        }
    }
}
