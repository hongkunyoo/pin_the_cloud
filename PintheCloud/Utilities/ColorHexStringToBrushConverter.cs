using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PintheCloud.Utilities
{
    public  class ColorHexStringToBrushConverter : IValueConverter
    {
        public const string LIKE_COLOR = "3ABDBE";
        public const string LIKE_NOT_COLOR = "A7B6BE";


        // Implement Convert
        public Dictionary<string, Brush> _brushCache = new Dictionary<string, Brush>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorStr = ((string)value).ToLower();

            lock (_brushCache)
            {
                if (!_brushCache.ContainsKey(colorStr))
                    _brushCache.Add(colorStr, new SolidColorBrush(GetColorFromHex(colorStr)));

                return _brushCache[colorStr];
            }
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }


        // Get color from hex
        private static Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static Color GetColorFromHex(string hexColorString)
        {
            if (hexColorString == null)
                throw new NullReferenceException("Hex string can't be null.");

            // Regex match the string
            hexColorString = "#" + hexColorString;
            var match = _hexColorMatchRegex.Match(hexColorString);

            if (!match.Success)
                throw new InvalidCastException(string.Format("Can't convert string \"{0}\" to argb or rgb color. Needs to be 6 (rgb) or 8 (argb) hex characters long. It can optionally start with a #.", hexColorString));

            // a value is optional
            byte a = 255, r = 0, b = 0, g = 0;
            if (match.Groups["a"].Success)
                a = System.Convert.ToByte(match.Groups["a"].Value, 16);
            // r,b,g values are not optional
            r = System.Convert.ToByte(match.Groups["r"].Value, 16);
            b = System.Convert.ToByte(match.Groups["b"].Value, 16);
            g = System.Convert.ToByte(match.Groups["g"].Value, 16);
            return Color.FromArgb(a, r, b, g);
        }
    }
}
