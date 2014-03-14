using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Helpers;
using System.Collections.ObjectModel;
using PintheCloud.Managers;

namespace PintheCloud.Pages
{
    public partial class SignInStoragePage : PhoneApplicationPage
    {
        public SignInStoragePage()
        {
            InitializeComponent();

            ////////////////////////////////////////////////
            // TODO : SEUNGMIN
            // I don't know how to bind with Template Plz change this shit.
            ////////////////////////////////////////////////
            ObservableCollection<string> list = new ObservableCollection<string>();

            ui_finish_btn.IsEnabled = false;

            using (var itr = StorageHelper.GetStorageEnumerator())
            {
                while (itr.MoveNext())
                {
                    list.Add(itr.Current.GetStorageName());
                }
            }

            ui_storage_list.DataContext = list;
        }

        private void ui_storage_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.

            string SelectedStorageName = ui_storage_list.SelectedItem as string;

            IStorageManager Storage = StorageHelper.GetStorageManager(SelectedStorageName);
            App.TaskHelper.AddSignInTask(Storage.GetStorageName(), Storage.SignIn());

            ui_finish_btn.IsEnabled = true;
        }

        private void ui_finish_btn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
        }
    }
}