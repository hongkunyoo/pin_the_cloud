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


        public FileObjectViewItem()
        { 
        }


        // Deep copy
        public FileObjectViewItem(FileObjectViewItem fileObjectViewItem)
        {
            this.Id = fileObjectViewItem.Id;
            this.Name = fileObjectViewItem.Name;
            this.Size = fileObjectViewItem.Size;
            this.SizeUnit = fileObjectViewItem.SizeUnit;
            this.ThumnailType = fileObjectViewItem.ThumnailType;
            this.SelectFileImage = fileObjectViewItem.SelectFileImage;
            this.DownloadUrl = fileObjectViewItem.DownloadUrl;
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
