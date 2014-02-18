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
        public string Name { get; set; }
        public double Size { get; set; }
        public string SizeUnit { get; set; }
        public string ThumnailType { get; set; }

        private string selectCheckImage;
        public string SelectCheckImage
        {
            get
            {
                return selectCheckImage;
            }
            set
            {
                if (selectCheckImage != value)
                {
                    selectCheckImage = value;
                    NotifyPropertyChanged("SelectCheckImage");
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
