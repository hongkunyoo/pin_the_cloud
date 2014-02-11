using PintheCloud.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace PintheCloud.Utilities
{
    public static class Extensions
    {
        public static Task showSkyDriveAsync(this Popup popup){
            MyDebug.WriteLine("in showSkyDrive");
            popup.Child = new PopupUserControl();
            popup.IsOpen = true; 
            return null;
        }
    }
}
