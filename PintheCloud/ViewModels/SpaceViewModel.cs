using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    public class SpaceViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// ItemViewModel 개체에 대한 컬렉션입니다.
        /// </summary>
        public ObservableCollection<Space> Items { get; private set; }

        public SpaceViewModel()
        {
            this.Items = new ObservableCollection<Space>();
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
