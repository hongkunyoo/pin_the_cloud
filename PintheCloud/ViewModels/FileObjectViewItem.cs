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
        private Uri fileThumnail;
        public Uri FileThumnail
        {
            get
            {
                return fileThumnail;
            }
            set
            {
                if (fileThumnail != value)
                {
                    fileThumnail = value;
                    NotifyPropertyChanged("FileThumnail");
                }
            }
        }

        private string fileName;
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                if (fileName != value)
                {
                    fileName = value;
                    NotifyPropertyChanged("FileName");
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
