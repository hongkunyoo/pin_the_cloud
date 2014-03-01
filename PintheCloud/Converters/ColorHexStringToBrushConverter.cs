using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PintheCloud.Converters
{
    public class ColorHexStringToBrushConverter : IValueConverter
    {
        // Implement Convert
        public Dictionary<string, Brush> _brushCache = new Dictionary<string, Brush>();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var colorStr = ((string)value).ToLower();

            lock (_brushCache)
            {
                if (!_brushCache.ContainsKey(colorStr))
                    _brushCache.Add(colorStr, new SolidColorBrush(GetColorFromHexString(colorStr)));

                return _brushCache[colorStr];
            }
        }
        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }


        // Get color from hex
        public static Color GetColorFromHexString(string hexColorString)
        {
            hexColorString = "FF" + hexColorString;
            var a = System.Convert.ToByte(hexColorString.Substring(0, 2), 16);
            var r = System.Convert.ToByte(hexColorString.Substring(2, 2), 16);
            var g = System.Convert.ToByte(hexColorString.Substring(4, 2), 16);
            var b = System.Convert.ToByte(hexColorString.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }
    }
}
