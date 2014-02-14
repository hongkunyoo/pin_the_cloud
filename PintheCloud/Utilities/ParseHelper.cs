using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    /// <summary>
    /// Helps parsing strings
    /// </summary>
    class ParseHelper
    {
        public enum Mode { DIRECTORY, FULL_PATH };
        /// <summary>
        /// Parses the path to split from each path and file name
        /// </summary>
        /// <param name="path">The path to parse</param>
        /// <param name="mode">
        /// DIRECTORY if it is only directory path
        /// FULL_PATH if it includes file name</param>
        /// <param name="name">Pass the variable to store the file name of given path</param>
        /// <returns>Path tokens in string array</returns>
        public static string[] Parse(string path, ParseHelper.Mode mode, out string name)
        {
            List<string> list = new List<string>();
            path = ParseHelper.TrimSlash(path);

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
        /// <summary>
        /// Split the given string to file name and extension.
        /// </summary>
        /// <param name="name">The name to parse</param>
        /// <returns>the name and the extension in string array</returns>
        public static string[] SplitName(string name)
        {
            return name.Split(new char[] { '.' }, 2);
        }
        /// <summary>
        /// Trim "/" character from the both side of the given string
        /// </summary>
        /// <param name="path">The string to trim</param>
        /// <returns>The trimed string</returns>
        public static string TrimSlash(string path)
        {
            if (path.StartsWith("/"))
                path = path.Substring(1, path.Length - 1);
            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);

            return path;
        }

        // Private Method
        private static string getToken(string path, out string slicedPath)
        {
            slicedPath = path;
            path = ParseHelper.TrimSlash(path);

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
