using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.ViewModels;
using System.Windows.Media.Imaging;

namespace PintheCloud.Pages
{
    public partial class FileListPage : PhoneApplicationPage
    {
        private string SpaceId;
        private string SpaceName;
        private string AccountName;
        private string SpaceLike;
        private Uri SpaceLikeButtonImage;

        public FileListPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Get parameters
            this.SpaceId = NavigationContext.QueryString["spaceId"];
            this.SpaceName = NavigationContext.QueryString["spaceName"];
            this.AccountName = NavigationContext.QueryString["accountName"];
            this.SpaceLike = NavigationContext.QueryString["spaceLike"];
            string spaceLikeButtonImage = NavigationContext.QueryString["spaceLikeButtonImage"];
            if(spaceLikeButtonImage.Equals(SpaceViewModel.LIKE_NOT_PRESS_IMAGE_PATH))  // Not Like
                this.SpaceLikeButtonImage = new Uri(FileObjectViewModel.LIKE_NOT_PRESS_IMAGE_PATH, UriKind.Relative);
            else  // Like
                this.SpaceLikeButtonImage = new Uri(FileObjectViewModel.LIKE_PRESS_IMAGE_PATH, UriKind.Relative);


            // Set Instances to UI
            uiSpaceName.Text = this.SpaceName;
            uiAccountName.Text = this.AccountName;
            uiSpaceLike.Text = this.SpaceLike;
            uiSpaceLikeButtonImage.Source = new BitmapImage(this.SpaceLikeButtonImage);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
    }
}