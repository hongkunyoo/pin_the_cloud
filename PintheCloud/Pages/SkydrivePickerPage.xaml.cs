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

namespace PintheCloud.Pages
{
    public partial class SkyDrivePickerPage : PtcPage
    {
        // Instances
        public FileObjectViewModel SkyDriveFileObjectViewModel = new FileObjectViewModel();


        public SkyDrivePickerPage()
        {
            InitializeComponent();

            // TODO
            // Get file list using API
            SkyDriveFileObjectViewModel.Items = null; // this is the ObservableCollection List input
            this.DataContext = null;
            this.DataContext = SkyDriveFileObjectViewModel;
        }
    }
}