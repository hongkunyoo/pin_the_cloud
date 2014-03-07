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
    public class SpotViewItem : INotifyPropertyChanged
    {
        public string SpotName { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public double SpotDistance { get; set; }
        public string SpotId { get; set; }
        public string SpotNameInitialImage { get; set; }
        public string IsPrivateImage { get; set; }
        public string SpotPassword { get; set; }

        private bool deleteImagePress;
        public bool DeleteImagePress
        {
            get
            {
                return deleteImagePress;
            }
            set
            {
                if (deleteImagePress != value)
                {
                    deleteImagePress = value;
                    NotifyPropertyChanged("DeleteImagePress");
                }
            }
        }
        
        private string deleteImage;
        public string DeleteImage
        {
            get
            {
                return deleteImage;
            }
            set
            {
                if (deleteImage != value)
                {
                    deleteImage = value;
                    NotifyPropertyChanged("DeleteImage");
                }
            }
        }


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
