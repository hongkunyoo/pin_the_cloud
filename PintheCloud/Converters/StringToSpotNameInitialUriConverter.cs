using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PintheCloud.Converters
{
    public class StringToSpotNameInitialUriConverter : IValueConverter
    {
        // Instances
        private const string A_URI = "/Assets/pajeon/at_here/png/pin_label_001.png";
        private const string B_URI = "/Assets/pajeon/at_here/png/pin_label_002.png";
        private const string C_URI = "/Assets/pajeon/at_here/png/pin_label_003.png";
        private const string D_URI = "/Assets/pajeon/at_here/png/pin_label_004.png";
        private const string E_URI = "/Assets/pajeon/at_here/png/pin_label_005.png";
        private const string F_URI = "/Assets/pajeon/at_here/png/pin_label_006.png";
        private const string G_URI = "/Assets/pajeon/at_here/png/pin_label_007.png";
        private const string H_URI = "/Assets/pajeon/at_here/png/pin_label_008.png";
        private const string I_URI = "/Assets/pajeon/at_here/png/pin_label_009.png";
        private const string J_URI = "/Assets/pajeon/at_here/png/pin_label_010.png";
        private const string K_URI = "/Assets/pajeon/at_here/png/pin_label_011.png";
        private const string L_URI = "/Assets/pajeon/at_here/png/pin_label_012.png";
        private const string M_URI = "/Assets/pajeon/at_here/png/pin_label_013.png";
        private const string N_URI = "/Assets/pajeon/at_here/png/pin_label_014.png";
        private const string O_URI = "/Assets/pajeon/at_here/png/pin_label_015.png";
        private const string P_URI = "/Assets/pajeon/at_here/png/pin_label_016.png";
        private const string Q_URI = "/Assets/pajeon/at_here/png/pin_label_017.png";
        private const string R_URI = "/Assets/pajeon/at_here/png/pin_label_018.png";
        private const string S_URI = "/Assets/pajeon/at_here/png/pin_label_019.png";
        private const string T_URI = "/Assets/pajeon/at_here/png/pin_label_020.png";
        private const string U_URI = "/Assets/pajeon/at_here/png/pin_label_021.png";
        private const string V_URI = "/Assets/pajeon/at_here/png/pin_label_022.png";
        private const string W_URI = "/Assets/pajeon/at_here/png/pin_label_023.png";
        private const string X_URI = "/Assets/pajeon/at_here/png/pin_label_024.png";
        private const string Y_URI = "/Assets/pajeon/at_here/png/pin_label_025.png";
        private const string Z_URI = "/Assets/pajeon/at_here/png/pin_label_026.png";
        private const string SPECIAL_URI = "/Assets/pajeon/at_here/png/pin_label_027.png";



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string initial = ((string)value).ToLower();
            if (initial.Equals("a"))
                return A_URI;
            else if (initial.Equals("b"))
                return B_URI;
            else if (initial.Equals("c"))
                return C_URI;
            else if (initial.Equals("d"))
                return D_URI;
            else if (initial.Equals("e"))
                return E_URI;
            else if (initial.Equals("f"))
                return F_URI;
            else if (initial.Equals("g"))
                return G_URI;
            else if (initial.Equals("h"))
                return H_URI;
            else if (initial.Equals("i"))
                return I_URI;
            else if (initial.Equals("j"))
                return J_URI;
            else if (initial.Equals("k"))
                return K_URI;
            else if (initial.Equals("l"))
                return L_URI;
            else if (initial.Equals("m"))
                return M_URI;
            else if (initial.Equals("n"))
                return N_URI;
            else if (initial.Equals("o"))
                return O_URI;
            else if (initial.Equals("p"))
                return P_URI;
            else if (initial.Equals("q"))
                return Q_URI;
            else if (initial.Equals("r"))
                return R_URI;
            else if (initial.Equals("s"))
                return S_URI;
            else if (initial.Equals("t"))
                return T_URI;
            else if (initial.Equals("u"))
                return U_URI;
            else if (initial.Equals("v"))
                return V_URI;
            else if (initial.Equals("w"))
                return W_URI;
            else if (initial.Equals("x"))
                return X_URI;
            else if (initial.Equals("y"))
                return Y_URI;
            else if (initial.Equals("z"))
                return Z_URI;
            else
                return SPECIAL_URI;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
