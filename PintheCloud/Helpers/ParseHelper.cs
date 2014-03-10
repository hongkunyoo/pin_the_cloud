using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Helpers
{
    /// <summary>
    /// Helps parsing strings
    /// </summary>
    public class ParseHelper
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
        public static string[] ParsePathAndName(string path, ParseHelper.Mode mode, out string name)
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


        /*
        public static string ParseParentId(string fullPath)
        {
            fullPath = ParseHelper.TrimSlash(fullPath);
            return System.Text.RegularExpressions.Regex.Match(fullPath, ".*\/").Value;
        }
        */


        /// <summary>
        /// Parse the name from the full path
        /// </summary>
        /// <param name="fullPath">The full path to parse</param>
        /// <returns>name in the path</returns>
        public static string ParseName(string fullPath)
        {
            fullPath = ParseHelper.TrimSlash(fullPath);
            return System.Text.RegularExpressions.Regex.Match(fullPath, "[^/]*$").Value;
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
