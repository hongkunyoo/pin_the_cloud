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

        private string spaceId;
        public string SpaceId
        {
            get
            {
                return spaceId;
            }
            set
            {
                if (spaceId != value)
                {
                    spaceId = value;
                    NotifyPropertyChanged("SpaceId");
                }
            }
        }

        private string spaceDistance;
        public string SpaceDistance
        {
            get
            {
                return spaceDistance;
            }
            set
            {
                if (spaceDistance != value)
                {
                    spaceDistance = value;
                    NotifyPropertyChanged("SpaceDistance");
                }
            }
        }

        private string spaceLike;
        public string SpaceLike
        {
            get
            {
                return spaceLike;
            }
            set
            {
                if (spaceLike != value)
                {
                    spaceLike = value;
                    NotifyPropertyChanged("SpaceLike");
                }
            }
        }

        private Uri spaceLikeButtonImage;
        public Uri SpaceLikeButtonImage
        {
            get
            {
                return spaceLikeButtonImage;
            }
            set
            {
                if (spaceLikeButtonImage != value)
                {
                    spaceLikeButtonImage = value;
                    NotifyPropertyChanged("SpaceLikeButtonImage");
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
