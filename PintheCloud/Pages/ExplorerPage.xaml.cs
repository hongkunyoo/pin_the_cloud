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
using Windows.Storage;
using Windows.System;
using PintheCloud.Exceptions;

namespace PintheCloud.Pages
{
    public partial class ExplorerPage : PtcPage
    {
        // Const Instances
        private const string PICK_PIVOT_TITLE_GRID_COLOR_HEX_STRING = "00A4BF";
        private const string PICK_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/at_here/png/general_bar_download.png";
        private const string PIN_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/pin_the_cloud/png/general_bar_upload.png";
        private const string DELETE_APP_BAR_BUTTON_ICON_URI = "/Assets/pajeon/pin_the_cloud/png/general_bar_delete.png";

        private const string EDIT_IMAGE_URI = "/Assets/pajeon/at_here/png/list_edit.png";
        private const string VIEW_IMAGE_URI = "/Assets/pajeon/at_here/png/list_view.png";
        private const string EDIT_PRESS_IMAGE_URI = "/Assets/pajeon/at_here/png/list_edit_p.png";
        private const string VIEW_PRESS_IMAGE_URI = "/Assets/pajeon/at_here/png/list_view_p.png";

        private const string PICK_PIVOT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pick.png";
        private const string PIN_PIVOT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pin.png";
        private const string PICK_PIVOT_HIGHLIGHT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pick_highlight.png";
        private const string PIN_PIVOT_HIGHLIGHT_IMAGE_URI = "/Assets/pajeon/at_here/130315_png/tab_pin_highlight.png";


        // Instances
        private ApplicationBarIconButton PickDeleteAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarIconButton PickAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarIconButton PinFileAppBarButton = new ApplicationBarIconButton();
        private ApplicationBarMenuItem[] AppBarMenuItems = null;
        private Popup SubmitSpotPasswordParentPopup = new Popup();

        private string SpotId = null;
        private string SpotName = null;
        private string AccountId = null;
        private string AccountName = null;
        private bool LaunchLock = false;

        private FileObjectViewModel PickFileObjectViewModel = new FileObjectViewModel();
        private List<FileObjectViewItem> PickSelectedFileList = new List<FileObjectViewItem>();

        private FileObjectViewModel PinFileObjectViewModel = new FileObjectViewModel();
        private List<FileObjectViewItem> PinSelectedFileList = new List<FileObjectViewItem>();

        private List<FileObject> CurrentFileObjectList = new List<FileObject>();
        private SpotObject CurrentSpot = null;



