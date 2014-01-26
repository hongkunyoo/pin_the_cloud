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
        public ObservableCollection<SpaceViewModelItem> Items { get; private set; }

        public SpaceViewModel()
        {
            this.Items = new ObservableCollection<SpaceViewModelItem>();
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// 몇 개의 ItemViewModel 개체를 만들어 Items 컬렉션에 추가합니다.
        /// </summary>
        public void LoadData()
        {
            this.IsDataLoaded = true;
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
