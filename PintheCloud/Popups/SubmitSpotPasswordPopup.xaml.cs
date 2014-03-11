using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using PintheCloud.Helpers;
using PintheCloud.Resources;
using PintheCloud.Pages;
using PintheCloud.Managers;

namespace PintheCloud.Popups
{
    public partial class SubmitSpotPasswordPopup : UserControl
    {
        private Popup Popup = null;
        private string SpotId = null;
        private string SpotPassword = null;

        public bool result = false;


        public SubmitSpotPasswordPopup(Popup popup, string spotId, string spotPassword, double width, double height, double topMargin)
        {
            InitializeComponent();
            this.Popup = popup;
            this.SpotId = spotId;
            this.SpotPassword = spotPassword;

            this.Width = width;
            this.Height = height;
            this.Margin = new Thickness(0, PtcPage.STATUS_BAR_HEIGHT + topMargin, 0, 0);

            EventHelper.AddEventHandler(EventHelper.POPUP_CLOSE, () =>
            {
                this.Popup.IsOpen = false;
            });
        }


        private void uiSpotPasswordTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (uiSpotPasswordTextBox.Text.Trim().Length > 0)
                uiSubmitPasswordButton.IsEnabled = true;
            else
                uiSubmitPasswordButton.IsEnabled = false;
        }


        private void uiSubmitPasswordButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (uiSubmitPasswordButton.IsEnabled)
            {
                uiSpotPasswordTextBox.Text = uiSpotPasswordTextBox.Text.Trim();
                if (this.SpotPassword.Equals(AESHelper.Encrypt(uiSpotPasswordTextBox.Text)))
                {
                    this.result = true;
                    this.Popup.IsOpen = false;
                }
                else
                {
                    MessageBox.Show(AppResources.PasswordWrongMessage, AppResources.PasswordWrongCaption, MessageBoxButton.OK);
                }
            }
        }


        private void uiSubmitSpotPasswordPopupCloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Popup.IsOpen = false;
        }
    }
}
