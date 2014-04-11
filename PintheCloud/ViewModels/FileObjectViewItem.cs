using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    public class FileObjectViewItem : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string SizeUnit { get; set; }
        public string ThumnailType { get; set; }

        public string DownloadUrl { get; set; }

        private string selectFileImage;
        public string SelectFileImage
        {
            get
            {
                return selectFileImage;
            }
            set
            {
                if (selectFileImage != value)
                {
                    selectFileImage = value;
                    NotifyPropertyChanged("SelectFileImage");
                }
            }
        }

        private bool selectFileImagePress;
        public bool SelectFileImagePress
        {
            get
            {
                return selectFileImagePress;
            }
            set
            {
                if (selectFileImagePress != value)
                {
                    selectFileImagePress = value;
                    NotifyPropertyChanged("SelectFileImagePress");
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
