using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    public class SpaceViewItem : INotifyPropertyChanged
    {
        private string spaceName;
        public string SpaceName
        {
            get
            {
                return spaceName;
            }
            set
            {
                if (spaceName != value)
                {
                    spaceName = value;
                    NotifyPropertyChanged("SpaceName");
                }
            }
        }

        private string spaceLikeDescription;
        public string SpaceLikeDescription
        {
            get
            {
                return spaceLikeDescription;
            }
            set
            {
                if (spaceLikeDescription != value)
                {
                    spaceLikeDescription = value;
                    NotifyPropertyChanged("SpaceLikeDescription");
                }
            }
        }

        private string spaceDescription;
        public string SpaceDescription
        {
            get
            {
                return spaceDescription;
            }
            set
            {
                if (spaceDescription != value)
                {
                    spaceDescription = value;
                    NotifyPropertyChanged("SpaceDescription");
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
