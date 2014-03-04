using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PintheCloud.Pages
{
    public partial class AddFilePage : PtcPage
    {
        // Instances
        private ApplicationBarMenuItem[] AppBarMenuItems = null;


        public AddFilePage()
        {
            InitializeComponent();

            // Set Cloud Setting selection.
            this.AppBarMenuItems = new ApplicationBarMenuItem[App.IStorageManagers.Length];
            for (int i = 0; i < this.AppBarMenuItems.Length; i++)
            {
                this.AppBarMenuItems[i] = new ApplicationBarMenuItem();
                this.AppBarMenuItems[i].Text = App.IStorageManagers[i].GetStorageName();
                this.AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }
        }


        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            //// Get index
            //ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;
            //int platformIndex = base.GetPlatformIndexFromString(appBarMenuItem.Text);


            //// If it is not in current cloud mode, change it.
            //if (this.CurrentPlatformIndex != platformIndex)
            //{
            //    // Kill previous job.

            //    IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
            //    uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
            //    uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
            //    this.CurrentPlatformIndex = platformIndex;

            //    // If it is already signin, load files.
            //    // Otherwise, show signin button.
            //    if (!iStorageManager.IsSignIn())
            //    {
            //        iStorageManager.GetFolderRootTree().Clear();
            //        iStorageManager.GetFoldersTree().Clear();
            //        this.SelectedFile.Clear();
            //        this.PinInfoAppBarButton.IsEnabled = false;

            //        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
            //        uiPinInfoSignInPanel.Visibility = Visibility.Visible;
            //    }
            //    else
            //    {
            //        // Show Loading message and save is login true for pivot moving action while sign in.
            //        uiPinInfoListGrid.Visibility = Visibility.Visible;
            //        uiPinInfoSignInPanel.Visibility = Visibility.Collapsed;

            //        if (NetworkInterface.GetIsNetworkAvailable())
            //        {
            //            if (iStorageManager.GetFolderRootTree().Count > 0)
            //                this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, false);
            //            else
            //                this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, true, null);
            //        }
            //        else
            //        {
            //            base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
            //        }
            //    }
            //}
        }
    }
}