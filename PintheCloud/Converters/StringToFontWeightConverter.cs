using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PintheCloud.Converters
{
    public class StringToFontWeightConverter : IValueConverter
    {
        public const string BOLD = "bold";
        public const string LIGHT = "light";


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetFontWeightFromString((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public static FontWeight GetFontWeightFromString(string fontWeightString)
        {
            if (fontWeightString.Equals(LIGHT))
                return FontWeights.Light;
            else if (fontWeightString.Equals(BOLD))
                return FontWeights.Bold;
            else
                return FontWeights.Normal;
        }
    }
}
