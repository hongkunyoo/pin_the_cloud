using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

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

        private double spaceDistance;
        public double SpaceDistance
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

        private int spaceLikeNumber;
        public int SpaceLikeNumber
        {
            get
            {
                return spaceLikeNumber;
            }
            set
            {
                if (spaceLikeNumber != value)
                {
                    spaceLikeNumber = value;
                    NotifyPropertyChanged("SpaceLikeNumber");
                }
            }
        }

        private string spaceLikeNumberColor;
        public string SpaceLikeNumberColor
        {
            get
            {
                return spaceLikeNumberColor;
            }
            set
            {
                if (spaceLikeNumberColor != value)
                {
                    spaceLikeNumberColor = value;
                    NotifyPropertyChanged("SpaceLikeNumberColor");
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
