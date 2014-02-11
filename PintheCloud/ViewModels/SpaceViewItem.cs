using PintheCloud.Utilities;
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
        public static string LIKE_NOT_PRESS_IMAGE_PATH = "/Assets/pajeon/png/general_like.png";
        public static string LIKE_PRESS_IMAGE_PATH = "/Assets/pajeon/png/general_like_p.png";


        public string SpaceName { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public double SpaceDistance { get; set; }
        public string SpaceId { get; set; }
        
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

        private string spaceLikeButtonImage;
        public string SpaceLikeButtonImage
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


        public void SetLikeButtonImage(bool isLike, int like)
        {
            if (isLike)
            {
                this.SpaceLikeNumber += like;
                this.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_COLOR;
                this.SpaceLikeButtonImage = LIKE_PRESS_IMAGE_PATH;
            }
            else
            {
                this.SpaceLikeNumber -= like;
                this.SpaceLikeNumberColor = ColorHexStringToBrushConverter.LIKE_NOT_COLOR;
                this.SpaceLikeButtonImage = LIKE_NOT_PRESS_IMAGE_PATH;
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
