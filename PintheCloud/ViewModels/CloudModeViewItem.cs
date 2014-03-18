using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    public class CloudModeViewItem : INotifyPropertyChanged
    {
        public string CloudName { get; set; }
        public string CloudModeImage { get; set; }
        public string CloudModeColor { get; set; }

        private string accountName;
        public string AccountName
        {
            get
            {
                return accountName;
            }
            set
            {
                if (accountName != value)
                {
                    accountName = value;
                    NotifyPropertyChanged("AccountName");
                }
            }
        }


        public CloudModeViewItem(string cloudName, string cloudModeImage, string cloudModeColor, string accountName)
        {
            this.CloudName = cloudName;
            this.CloudModeImage = cloudModeImage;
            this.CloudModeColor = cloudModeColor;
            this.AccountName = accountName;
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
