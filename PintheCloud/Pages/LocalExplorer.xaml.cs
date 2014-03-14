using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Storage;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.System;

namespace PintheCloud.Pages
{
    public partial class LocalExplorer : PhoneApplicationPage
    {
        IReadOnlyList<StorageFile> files = null;
        ObservableCollection<string> nameList = new ObservableCollection<string>();
        public LocalExplorer()
        {
            InitializeComponent();

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            await SetFileList();
        }
        public async Task SetFileList()
        {
            files = await ApplicationData.Current.LocalFolder.GetFilesAsync();

            for(var i = 0; i < files.Count ; i++)
            {
                nameList.Add(files[i].Name);
            }

            ui_local_list.ItemsSource = nameList;
        }

        private async void ui_local_list_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.

            string name = ui_local_list.SelectedItem as string;

            await Launcher.LaunchFileAsync(files[nameList.IndexOf(name)]);
        }
    }
}