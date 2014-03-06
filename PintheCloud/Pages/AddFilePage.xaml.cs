using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Managers;
using PintheCloud.Resources;
using PintheCloud.ViewModels;
using System.Windows.Media;
using PintheCloud.Converters;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Models;

namespace PintheCloud.Pages
{
    public partial class AddFilePage : PtcPage
    {
        // Const Instances
        private int PIN_INFO_APP_BAR_BUTTON_INDEX = 1;

        // Instances
        private int CurrentPlatformIndex = 0;
        private ApplicationBarIconButton PinInfoAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;

        public FileObjectViewModel FileObjectViewModel = new FileObjectViewModel();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();



        public AddFilePage()
        {
            InitializeComponent();

            // Check main platform and set current platform index.
            this.CurrentPlatformIndex = (int)App.ApplicationSettings[Account.ACCOUNT_MAIN_PLATFORM_TYPE_KEY];

            // Set Datacontext
            uiPinInfoList.DataContext = this.FileObjectViewModel;

            // Set Pin Infor app bar button and Cloud Setting selection.
            this.PinInfoAppBarButton = (ApplicationBarIconButton)ApplicationBar.Buttons[PIN_INFO_APP_BAR_BUTTON_INDEX];
            this.AppBarMenuItems = new ApplicationBarMenuItem[App.IStorageManagers.Length];
            for (int i = 0; i < this.AppBarMenuItems.Length; i++)
            {
                this.AppBarMenuItems[i] = new ApplicationBarMenuItem();
                this.AppBarMenuItems[i].Text = App.IStorageManagers[i].GetStorageName();
                this.AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }

            // Set Cloud Setting selection.
            this.AppBarMenuItems = new ApplicationBarMenuItem[App.IStorageManagers.Length];
            for (int i = 0; i < this.AppBarMenuItems.Length; i++)
            {
                this.AppBarMenuItems[i] = new ApplicationBarMenuItem();
                this.AppBarMenuItems[i].Text = App.IStorageManagers[i].GetStorageName();
                this.AppBarMenuItems[i].Click += AppBarMenuItem_Click;
            }
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Set Pin Pivot UI
            for (int i = 0; i < AppBarMenuItems.Length; i++)
                ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
            uiPivotTitleGrid.Background = new SolidColorBrush(
                ColorHexStringToBrushConverter.GetColorFromHexString(App.IStorageManagers[this.CurrentPlatformIndex].GetStorageColorHexString()));
            uiCurrentCloudModeImage.Source = new BitmapImage(
                new Uri(App.IStorageManagers[this.CurrentPlatformIndex].GetStorageImageUri(), UriKind.Relative));
            this.SetPinPivot();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void SetPinPivot()
        {
            // If it wasn't already signed in, show signin button.
            // Otherwise, load files
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (!iStorageManager.IsSignIn())  // wasn't signed in.
            {
                iStorageManager.GetFolderRootTree().Clear();
                iStorageManager.GetFoldersTree().Clear();
                this.SelectedFile.Clear();
                this.PinInfoAppBarButton.IsEnabled = false;

                uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                uiPinInfoSignInPanel.Visibility = Visibility.Visible;
            }
            else  // already signed in.
            {
                uiPinInfoListGrid.Visibility = Visibility.Visible;
                uiPinInfoSignInPanel.Visibility = Visibility.Collapsed;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (iStorageManager.GetFolderRootTree().Count > 0)
                        this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, false);
                    else
                        this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, true, null);
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }
        }