        // Constructer
        public ExplorerPage()
        {
            InitializeComponent();

            // Set pin app bar button
            this.PickDeleteAppBarButton.Text = AppResources.Pick;
            this.PickDeleteAppBarButton.IconUri = new Uri(DELETE_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PickDeleteAppBarButton.IsEnabled = false;
            this.PickDeleteAppBarButton.Click += PickDeleteAppBarButton_Click;

            this.PickAppBarButton.Text = AppResources.Pick;
            this.PickAppBarButton.IconUri = new Uri(PICK_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PickAppBarButton.IsEnabled = false;
            this.PickAppBarButton.Click += PickAppBarButton_Click;

            this.PinFileAppBarButton.Text = AppResources.Pin;
            this.PinFileAppBarButton.IconUri = new Uri(PIN_APP_BAR_BUTTON_ICON_URI, UriKind.Relative);
            this.PinFileAppBarButton.IsEnabled = false;
            this.PinFileAppBarButton.Click += PinFileAppBarButton_Click;

            // Set Cloud Setting selection.
            base.SetStorageBarMenuItem(out this.AppBarMenuItems, AppBarMenuItem_Click);

            // Set Datacontext
            uiPickFileList.DataContext = this.PickFileObjectViewModel;
            uiPinFileList.DataContext = this.PinFileObjectViewModel;

            // Set event by previous page
            Context context = EventHelper.GetContext(EventHelper.EXPLORER_PAGE);
            context.HandleEvent(EventHelper.NEW_SPOT_PAGE, this.PreviousNewSpotPage);
            context.HandleEvent(EventHelper.SIGNIN_STORAGE_PAGE, this.PreviousSignInStoragePage);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.LaunchLock = false;
            this.SpotId = NavigationContext.QueryString["spotId"];
            this.SpotName = NavigationContext.QueryString["spotName"];
            this.AccountId = NavigationContext.QueryString["accountId"];
            this.AccountName = NavigationContext.QueryString["accountName"];
            uiSpotNameText.Text = this.SpotName;

            Switcher.SetStorageTo(Switcher.GetMainStorage().GetStorageName());
            this.CurrentSpot = App.SpotManager.GetSpotObject(this.SpotId);
            this.SetPickPivot(AppResources.Loading);
            this.SetPinPivot(AppResources.Loading);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        private void PreviousNewSpotPage()
        {
            NavigationService.RemoveBackEntry();
        }


        private void PreviousSignInStoragePage()
        {
            for (int i = 0; !NavigationService.BackStack.First().Source.ToString().Contains(EventHelper.SPOT_LIST_PAGE); i++)
                NavigationService.RemoveBackEntry();    
        }


        // Construct pivot item by index
        private void uiExplorerPivot_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Set View model for dispaly. One time loading.
            PhoneApplicationService.Current.State[PIVOT_KEY] = uiExplorerPivot.SelectedIndex;
            switch (uiExplorerPivot.SelectedIndex)
            {
                case EventHelper.PICK_PIVOT:
                    // Change edit view mode
                    string currentEditViewMode = uiPickFileListEditViewImageButton.ImageSource;
                    if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit mode
                    {
                        ApplicationBar.Buttons.Add(this.PickAppBarButton);
                        if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                            ApplicationBar.Buttons.Add(this.PickDeleteAppBarButton);
                    }
                    else if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View mode
                    {
                        ApplicationBar.Buttons.Remove(this.PickAppBarButton);
                        if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                            ApplicationBar.Buttons.Remove(this.PickDeleteAppBarButton);
                    }

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
                    ApplicationBar.Buttons.Remove(this.PickAppBarButton);
                    if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                        ApplicationBar.Buttons.Remove(this.PickDeleteAppBarButton);
                    ApplicationBar.Buttons.Add(this.PinFileAppBarButton);
                    for (int i = 0; i < AppBarMenuItems.Length; i++)
                        ApplicationBar.MenuItems.Add(this.AppBarMenuItems[i]);
                    uiPickPivotImage.Source = new BitmapImage(new Uri(PICK_PIVOT_IMAGE_URI, UriKind.Relative));
                    uiPinPivotImage.Source = new BitmapImage(new Uri(PIN_PIVOT_HIGHLIGHT_IMAGE_URI, UriKind.Relative));
                    uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
                    uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));
                    uiCurrentCloudModeImage.Visibility = Visibility.Visible;

                    this.SetPinPivot(AppResources.Loading);
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

            try
            {
                // Get files from the spot and set it to list.
                List<FileObject> fileList = await this.CurrentSpot.ListFileObjectsAsync();
                if (fileList.Count > 0)
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.PickFileObjectViewModel.IsDataLoaded = true;
                        uiPickFileList.Visibility = Visibility.Visible;
                        uiPickFileListMessage.Visibility = Visibility.Collapsed;
                        this.PickFileObjectViewModel.SetItems(fileList, false);

                        // Change edit view mode
                        string currentEditViewMode = uiPickFileListEditViewImageButton.ImageSource;
                        if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // To View mode
                        {
                            // Change select check image of each file object view item.
                            foreach (FileObjectViewItem fileObjectViewItem in this.PickFileObjectViewModel.Items)
                            {
                                if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI)
                                    || fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                            }
                        }

                        else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // To Edit mode
                        {
                            // Change select check image of each file object view item.
                            foreach (FileObjectViewItem fileObjectViewItem in this.PickFileObjectViewModel.Items)
                            {
                                if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                            }
                        }
                    });
                }
                else
                {
                    this.PickFileObjectViewModel.IsDataLoaded = true;
                    base.SetListUnableAndShowMessage(uiPickFileList, uiPickFileListMessage, AppResources.NoFileInSpotMessage);
                }
            }
            catch
            {
                base.SetListUnableAndShowMessage(uiPickFileList, uiPickFileListMessage, AppResources.BadLoadingFileMessage);
            }
            finally
            {
                base.SetProgressIndicator(false);
            }
        }


        private void SetPinPivot(string message)
        {
            // If it wasn't already signed in, show signin button.
            // Otherwise, load files
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            if (!iStorageManager.IsSignIn())  // wasn't signed in.
            {
                this.PinSelectedFileList.Clear();
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
                        this.SetPinFileListAsync(iStorageManager, message, null);
                }
                else
                {
                    base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, AppResources.InternetUnavailableMessage);
                }
            }
        }


        private async void SetPinFileListAsync(IStorageManager iStorageManager, string message, FileObjectViewItem folder)
        {

            // Set Mutex true and Show Process Indicator
            // Clear selected file and set pin button false.
            base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, message);
            base.SetProgressIndicator(true);
            this.PinSelectedFileList.Clear();
            base.Dispatcher.BeginInvoke(() =>
            {
                this.PinFileAppBarButton.IsEnabled = false;
            });


            try
            {
                // Wait Sync task
                await TaskHelper.WaitTask(TaskHelper.STORAGE_EXPLORER_SYNC + Switcher.GetCurrentStorage().GetStorageName());

                // Get files from current folder in the cloud.
                // If it is not null, set items.
                // Otherwise, show message
                if (folder == null)
                    this.CurrentFileObjectList = StorageExplorer.GetFilesFromRootFolder();
                else
                    this.CurrentFileObjectList = StorageExplorer.GetTreeForFolder(this.GetCloudStorageFileObjectById(folder.Id));
                if (this.CurrentFileObjectList != null)
                {
                    // If didn't change cloud mode while loading, set it to list.
                    // Set file list visible and current path.
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        this.PinFileObjectViewModel.IsDataLoaded = true;
                        uiPinFileList.Visibility = Visibility.Visible;
                        uiPinFileCurrentPath.Text = StorageExplorer.GetCurrentPath();
                        this.PinFileObjectViewModel.SetItems(this.CurrentFileObjectList, true);
                    });


                    // If there exists file, show it.
                    // Otherwise, show no file message.
                    if (this.CurrentFileObjectList.Count > 0)
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
                else
                {
                    base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, AppResources.BadLoadingFileMessage);
                }
            }
            catch
            {
                base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, AppResources.BadLoadingFileMessage);
            }
            finally
            {
                base.SetProgressIndicator(false);
            }
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
                    TaskHelper.AddTask(TaskHelper.STORAGE_EXPLORER_SYNC + Switcher.GetCurrentStorage().GetStorageName(), StorageExplorer.Refresh());
                    this.SetPinPivot(AppResources.Refreshing);
                    break;
            }
        }


        private void AppBarMenuItem_Click(object sender, EventArgs e)
        {
            // Get index
            ApplicationBarMenuItem appBarMenuItem = (ApplicationBarMenuItem)sender;

            if (Switcher.GetCurrentStorage().GetStorageName().Equals(appBarMenuItem.Text)) return;
            if (Switcher.GetCurrentStorage().IsSigningIn()) return;
            Switcher.SetStorageTo(appBarMenuItem.Text);

            // If it is not in current cloud mode, change it.
            uiPinFileCurrentPath.Text = "";
            IStorageManager iStorageManager = Switcher.GetCurrentStorage();
            uiPivotTitleGrid.Background = new SolidColorBrush(ColorHexStringToBrushConverter.GetColorFromHexString(iStorageManager.GetStorageColorHexString()));
            uiCurrentCloudModeImage.Source = new BitmapImage(new Uri(iStorageManager.GetStorageImageUri(), UriKind.Relative));

            this.PinFileObjectViewModel.IsDataLoaded = false;
            this.SetPinPivot(AppResources.Loading);
        }


        private void PinFileAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.PinFileAppBarButton.IsEnabled = false;
                foreach (FileObjectViewItem fileObjectViewItem in this.PinSelectedFileList)
                    this.PinFileAsync(fileObjectViewItem);
                this.PinSelectedFileList.Clear();
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void PinFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Uploading message and file for good UX
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.ING_IMAGE_URI;
            });


            // Upload file from cloud to spot.
            string blobId = await this.CurrentSpot.AddFileObjectAsync(this.GetCloudStorageFileObjectById(fileObjectViewItem.Id));
            if (blobId != null)
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.PickFileObjectViewModel.IsDataLoaded = false;
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
            else
            {
                // If some files in list, back tree.
                if (uiExplorerPivot.SelectedIndex == EventHelper.PIN_PIVOT)
                {
                    if (StorageExplorer.GetCurrentTree() != null && StorageExplorer.GetCurrentTree().Count > 1)
                    {
                        e.Cancel = true;
                        this.TreeUp();
                    }
                }
            }
        }


        private void uiPinFileListUpButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (StorageExplorer.GetCurrentTree() != null)
                this.TreeUp();
        }



        private async void TreeUp()
        {
            try
            {
                // Wait Sync work
                await TaskHelper.WaitTask(TaskHelper.STORAGE_EXPLORER_SYNC + Switcher.GetCurrentStorage().GetStorageName());

                // If message is visible, set collapsed.
                if (uiPinFileMessage.Visibility == Visibility.Visible && !uiPinFileMessage.Text.Equals(AppResources.Refreshing))
                    uiPinFileMessage.Visibility = Visibility.Collapsed;

                // Do tree up work and set items to list
                List<FileObject> fileList = StorageExplorer.TreeUp();
                this.CurrentFileObjectList = fileList;
                this.PinFileObjectViewModel.SetItems(this.CurrentFileObjectList, true);
                uiPinFileCurrentPath.Text = StorageExplorer.GetCurrentPath();

                // Clear trees.
                this.PinFileAppBarButton.IsEnabled = false;
                this.PinSelectedFileList.Clear();
            }
            catch
            {
                return;
            }
        }


        // Pin file selection event.
        private void uiPinFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Object
            // Set selected item to null for next selection of list item. 
            // If selected item isn't null, Do something
            FileObjectViewItem fileObjectViewItem = uiPinFileList.SelectedItem as FileObjectViewItem;
            uiPinFileList.SelectedItem = null;
            if (fileObjectViewItem != null)
            {
                // If user select folder, go in.
                // Otherwise, add it to list.
                if (fileObjectViewItem.ThumnailType.Equals(FileObjectViewModel.FOLDER))  // If folder, get files it the folder
                {
                    this.SetPinFileListAsync(Switcher.GetCurrentStorage(), AppResources.Loading, fileObjectViewItem);
                }
                else  // If file, Do selection
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.PinSelectedFileList.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PinFileAppBarButton.IsEnabled = true;
                    }
                    else if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.PinSelectedFileList.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.PinSelectedFileList.Count < 1)
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
                base.SetListUnableAndShowMessage(uiPinFileList, uiPinFileMessage, AppResources.DoingSignIn);
                base.Dispatcher.BeginInvoke(() =>
                {
                    uiPinFileListGrid.Visibility = Visibility.Visible;
                    uiPinFileSignInPanel.Visibility = Visibility.Collapsed;
                });


                try
                {
                    // Sign in and await that task.
                    IStorageManager iStorageManager = Switcher.GetCurrentStorage();
                    if (!iStorageManager.IsSigningIn())
                        TaskHelper.AddSignInTask(iStorageManager.GetStorageName(), iStorageManager.SignIn());
                    await TaskHelper.WaitSignInTask(iStorageManager.GetStorageName());

                    // If sign in success, set list.
                    base.SetProgressIndicator(true);
                    this.SetPinFileListAsync(iStorageManager, AppResources.Loading, null);
                }
                catch
                {
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show(AppResources.BadSignInMessage, AppResources.BadSignInCaption, MessageBoxButton.OK);
                        uiPinFileListGrid.Visibility = Visibility.Collapsed;
                        uiPinFileSignInPanel.Visibility = Visibility.Visible;
                        base.SetProgressIndicator(false);
                    });
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private void uiPickFileListEditViewImageButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Change edit view mode
            string currentEditViewMode = uiPickFileListEditViewImageButton.ImageSource;
            if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // To View mode
            {
                // Change mode image and remove app bar buttons.
                if (this.PickSelectedFileList.Count > 0)
                {
                    this.PickSelectedFileList.Clear();
                    this.PickAppBarButton.IsEnabled = false;
                    if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                        this.PickDeleteAppBarButton.IsEnabled = false;
                }
                ApplicationBar.Buttons.Remove(this.PickAppBarButton);
                if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                    ApplicationBar.Buttons.Remove(this.PickDeleteAppBarButton);
                uiPickFileListEditViewImageButton.ImageSource = EDIT_IMAGE_URI;
                uiPickFileListEditViewImageButton.ImagePressedSource = EDIT_PRESS_IMAGE_URI;

                // Change select check image of each file object view item.
                foreach (FileObjectViewItem fileObjectViewItem in this.PickFileObjectViewModel.Items)
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI)
                        || fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                }
            }

            else if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // To Edit mode
            {
                // Change mode image and remove app bar buttons.
                ApplicationBar.Buttons.Add(this.PickAppBarButton);
                if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                    ApplicationBar.Buttons.Add(this.PickDeleteAppBarButton);
                uiPickFileListEditViewImageButton.ImageSource = VIEW_IMAGE_URI;
                uiPickFileListEditViewImageButton.ImagePressedSource = VIEW_PRESS_IMAGE_URI;

                // Change select check image of each file object view item.
                foreach (FileObjectViewItem fileObjectViewItem in this.PickFileObjectViewModel.Items)
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                }
            }
        }


        private void uiPickFileList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Get Selected File Obejct
            FileObjectViewItem fileObjectViewItem = uiPickFileList.SelectedItem as FileObjectViewItem;

            // Set selected item to null for next selection of list item. 
            uiPickFileList.SelectedItem = null;

            // If selected item isn't null, Do something
            if (fileObjectViewItem != null)
            {
                // If it is view mode, click is preview.
                // If it is edit mode, click is selection.
                string currentEditViewMode = uiPickFileListEditViewImageButton.ImageSource;
                if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View mode
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.TRANSPARENT_IMAGE_URI))
                    {
                        // Launch files to other reader app.
                        if (!this.LaunchLock)
                        {
                            this.LaunchLock = true;
                            this.LaunchFileAsync(fileObjectViewItem);
                        }
                    }
                }
                else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit mode
                {
                    if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_NOT_IMAGE_URI))
                    {
                        this.PickSelectedFileList.Add(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_IMAGE_URI;
                        this.PickAppBarButton.IsEnabled = true;
                        if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                            this.PickDeleteAppBarButton.IsEnabled = true;
                    }

                    else if (fileObjectViewItem.SelectFileImage.Equals(FileObjectViewModel.CHECK_IMAGE_URI))
                    {
                        this.PickSelectedFileList.Remove(fileObjectViewItem);
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                        if (this.PickSelectedFileList.Count < 1)
                        {
                            this.PickAppBarButton.IsEnabled = false;
                            if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                                this.PickDeleteAppBarButton.IsEnabled = false;
                        }
                    }
                }
            }
        }



        private async void LaunchFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Downloading message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.ING_IMAGE_URI;
            });

            // Download file and Launch files to other reader app.
            if (await this.CurrentSpot.PreviewFileObjectAsync(this.CurrentSpot.GetFileObject(fileObjectViewItem.Id)))
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                });
            }
            else
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }



        // Download files.
        private void PickAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                IStorageManager iStr = Switcher.GetCurrentStorage();
                if (iStr.IsSignIn())
                {
                    this.PickAppBarButton.IsEnabled = false;
                    if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                        this.PickDeleteAppBarButton.IsEnabled = false;
                    foreach (FileObjectViewItem fileObjectViewItem in this.PickSelectedFileList)
                        this.PickFileAsync(iStr, fileObjectViewItem);
                    this.PickSelectedFileList.Clear();
                }
                else
                {
                    MessageBoxResult result = MessageBox.Show(AppResources.NoMainCloudSignInMessage, AppResources.MainCloud, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        string parameters = "?spotId=" + this.SpotId + "&spotName=" + this.SpotName + "&accountId=" + this.AccountId + "&accountName=" + this.AccountName;
                        NavigationService.Navigate(new Uri(EventHelper.SIGNIN_STORAGE_PAGE + parameters, UriKind.Relative));
                    }
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void PickFileAsync(IStorageManager storageManager, FileObjectViewItem fileObjectViewItem)
        {
            // Show Downloading message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.ING_IMAGE_URI;
            });

            try
            {
                // Download
                await TaskHelper.WaitSignInTask(storageManager.GetStorageName());
                await this.CurrentSpot.DownloadFileObjectAsync(storageManager, this.CurrentSpot.GetFileObject(fileObjectViewItem.Id));
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.PinFileObjectViewModel.IsDataLoaded = false;
                    string currentEditViewMode = uiPickFileListEditViewImageButton.ImageSource;
                    if (currentEditViewMode.Equals(EDIT_IMAGE_URI))  // View Mode
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.TRANSPARENT_IMAGE_URI;
                    else if (currentEditViewMode.Equals(VIEW_IMAGE_URI))  // Edit Mode
                        fileObjectViewItem.SelectFileImage = FileObjectViewModel.CHECK_NOT_IMAGE_URI;
                });
            }
            catch
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }
            finally
            {
                base.SetProgressIndicator(false); 
            }
        }


        private FileObject GetCloudStorageFileObjectById(string fileId)
        {
            if (fileId == null) return null;
            for (int i = 0; i < this.CurrentFileObjectList.Count; i++)
            {
                if (this.CurrentFileObjectList[i] == null) return null;
                if (this.CurrentFileObjectList[i].Id == null) return null;
                if (this.CurrentFileObjectList[i].Id.Equals(fileId)) return this.CurrentFileObjectList[i];
            }
            return null;
        }


        // Delete files.
        private void PickDeleteAppBarButton_Click(object sender, EventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBoxResult result = MessageBox.Show(AppResources.DeleteFileMessage, AppResources.DeleteFileCaption, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    this.PickAppBarButton.IsEnabled = false;
                    if (this.AccountId.Equals(App.AccountManager.GetPtcId()))
                        this.PickDeleteAppBarButton.IsEnabled = false;
                    foreach (FileObjectViewItem fileObjectViewItem in this.PickSelectedFileList)
                        this.DeleteFileAsync(fileObjectViewItem);
                    this.PickSelectedFileList.Clear();
                }
            }
            else
            {
                MessageBox.Show(AppResources.InternetUnavailableMessage, AppResources.InternetUnavailableCaption, MessageBoxButton.OK);
            }
        }


        private async void DeleteFileAsync(FileObjectViewItem fileObjectViewItem)
        {
            // Show Deleting message
            base.SetProgressIndicator(true);
            base.Dispatcher.BeginInvoke(() =>
            {
                fileObjectViewItem.SelectFileImage = FileObjectViewModel.ING_IMAGE_URI;
            });


            // Delete
            try
            {
                await App.BlobStorageManager.DeleteFileAsync(fileObjectViewItem.Id);
                base.Dispatcher.BeginInvoke(() =>
                {
                    this.PickFileObjectViewModel.Items.Remove(fileObjectViewItem);
                    if (this.PickFileObjectViewModel.Items.Count < 1)
                        base.SetListUnableAndShowMessage(uiPickFileList, uiPickFileListMessage, AppResources.NoFileInSpotMessage);
                });
            }
            catch
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    fileObjectViewItem.SelectFileImage = FileObjectViewModel.FAIL_IMAGE_URI;
                });
            }

            // Hide Progress Indicator
            base.SetProgressIndicator(false);
        }
    }
}