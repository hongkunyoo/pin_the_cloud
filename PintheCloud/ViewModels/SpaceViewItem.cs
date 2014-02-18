using PintheCloud.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PintheCloud.ViewModels
{
    public class SpaceViewItem : INotifyPropertyChanged
    {
        public const string LIKE_NOT_PRESS_IMAGE_URI = "/Assets/pajeon/png/general_like.png";
        public const string LIKE_PRESS_IMAGE_URI = "/Assets/pajeon/png/general_like_p.png";


        public string SpaceName { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public double SpaceDistance { get; set; }
        public string SpaceId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
