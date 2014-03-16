using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using PintheCloud.Managers;
using System.Threading.Tasks;
using PintheCloud.ViewModels;
using PintheCloud.Models;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using PintheCloud.Resources;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using PintheCloud.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using PintheCloud.Converters;
using System.Windows.Media;
using PintheCloud.Helpers;
using System.Windows.Controls.Primitives;
using PintheCloud.Popups;
using System.IO;

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Const Instances
        private const string PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING = "00A4BF";
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/pin_the_cloud/png/general_bar_upload.png";

        private const string EDIT_IMAGE_URI = "/Assets/pajeon/at_here/png/list_edit.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/at_here/png/list_view.png";

        private const string PICK_PIVOT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pick.png";
        private const string PIN_PIVOT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pin.png";
        private const string PICK_PIVOT_HIGHLIGHT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pick_highlight.png";
        private const string PIN_PIVOT_HIGHLIGHT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pin_highlight.png";


        // Instances
        private ApplicationBarIconButton PinFileAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;
        private Popup SubmitSpotPasswordParentPopup = new Popup();

        private string SpotId = null;
        private string SpotName = null;
        private string AccountId = null;
        private string AccountName = null;

        public FileObjectViewModel PickFileObjectViewModel = new FileObjectViewModel();
        public FileObjectViewModel PinFileObjectViewModel = new FileObjectViewModel();
        public List<FileObjectViewItem> SelectedFile = new List<FileObjectViewItem>();


        // Constructer
        public ExplorerPage()
        {
            InitializeComponent();

            // Set pin app bar button
            this.PinFileAppBarButton.Text = AppResources.Pin;
            this.PinFileAppBarButton.IconUri = new Uri(PIN_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PinFileAppBarButton.IsEnabled = false;
            this.PinFileAppBarButton.Click += PinFileAppBarButton_Click;

            // Set Cloud Setting selection.
            base.SetStorageBarMenuItem(out this.AppBarMenuItems, AppBarMenuItem_Click);

            // Set Datacontext
            uiPickFileList.DataContext = this.PickFileObjectViewModel;
            uiPinFileList.DataContext = this.PinFileObjectViewModel;
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //this.NavigationContext
            this.SpotId = NavigationContext.QueryString["spotId"];
            this.SpotName = NavigationContext.QueryString["spotName"];
            this.AccountId = NavigationContext.QueryString["accountId"];
            this.AccountName = NavigationContext.QueryString["accountName"];
            uiSpotNameText.Text = this.SpotName;

            this.SetPickPivot(AppResources.Loading);
            this.SetPinPivot(Switcher.GetCurrentStorage(), AppResources.Loading, false);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        // Construct pivot item by index
        private void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly. One time loading.
            PhoneApplicationService.Current.State[PIVOT_KEY] = uiExplorerPivot.SelectedIndex;
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EventHelper.PICK_PIVOT:
                    // Set Pick Pivot UI
                    ApplicationBar.Buttons.Remove(this.PinFileAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Remove(this.AppBarMenuItems[i]);
                    uiPickPivotImage.Source = new BitmapImage(new Uri(PICK_PIVOT_HIGHLIGHT_IMAGE_URI, UriKind.Relative));
                    uiPinPivotImage.Source = new BitmapImage(new Uri(PIN_PIVOT_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING));
                    uiCurrentCloudModeImage.Visibility = Visibility.Collapsed;

                    this.SetPickPivot(AppResources.Loading);
                    break;


                case EventHelper.PIN_PIVOT:
                    // Set Pin Pivot UI
                    IStorageManager iStorageManager = Switcher.GetCurrentStorage();
                    ApplicationBar.Buttons.Add(this.PinFileAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
                    uiPickPivotImage.Source = new BitmapImage(new Uri(PICK_PIVOT_IMAGE_URI, UriKind.Relative));
                    uiPinPivotImage.Source = new BitmapImage(new Uri(PIN_PIVOT_HIGHLIGHT_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
                    uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
                    uiCurrentCloudModeImage.Visibility = Visibility.Visible;

                    this.SetPinPivot(iStorageManager, AppResources.Loading, false);
                    break;
            }
        }


        private void SetPickPivot(string message)
        {
            // If internet is on, refresh
            // Otherwise, show internet unavailable message.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!this.PickFileObjectViewModel.IsDataLoaded)
                    this.SetPickPivotListAsync(AppResources.Loading);
            }
            else
            {
                base.SetListUnableAndShowMessage(uiPickFileList, uiPickFileListMessage, AppResources.InternetUnavailableMessage);
            }
        }


        private async void SetPickPivotListAsync(string message)
        {
            // Show Refresh message and Progress Indicator
            base.SetListUnableAndShowMessage(uiPickFileList, uiPickFileListMessage, message);
            base.SetProgressIndicator(true);

            // If file exists, show it.
            // Otherwise, show no file in spot message.
            List<FileObject> fileList = await App.BlobStorageManager.GetFilesFromSpotAsync(this.AccountId, this.SpotId);
            if (fileList.Count > 0)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.PickFileObjectViewModel.IsDataLoaded = true;
                    uiPickFileList.Visibility = Visibility.Visible;
                    uiPickFileListMessage.Visibility = Visibility.Collapsed;
                    this.PickFileObjectViewModel.SetItems(fileList, false);
                });
            }
            else
            {
                this.PickFileObjectViewModel.IsDataLoaded = true;
                base.SetListUnableAndShowMessage(uiPickFileList, uiPickFileListMessage, AppResources.NoFileInSpotMessage);
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }


        private void SetPinPivot(IStorageManager iStorageManager, string message, bool load)
        {
            // If it wasn't already signed in, show signin button.
            // Otherwise, load files
            if (!iStorageManager.IsSignIn())  // wasn't signed in.
            {
                iStorageManager.GetFolderRootTree().Clear();
                iStorageManager.GetFoldersTree().Clear();
                this.SelectedFile.Clear();
                this.PinFileAppBarButton.IsEnabled = false;

                uiPinFileListGrid.Visibility = Visibility.Collapsed;
                uiPinFileSignInPanel.Visibility = Visibility.Visible;
            }
            else  // already signed in.
            {
                uiPinFileListGrid.Visibility = Visibility.Visible;
                uiPinFileSignInPanel.Visibility = Visibility.Collapsed;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (!this.PinFileObjectViewModel.IsDataLoaded)
                    {
                        Stack<FileObjectViewItem> folderRootStack = iStorageManager.GetFolderRootTree();
                        if (folderRootStack.Count > 0)
                            this.SetPinFileListAsync(iStorageManager, message, folderRootStack.First(), load);
                        else
                            this.SetPinFileListAsync(iStorageManager, message, null, true);
                    }
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, AppResources.InternetUnavailableMessage);
                }
            }
        }


        private async void SetPinFileListAsync(IStorageManager iStorageManager, string message, FileObjectViewItem folder, bool load)
        {
            // Set Mutex true and Show Process Indicator            
            base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, message);
            base.SetProgressIndicator(true);

            // Clear selected file and set pin button false.
            this.SelectedFile.Clear();
            base.Dispatcher.BeginInvoke(() =>
            {
                this.PinFileAppBarButton.IsEnabled = false;
            });

            // Wait task
            await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());
            await App.TaskHelper.WaitSignOutTask(iStorageManager.GetStorageName());

            // If it wasn't signed out, set list.
            // Othersie, show sign in grid.
            if (iStorageManager.GetStorageAccount() != null)  // Wasn't signed out.
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
                if (iStorageManager.GetStorageName().Equals(iStorageManager.GetStorageName()))
                {
                    // Set file list visible and current path.
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.PinFileObjectViewModel.IsDataLoaded = true;
                        uiPinFileList.Visibility = Visibility.Visible;
                        this.SetCurrentPath(iStorageManager);
                        this.PinFileObjectViewModel.SetItems(fileObjects, true);
                    });

                    // If there exists file, show it.
                    // Otherwise, show no file message.
                    if (fileObjects.Count > 0)
                    {
                        base.Dispatcher.BeginInvoke(() =>
                        {
                            uiPinFileMessage.Visibility = Visibility.Collapsed;
                        });
                    }
                    else
                    {
                        base.Dispatcher.BeginInvoke(() =>
                        {
                            uiPinFileMessage.Text = AppResources.NoFileInFolderMessage;
                        });
                    }
                }
            }
            else  // Was signed out.
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinFileListGrid.Visibility = Visibility.Collapsed;
                    uiPinFileSignInPanel.Visibility = Visibility.Visible;
                });
            }

            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
        }


        // Refresh spot list.
        private void uiAppBarRefreshButton_Click(object sender, System.EventArgs e)
        {
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EventHelper.PICK_PIVOT:
                    this.PickFileObjectViewModel.IsDataLoaded = false;
                    this.SetPickPivot(AppResources.Refreshing);
                    break;


                case EventHelper.PIN_PIVOT:
                    this.PinFileObjectViewModel.IsDataLoaded = false;
                    this.SetPinPivot(Switcher.GetCurrentStorage(), AppResources.Refreshing, true);
                    break;
            }
        }


        // Move to Setting Page
        private void uiAppBarSettingsButton_Click(object sender, System.EventArgs e)
        {
            PhoneApplicationService.Current.State[PICK_FILE_OBJECT_VIEW_MODEL_KEY] = this.PickFileObjectViewModel;
            PhoneApplicationService.Current.State[PIN_FILE_OBJECT_VIEW_MODEL_KEY] = this.PinFileObjectViewModel;
            EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            NavigationService.Navigate(new Uri(EventHelper.SETTINGS_PAGE, UriKind.Relative));
        }



        /*** Pick Pivot ***/



        /*** Pin Pivot ***/

        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;

            if (Switcher.GetCurrentStorage().GetStorageName().Equals(appBarMenuItem.Text)) return;
            if (Switcher.GetCurrentStorage().IsSigningIn()) return;
            Switcher.SetStorageTo(appBarMenuItem.Text);

            // If it is not in current cloud mode, change it.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
            uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));

            this.PinFileObjectViewModel.IsDataLoaded = false;
            this.SetPinPivot(iStorageManager, AppResources.Loading, false);
        }


        private void PinFileAppBarButton_Click(object sender, EventArgs e)
        {
            List<FileObjectViewItem> fileList = new List<FileObjectViewItem>();
            foreach (FileObjectViewItem fileObjectViewItem in this.SelectedFile)
                fileList.Add(fileObjectViewItem);

            this.SelectedFile.Clear();
            this.PinFileAppBarButton.IsEnabled = false;

            foreach (FileObjectViewItem fileObjectViewItem in fileList)
                this.UploadFileAsync(new FileObjectViewItem(fileObjectViewItem));
        }



        // Upload. have to wait it.
        private async void UploadFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Uploading message and file for good UX
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.UPLOADING_IMAGE_URI;
            });

            // Upload
            Stream stream = await Switcher.GetCurrentStorage().DownloadFileStreamAsync((fileObjectViewItem.DownloadUrl == null ? fileObjectViewItem.Id : fileObjectViewItem.DownloadUrl));
            if (stream != null)
            {
                string blobId = await App.BlobStorageManager.UploadFileStreamAsync(this.AccountId, this.SpotId, fileObjectViewItem.Name, stream);
                if (blobId != null)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.Id = blobId;
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                    });
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                    });
                }
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide progress message
            base.SetProgressIndicator(false);
        }



        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            // If it is signing, don't close app.
            // Otherwise, show close app message.
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (iStorageManager.IsSigningIn())
            {
                e.Cancel = true;

                // If it is popup, close popup.
                if (iStorageManager.IsPopup())
                    EventHelper.TriggerEvent(EventHelper.POPUP_CLOSE);
            }
            else if (this.SubmitSpotPasswordParentPopup.IsOpen)
            {
                e.Cancel = true;
                this.SubmitSpotPasswordParentPopup.IsOpen = false;
            }
            else
            {
                // If some files in list, back tree.
                if (uiExplorerPivot.SelectedIndex == EventHelper.PIN_PIVOT)
                {
                    if (iStorageManager.GetFolderRootTree().Count > 1)
                    {
                        e.Cancel = true;
                        this.TreeUp(iStorageManager);
                        return;
                    }
                }

                MessageBoxResult result = MessageBox.Show(AppResources.CloseAppMessage, AppResources.CloseAppCaption, MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK)
                    e.Cancel = true;
            }
        }


        private void uiPinFileListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (iStorageManager.GetFolderRootTree().Count > 1)
                this.TreeUp(iStorageManager);
        }



        private void TreeUp(IStorageManager iStorageManager)
        {
            // If message is visible, set collapsed.
            if (uiPinFileMessage.Visibility == Visibility.Visible)
                uiPinFileMessage.Visibility = Visibility.Collapsed;

            // Clear trees.
            iStorageManager.GetFolderRootTree().Pop();
            iStorageManager.GetFoldersTree().Pop();
            this.SelectedFile.Clear();
            this.PinFileAppBarButton.IsEnabled = false;

            // Set previous files to list.
            this.PinFileObjectViewModel.SetItems(iStorageManager.GetFoldersTree().First(), true);
            this.SetCurrentPath(iStorageManager);
        }


        private void SetCurrentPath(IStorageManager iStorageManager)
        {
            FileObjectViewItem[] array = iStorageManager.GetFolderRootTree().Reverse<FileObjectViewItem>().ToArray<FileObjectViewItem>();
            uiPinFileCurrentPath.Text = String.Empty;
            foreach (FileObjectViewItem f in array)
                uiPinFileCurrentPath.Text = uiPinFileCurrentPath.Text + f.Name + "/";
        }


        // Pin file selection event.
        private void uiPinFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Object
            FileObjectViewItem fileObjectViewItem = uiPinFileList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiPinFileList.SelectedItem = null;


            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER))
                {
                    this.SetPinFileListAsync(Switcher.GetCurrentStorage(), AppResources.Loading, fileObjectViewItem, true);
                }
                else  // Do selection if file
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.SelectedFile.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PinFileAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.SelectedFile.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.SelectedFile.Count < 1)
                            this.PinFileAppBarButton.IsEnabled = false;
                    }
                }
            }
        }


        private async void uiPinFileSignInButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet available, Set pin list with root folder file list.
            // Otherwise, show internet bad message
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // Show Loading message and save is login true for pivot moving action while sign in.
                base.SetProgressIndicator(true);
                base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, AppResources.DoingSignIn);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinFileListGrid.Visibility = Visibility.Visible;
                    uiPinFileSignInPanel.Visibility = Visibility.Collapsed;
                });

                // Sign in and await that task.
                IStorageManager iStorageManager = Switcher.GetCurrentStorage();
                if (!iStorageManager.IsSigningIn())
                    App.TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());
                bool result = await App.TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());

                // If sign in success, set list.
                // Otherwise, show bad sign in message box.
                if (result)
                {
                    this.SetPinFileListAsync(iStorageManager, AppResources.Loading, null, true);
                }
                else
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinFileListGrid.Visibility = Visibility.Collapsed;
                        uiPinFileSignInPanel.Visibility = Visibility.Visible;
                    });
                }

                base.SetProgressIndicator(false);
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void uiPickFileListEditViewImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }


        private void uiPickFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}