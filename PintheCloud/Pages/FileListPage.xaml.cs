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
using PintheCloud.Utilities;
using System.Windows.Media;
using PintheCloud.Models;

namespace PintheCloud.Pages
{
    public partial class FileListPage : PtcPage
    {
        private string SpaceId;
        private string SpaceName;
        private string AccountId;
        private string AccountIdFontWeight;
        private string AccountName;
        private string SpaceLikeNumber;
        private string SpaceLikeNumberColor;


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
            this.AccountId = NavigationContext.QueryString["accountId"];
            this.AccountIdFontWeight = NavigationContext.QueryString["accountIdFontWeight"];
            this.AccountName = NavigationContext.QueryString["accountName"];
            this.SpaceLikeNumber = NavigationContext.QueryString["spaceLikeNumber"];
            this.SpaceLikeNumberColor = NavigationContext.QueryString["spaceLikeNumberColor"];

            // Set Binding Instances to UI
            uiSpaceName.Text = this.SpaceName;
            uiAccountName.Text = this.AccountName;
            uiSpaceLikeNumber.Text = this.SpaceLikeNumber;

            uiAccountName.FontWeight = StringToFontWeightConverter.GetFontWeightFromString(this.AccountIdFontWeight);
            Brush likeColor = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHex(this.SpaceLikeNumberColor));
            uiSpaceLikeNumber.Foreground = likeColor;
            uiSpaceLikeText.Foreground = likeColor;
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void uiFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Obejct
            FileObject fileObject = uiFileList.SelectedItem as FileObject;

            // Set selected item to null for next selection of list item. 
            uiFileList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObject != null) 
            {
                // Do Something
            }
        }
    }
}