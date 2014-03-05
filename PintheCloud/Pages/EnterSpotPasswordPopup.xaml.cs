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

namespace PintheCloud.Pages
{
    public partial class EnterSpotPasswordPopup : UserControl
    {
        private Popup Popup = null;


        public EnterSpotPasswordPopup(Popup popup)
        {
            InitializeComponent();
            this.Popup = popup;
        }

        // TODO Get password and check it.
        // Return bool value by password.
    }
}
