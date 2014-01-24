using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PintheCloud
{
    class GlobalManager
    {
        public static void showNoInternetMessageBox()
        {
            MessageBoxResult result = MessageBox.Show("Please make sure connection with Internet.",
                "Internet unavailable", MessageBoxButton.OK);
        }
    }
}
