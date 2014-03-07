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

namespace PintheCloud.Popups
{
    public partial class SubmitSpotPasswordPopup : UserControl
    {
        private Popup Popup = null;
        private string SpotId = null;
        private string SpotPassword = null;
        public bool result = false;


        public SubmitSpotPasswordPopup(Popup popup, string spotId, string spotPassword)
        {
            InitializeComponent();
            this.Popup = popup;
            this.SpotId = spotId;
            this.SpotPassword = spotPassword;
        }

        private void uiSubmitPasswordButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string password = uiSpotPasswordTextBox.Text;
            if (!password.Trim().Equals(String.Empty))
            {
                if (this.SpotPassword.Equals(AESHelper.Encrypt(password)))
                {
                    this.result = true;
                    this.Popup.IsOpen = false;
                }
                else
                {
                    MessageBox.Show(AppResources.PasswordWrongMessage, AppResources.PasswordWrongCaption, MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show(AppResources.NoPasswordMessage, AppResources.NoPasswordCption, MessageBoxButton.OK);
            }
        }
    }
}
