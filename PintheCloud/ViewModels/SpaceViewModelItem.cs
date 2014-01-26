using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    class SpaceViewModelItem : INotifyPropertyChanged
    {
        private string spaceName;
        /// <summary>
        /// 샘플 ViewModel 속성: 이 속성은 바인딩을 사용하여 해당 값을 표시하기 위해 뷰에서 사용됩니다.
        /// </summary>
        /// <returns></returns>
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

        private string spaceLikeNumber;
        /// <summary>
        /// 샘플 ViewModel 속성: 이 속성은 바인딩을 사용하여 해당 값을 표시하기 위해 뷰에서 사용됩니다.
        /// </summary>
        /// <returns></returns>
        public string SpaceLikeNumber
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

        private string spaceDescription;
        /// <summary>
        /// 샘플 ViewModel 속성: 이 속성은 바인딩을 사용하여 해당 값을 표시하기 위해 뷰에서 사용됩니다.
        /// </summary>
        /// <returns></returns>
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
