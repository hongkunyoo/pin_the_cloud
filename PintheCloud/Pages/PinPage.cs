using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using PintheCloud.Managers;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.Devices.Geolocation;
using System.Windows;
using PintheCloud.Resources;
using PintheCloud.Models;
using Microsoft.Phone.Shell;

namespace PintheCloud.Pages
{
    public class PinPage : PtcPage
    {
        public PinPage()
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }



        /*** Pin Methods ***/

        protected void SetPinPivot(IStorageManager iStorageManager, LongListSelector list, TextBlock messageTextBlock, string message,
            TextBlock pathTextBlock, Grid listGrid, StackPanel signInPanel, ApplicationBarIconButton pinInfoAppBarButton,
            FileObjectViewModel viewModel, List<FileObjectViewItem> selectedFile, bool load)
        {
            // If it wasn't already signed in, show signin button.
            // Otherwise, load files
            if (!iStorageManager.IsSignIn())  // wasn't signed in.
            {
                iStorageManager.GetFolderRootTree().Clear();
                iStorageManager.GetFoldersTree().Clear();
                selectedFile.Clear();
                pinInfoAppBarButton.IsEnabled = false;

                listGrid.Visibility = Visibility.Collapsed;
                signInPanel.Visibility = Visibility.Visible;
            }
            else  // already signed in.
            {
                listGrid.Visibility = Visibility.Visible;
                signInPanel.Visibility = Visibility.Collapsed;

                if (NetworkInterface.GetIsNetworkAvailable())
                {
                    if (!viewModel.IsDataLoaded)
                    {
                        Stack<FileObjectViewItem> folderRootStack = iStorageManager.GetFolderRootTree();
                        if (folderRootStack.Count > 0)
                            this.SetPinInfoListAsync(iStorageManager, list, messageTextBlock, message, pathTextBlock, listGrid, signInPanel, pinInfoAppBarButton,
                                viewModel, selectedFile, folderRootStack.First(), load);
                        else
                            this.SetPinInfoListAsync(iStorageManager, list, messageTextBlock, message, pathTextBlock, listGrid, signInPanel, pinInfoAppBarButton,
                                viewModel, selectedFile, null, true);
                    }
                }
                else
                {
                    base.SetListUnableAndShowMessage(list, messageTextBlock, AppResources.InternetUnavailableMessage);
                }
            }
        }


        protected async void SetPinInfoListAsync(IStorageManager iStorageManager, LongListSelector list, TextBlock messageTextBlock, string message,
            TextBlock pathTextBlock, Grid listGrid, StackPanel signInPanel, ApplicationBarIconButton pinInfoAppBarButton,
            FileObjectViewModel viewModel, List<FileObjectViewItem> selectedFile, FileObjectViewItem folder, bool load)
        {
            // Set Mutex true and Show Process Indicator
            viewModel.IsDataLoaded = true;
            base.SetListUnableAndShowMessage(list, messageTextBlock, message);
            base.SetProgressIndicator(true);

            // Clear selected file and set pin button false.
            selectedFile.Clear();
            base.Dispatcher.BeginInvoke(() =>
            {
                pinInfoAppBarButton.IsEnabled = false;
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
                if (iStorageManager.GetStorageName().Equals(iStorageManager.GetStorageName()))
                {
                    // Set file list visible and current path.
                    base.Dispatcher.BeginInvoke(() =>
                    {
                        list.Visibility = Visibility.Visible;
                        this.SetCurrentPath(iStorageManager, pathTextBlock);
                        viewModel.SetItems(fileObjects, true);
                    });

                    // If there exists file, show it.
                    // Otherwise, show no file message.
                    if (fileObjects.Count > 0)
                    {
                        base.Dispatcher.BeginInvoke(() =>
                        {
                            messageTextBlock.Visibility = Visibility.Collapsed;
                        });
                    }
                    else
                    {
                        base.Dispatcher.BeginInvoke(() =>
                        {
                            messageTextBlock.Text = AppResources.NoFileInFolderMessage;
                        });
                    }
                }
            }
            else  // Was signed out.
            {
                base.Dispatcher.BeginInvoke(() =>
                {
                    listGrid.Visibility = Visibility.Collapsed;
                    signInPanel.Visibility = Visibility.Visible;
                });
            }

            // Set Mutex false and Hide Process Indicator
            base.SetProgressIndicator(false);
        }


        protected void TreeUp(IStorageManager iStorageManager, TextBlock messageTextBlock, TextBlock pathTextBlock, ApplicationBarIconButton pinInfoAppBarButton,
            FileObjectViewModel viewModel, List<FileObjectViewItem> selectedFile)
        {
            // If message is visible, set collapsed.
            if (messageTextBlock.Visibility == Visibility.Visible)
                messageTextBlock.Visibility = Visibility.Collapsed;

            // Clear trees.
            iStorageManager.GetFolderRootTree().Pop();
            iStorageManager.GetFoldersTree().Pop();
            selectedFile.Clear();
            pinInfoAppBarButton.IsEnabled = false;

            // Set previous files to list.
            viewModel.SetItems(iStorageManager.GetFoldersTree().First(), true);
            this.SetCurrentPath(iStorageManager, pathTextBlock);
        }


        private void SetCurrentPath(IStorageManager iStorageManager, TextBlock pathTextBlock)
        {
            FileObjectViewItem[] array = iStorageManager.GetFolderRootTree().Reverse<FileObjectViewItem>().ToArray<FileObjectViewItem>();
            pathTextBlock.Text = String.Empty;
            foreach (FileObjectViewItem f in array)
                pathTextBlock.Text = pathTextBlock.Text + f.Name + "/";
        }
    }
}