        private void uiAppBarPinInfoButton_Click(object sender, System.EventArgs e)
        {
            // Check whether user consented for location access.
            if (base.GetLocationAccessConsent())  // Got consent of location access.
            {
                PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                NavigationService.GoBack();
            }
            else  // First or not consented of access in location information.
            {
                MessageBox.Show(AppResources.NoLocationAcessConsentMessage, AppResources.NoLocationAcessConsentCaption, MessageBoxButton.OK);
            }
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            // If it wasn't already signed in, show signin button.
            // Otherwise, load files
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (!iStorageManager.IsSignIn())  // wasn't signed in.
            {
                iStorageManager.GetFolderRootTree().Clear();
                iStorageManager.GetFoldersTree().Clear();
                this.SelectedFile.Clear();
                this.PinInfoAppBarButton.IsEnabled = false;

                uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                uiPinInfoSignInPanel.Visibility = Visibility.Visible;
            }
            else  // already signed in.
            {
                uiPinInfoListGrid.Visibility = Visibility.Visible;
                uiPinInfoSignInPanel.Visibility = Visibility.Collapsed;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    Stack<FileObjectViewItem> folderRoot = iStorageManager.GetFolderRootTree();
                    if (folderRoot.Count > 0)
                        this.SetPinInfoListAsync(iStorageManager, AppResources.Refreshing, true, folderRoot.First());
                    else
                        this.SetPinInfoListAsync(iStorageManager, AppResources.Refreshing, true, null);
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                }
            }
        }


        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;
            int platformIndex = base.GetPlatformIndexFromString(appBarMenuItem.Text);


            // If it is not in current cloud mode, change it.
            if (this.CurrentPlatformIndex != platformIndex)
            {
                // Kill previous job.

                IStorageManager iStorageManager = App.IStorageManagers[platformIndex];
                uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
                uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
                this.CurrentPlatformIndex = platformIndex;

                // If it is already signin, load files.
                // Otherwise, show signin button.
                if (!iStorageManager.IsSignIn())
                {
                    iStorageManager.GetFolderRootTree().Clear();
                    iStorageManager.GetFoldersTree().Clear();
                    this.SelectedFile.Clear();
                    this.PinInfoAppBarButton.IsEnabled = false;

                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    // Show Loading message and save is login true for pivot moving action while sign in.
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInPanel.Visibility = Visibility.Collapsed;

                    if (NetworkInterface.GetIsNetworkAvailable())
                    {
                        if (iStorageManager.GetFolderRootTree().Count > 0)
                            this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, false);
                        else
                            this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, true, null);
                    }
                    else
                    {
                        base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.InternetUnavailableMessage, uiPinInfoMessage);
                    }
                }
            }
        }


        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (iStorageManager.IsSigningIn())
            {
                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                {
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
                    e.Cancel = true;
                    return;
                }
            }
            else
            {
                // If some files in list, back tree.
                if (iStorageManager.GetFolderRootTree().Count > 1)
                {
                    e.Cancel = true;
                    this.TreeUp(iStorageManager);
                }
                else
                {
                    this.SelectedFile.Clear();
                    PhoneApplicationService.Current.State[SELECTED_FILE_KEY] = this.SelectedFile;
                    PhoneApplicationService.Current.State[PLATFORM_KEY] = this.CurrentPlatformIndex;
                }
            }
        }


        private void uiPinInfoListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
            if (iStorageManager.GetFolderRootTree().Count > 1)
                this.TreeUp(iStorageManager);
        }


        // Pin file selection event.
        private void uiPinInfoList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Object
            FileObjectViewItem fileObjectViewItem = uiPinInfoList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiPinInfoList.SelectedItem = null;


            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER))
                {
                    this.SetPinInfoListAsync(App.IStorageManagers[this.CurrentPlatformIndex], AppResources.Loading, true, fileObjectViewItem);
                }
                else  // Do selection if file
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PinInfoAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count < 1)
                            this.PinInfoAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        private void SetCurrentPath(IStorageManager iStorageManager)
        {
            FileObjectViewItem[] array = iStorageManager.GetFolderRootTree().Reverse<FileObjectViewItem>().ToArray<FileObjectViewItem>();
            uiPinInfoCurrentPath.Text = String.Empty;
            foreach (FileObjectViewItem f in array)
                uiPinInfoCurrentPath.Text = uiPinInfoCurrentPath.Text + f.Name + "/";
        }


        private void TreeUp(IStorageManager iStorageManager)
        {
            // If message is visible, set collapsed.
            if (uiPinInfoMessage.Visibility == Visibility.Visible)
                uiPinInfoMessage.Visibility = Visibility.Collapsed;

            // Clear trees.
            iStorageManager.GetFolderRootTree().Pop();
            iStorageManager.GetFoldersTree().Pop();
            this.SelectedFile.Clear();
            this.PinInfoAppBarButton.IsEnabled = false;

            // Set previous files to list.
            this.FileObjectViewModel.SetItems(iStorageManager.GetFoldersTree().First(), true);
            this.SetCurrentPath(iStorageManager);
        }


        private async void uiPinInfoSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                base.SetListUnableAndShowMessage(uiPinInfoList, AppResources.DoingSignIn, uiPinInfoMessage);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Visible;
                    uiPinInfoSignInPanel.Visibility = Visibility.Collapsed;
                });

                // Sign in and await that task.
                IStorageManager iStorageManager = App.IStorageManagers[this.CurrentPlatformIndex];
                if (!iStorageManager.IsSigningIn())
                    App.TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());
                bool result = await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (result)
                {
                    this.SetPinInfoListAsync(iStorageManager, AppResources.Loading, true, null);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                        uiPinInfoSignInPanel.Visibility = Visibility.Visible;
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void SetPinInfoListAsync(IStorageManager iStorageManager, string message, bool load, FileObjectViewItem folder = null)
        {
            // Set Mutex true and Show Process Indicator
            base.SetListUnableAndShowMessage(uiPinInfoList, message, uiPinInfoMessage);
            base.SetProgressIndicator(true);

            // Clear selected file and set pin button false.
            this.SelectedFile.Clear();
            base.Dispatcher.BeginInvoke(() =>
            {
                this.PinInfoAppBarButton.IsEnabled = false;
            });

            // Wait task
            await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
            await App.TaskHelper.WaitSignOutTask(iStorageManager.GetStorageName());

            // If it wasn't signed out, set list.
            // Othersie, show sign in grid.
            if (iStorageManager.GetAccount() != null)  // Wasn't signed out.
            {
                // If it has to load, load new files.
                // Otherwise, set existing files to list.
                List<FileObject> fileObjects = new List<FileObject>();
                if (load)  // Load from server
                {
                    // If folder null, set root.
                    if (folder == null)
                    {
                        iStorageManager.GetFolderRootTree().Clear();
                        iStorageManager.GetFoldersTree().Clear();

                        FileObject rootFolder = await iStorageManager.GetRootFolderAsync();
                        folder = new FileObjectViewItem();
                        folder.Id = rootFolder.Id;
                    }

                    // Get files and push to stack tree.
                    fileObjects = await iStorageManager.GetFilesFromFolderAsync(folder.Id);
                    if (!message.Equals(AppResources.Refreshing))
                    {
                        iStorageManager.GetFoldersTree().Push(fileObjects);
                        if (!iStorageManager.GetFolderRootTree().Contains(folder))
                            iStorageManager.GetFolderRootTree().Push(folder);
                    }
                }
                else  // Set existed file to list
                {
                    fileObjects = iStorageManager.GetFoldersTree().First();
                }


                // If didn't change cloud mode while loading, set it to list.
                if (iStorageManager.GetStorageName().Equals(App.IStorageManagers[this.CurrentPlatformIndex].GetStorageName()))
                {
                    // Set file list visible and current path.
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        uiPinInfoList.Visibility = Visibility.Visible;
                        this.SetCurrentPath(iStorageManager);
                        this.FileObjectViewModel.SetItems(fileObjects, true);
                    });

                    // If there exists file, show it.
                    // Otherwise, show no file message.
                    if (fileObjects.Count > 0)
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
            }
            else  // Was signed out.
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinInfoListGrid.Visibility = Visibility.Collapsed;
                    uiPinInfoSignInPanel.Visibility = Visibility.Visible;
                });
            }

            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
        }
    }
}