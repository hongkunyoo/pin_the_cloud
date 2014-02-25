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
using PintheCloud.Models;
using PintheCloud.Managers;
using System.Net.NetworkInformation;
using PintheCloud.Resources;

namespace PintheCloud.Pages
{
    public partial class PinPickerUserControl : UserControl
    {
        private int CurrentPlatformIndex = 0;

        public FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();

        public Stack<List<FileObject>> FoldersTree = new Stack<List<FileObject>>();
        public Stack<FileObjectViewItem> FolderRootTree = new Stack<FileObjectViewItem>();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();



        public PinPickerUserControl()
        {
            InitializeComponent();

            // Set Datacontext
            uiPinInfoList.DataContext = this.FileObjectViewModel;
        }


        public void RefreshPinInforList(int currentPlatformIndex)
        {
            // Refresh only in was already signed in.
            if (uiPinInfoListGrid.Visibility == Visibility.Visible)
            {
                this.SelectedFile.Clear();
                if (NetworkInterface.GetIsNetworkAvailable())
                    this.SetPinInfoListAsync(this.FolderRootTree.First(), AppResources.Refreshing, App.IStorageManagers[currentPlatformIndex], currentPlatformIndex);
                else
                    this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
            }
        }


        public void PinInfo()
        {
            // Check whether user consented for location access.
            if (this.GetLocationAccessConsent())  // Got consent of location access.
            {
                // Check whether GPS is on or not
                if (App.GeoHelper.GetGeolocatorPositionStatus())  // GPS is on
                {
                    PhoneApplicationService.Current.State[SPOT_VIEW_MODEL_KEY] = this.NearSpotViewModel;
                    PhoneApplicationService.Current.State[FILE_OBJECT_VIEW_MODEL_KEY] = this.FileObjectViewModel;
                    PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                    PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                    NavigationService.Navigate(new Uri(EventHelper.FILE_LIST_PAGE, UriKind.Relative));
                }
                else  // GPS is off
                {
                    MessageBox.Show(AppResources.NoLocationServiceMessage, AppResources.NoLocationServiceCaption, MessageBoxButton.OK);
                }
            }
            else  // First or not consented of access in location information.
            {
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }


        private string GetCurrentPath()
        {
            FileObjectViewItem[] array = this.FolderRootTree.Reverse<FileObjectViewItem>().ToArray<FileObjectViewItem>();
            string str = "";
            foreach (FileObjectViewItem f in array)
                str += f.Name + "/";
            return str;
        }


        public void TreeUp()
        {
            // If message is visible, set collapsed.
            if (uiPinInfoMessage.Visibility == Visibility.Visible)
                uiPinInfoMessage.Visibility = Visibility.Collapsed;

            // Clear trees.
            this.FolderRootTree.Pop();
            this.FoldersTree.Pop();
            this.SelectedFile.Clear();

            // Set previous files to list.
            this.FileObjectViewModel.SetItems(this.FoldersTree.First(), true);
            uiPinInfoCurrentPath.Text = this.GetCurrentPath();
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                this.SetListUnableAndShowMessage(uiPinInfoList, AppResources.DoingSignIn, uiPinInfoMessage);
                this.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInGrid.Visibility = Visibility.Collapsed;
                });

                // Sign in and await that task.
                IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                App.TaskManager.AddSignInTask(iStorageManager.SignIn(), this.CurrentPlatformIndex);
                await App.TaskManager.WaitSignInTask(this.CurrentPlatformIndex);

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (iStorageManager.GetAccount() != null)
                {
                    this.SetPinInfoListAsync(null, AppResources.Loading, iStorageManager, this.CurrentPlatformIndex);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        public async void SetPinInfoListAsync(FileObjectViewItem folder, string message, IStorageManager iStorageManager, int currentPlatformIndex)
        {
            // Set Mutex true and Show Process Indicator
            this.FileObjectViewModel.IsDataLoaded = true;
            this.FileObjectViewModel.IsDataLoading = true;
            this.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            this.SetProgressIndicator(true);

            // Wait tasks
            await App.TaskManager.WaitSignInTask(currentPlatformIndex);
            await App.TaskManager.WaitSignOutTask(currentPlatformIndex);


            // If it haven't signed out before working below code, do it.
            if (iStorageManager.GetAccount() != null)
            {
                // Delete selected file and If folder null, set root.
                this.SelectedFile.Clear();
                if (folder == null)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoCurrentPath.Text = "";
                    });

                    this.FolderRootTree.Clear();
                    this.FoldersTree.Clear();

                    FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
                    folder = new FileObjectViewItem();
                    folder.Id = rootFolder.Id;
                }

                // Get files and push to stack tree.
                List<FileObject> files = await iStorageManager.GetFilesFromFolderAsync(folder.Id);
                if (!message.Equals(AppResources.Refreshing))
                {
                    this.FolderRootTree.Push(folder);
                    this.FoldersTree.Push(files);
                }

                // Set file list visible and current path.
                base.Dispatcher.BeginInvoke(() =>
                {
                    // Set file tree to list.
                    uiPinInfoList.Visibility = Visibility.Visible;
                    uiPinInfoCurrentPath.Text = this.GetCurrentPath();
                    this.FileObjectViewModel.SetItems(files, true);
                });

                // If there exists file, show it.
                // Otherwise, show no file message.
                if (files.Count > 0)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoMessage.Visibility = Visibility.Collapsed;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoMessage.Text = AppResources.NoFileInFolderMessage;
                    });
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInGrid.Visibility = Visibility.Visible;
                });
            }


            // Set Mutex false and Hide Process Indicator
            this.SetProgressIndicator(false);
            this.FileObjectViewModel.IsDataLoading = false;
        }


        public void SetProgressIndicator(bool value, string text = "")
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                ProgressIndicator progressIndicator = new ProgressIndicator();
                progressIndicator.IsIndeterminate = value;
                progressIndicator.IsVisible = value;
                progressIndicator.Text = text;
                SystemTray.SetProgressIndicator(this, progressIndicator);
            });
        }


        private void SetListUnableAndShowMessage(LongListSelector list, string message, TextBlock messageTextBlock)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                list.Visibility = Visibility.Collapsed;
                messageTextBlock.Text = message;
                messageTextBlock.Visibility = Visibility.Visible;
            });
        }


        private bool GetLocationAccessConsent()
        {
            bool locationAccess = false;
            App.ApplicationSettings.TryGetValue<bool>(Account.LOCATION_ACCESS_CONSENT_KEY, out locationAccess);
            if (!locationAccess)  // First or not consented of access in location information.
            {
                MessageBoxResult result = MessageBox.Show(AppResources.LocationAccessMessage, AppResources.LocationAccessCaption, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = true;
                else
                    App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY] = false;
                App.ApplicationSettings.Save();
            }
            return (bool)App.ApplicationSettings[Account.LOCATION_ACCESS_CONSENT_KEY];
        }
    }
}
