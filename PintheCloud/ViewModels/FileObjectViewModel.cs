using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    public class FileObjectViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FileObjectViewItem> Items { get; set; }

        // Mutex
        public bool IsDataLoading { get; set; }

        public FileObjectViewModel()
        {
            this.Items = new ObservableCollection<FileObjectViewItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
