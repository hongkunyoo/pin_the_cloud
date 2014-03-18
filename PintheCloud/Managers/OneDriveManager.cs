using Microsoft.Live;
using PintheCloud.Models;
using PintheCloud.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Storage;
using System.IO;
using Windows.System;
using System.Windows;
using PintheCloud.Pages;
using PintheCloud.Resources;
using System.Diagnostics;
using PintheCloud.ViewModels;
using PintheCloud.Helpers;
using PintheCloud.Converters;

namespace PintheCloud.Managers
{
    // Summary
    //      Implementation of IStorageManager.
    //      It helps to access OneDrive Storage.
    public class OneDriveManager : IStorageManager
    {
        #region Variables
        // Summary:
        //     Object to communicate with OneDrive.
        private const string LIVE_CLIENT_ID = "0000000044110129";
        private const string ONE_DRIVE_SIGN_IN_KEY = "ONE_DRIVE_SIGN_IN_KEY";

        private const string ONE_DRIVE_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_ico_skydrive.png";
        private const string ONE_DRIVE_COLOR_HEX_STRING = "2458A7";

        private Stack<List<FileObject>> FoldersTree = new Stack<List<FileObject>>();
        private Stack<FileObjectViewItem> FolderRootTree = new Stack<FileObjectViewItem>();

        private LiveConnectClient LiveClient = null;
        private StorageAccount CurrentAccount = null;
        private TaskCompletionSource<bool> tcs = null;
        #endregion


        public async Task<bool> SignIn()
        {
            this.tcs = new TaskCompletionSource<bool>();

            // If it haven't registerd live client, register
            LiveConnectClient liveClient = await this.GetLiveConnectClientAsync();
            if (liveClient != null)
            {
                this.LiveClient = liveClient;

                // Get id and name.
                LiveOperationResult operationResult = await this.LiveClient.GetAsync("me");
                string accountId = (string)operationResult.Result["id"];
                string accountUserName = (string)operationResult.Result["name"];

                // Register account
                StorageAccount storageAccount = App.AccountManager.GetPtcAccount().GetStorageAccountById(accountId);
                if (storageAccount == null)
                {
                    storageAccount = new StorageAccount();
                    storageAccount.Id = accountId;
                    storageAccount.StorageName = this.GetStorageName();
                    storageAccount.UserName = accountUserName;
                    storageAccount.UsedSize = 0.0;
                    await App.AccountManager.GetPtcAccount().CreateStorageAccountAsync(storageAccount);
                }

                this.CurrentAccount = storageAccount;

                // Save sign in setting.
                App.ApplicationSettings[ONE_DRIVE_SIGN_IN_KEY] = true;
                App.ApplicationSettings.Save();
                tcs.SetResult(true);
            }
            else
            {
                tcs.SetResult(false);
            }
            return tcs.Task.Result;
        }


        public bool IsSigningIn()
        {
            if (this.tcs != null)
                return !this.tcs.Task.IsCompleted && !App.ApplicationSettings.Contains(ONE_DRIVE_SIGN_IN_KEY);
            else
                return false;
        }


        public bool IsPopup()
        {
            return false;
        }


        // Remove user and record
        public void SignOut()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(LIVE_CLIENT_ID);
            liveAuthClient.Logout();
            App.ApplicationSettings.Remove(ONE_DRIVE_SIGN_IN_KEY);
            this.FoldersTree.Clear();
            this.FolderRootTree.Clear();
            this.LiveClient = null;
            this.CurrentAccount = null;
        }


        public bool IsSignIn()
        {
            return App.ApplicationSettings.Contains(ONE_DRIVE_SIGN_IN_KEY);
        }


        public string GetStorageName()
        {
            return AppResources.OneDrive;
        }


        public string GetStorageImageUri()
        {
            return ONE_DRIVE_IMAGE_URI;
        }


        public string GetStorageColorHexString()
        {
            return ONE_DRIVE_COLOR_HEX_STRING;
        }


        public Stack<FileObjectViewItem> GetFolderRootTree()
        {
            return this.FolderRootTree;
        }


        public Stack<List<FileObject>> GetFoldersTree()
        {
            return this.FoldersTree;
        }


        public StorageAccount GetStorageAccount()
        {
            return this.CurrentAccount;
        }


        // Summary:
        //     Gets Root Folder of OneDrive storage.
        //     It will be used to access the storage in the begining.
        //
        // Returns:
        //     Root Folder of OneDrive.
        public async Task<FileObject> GetRootFolderAsync()
        {
            FileObject root = ConvertToFileObjectHelper.ConvertToFileObject((await this.LiveClient.GetAsync("me/skydrive")).Result);
            root.Name = "";
            return root;
        }


        // Summary:
        //     Gets files in Root Folder of OneDrive storage.
        //
        // Returns:
        //     List of FileObject in root folder.
        public async Task<List<FileObject>> GetRootFilesAsync()
        {
            return _GetDataList((await this.LiveClient.GetAsync("me/skydrive/files")).Result);
        }


        // Summary:
        //     Gets the mete information of the file(such as id, name, size, etc.) by file id.
        //
        // Parameters:
        //  fildId:
        //      The id of the file you want the get the file meta information.
        //
        // Returns:
        //     FileObject of the certain file id.
        public async Task<FileObject> GetFileAsync(string fileId)
        {
            return ConvertToFileObjectHelper.ConvertToFileObject((await this.LiveClient.GetAsync(fileId)).Result);
        }


        // Summary:
        //     Gets list of meta information by folder id.
        //
        // Parameters:
        //  fildId:
        //      The id of the folder you want the get the list of file meta information.
        //
        // Returns:
        //     List of FileObject of the folder id.
        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            return _GetDataList((await this.LiveClient.GetAsync(folderId + "/files")).Result);
        }


        // Summary:
        //     Download a file by file id.
        //
        // Parameters:
        //  sourceFileId:
        //      The id of the file you want to download.
        //
        // Returns:
        //     The input stream to download file.
        public async Task<Stream> DownloadFileStreamAsync(string sourceFileId)
        {
            System.Threading.CancellationTokenSource ctsDownload = new System.Threading.CancellationTokenSource();
            LiveDownloadOperationResult result = null;
            try
            {
                result = await this.LiveClient.DownloadAsync(sourceFileId + "/content");
            }
            catch
            {
                throw new ShareException(sourceFileId, ShareException.ShareType.DOWNLOAD);
            }

            return result.Stream;
        }


        // Summary:
        //     Upload files by output stream.
        //
        // Parameters:
        //  sourceFolderId:
        //      The id of the place you want to upload.
        //
        //  fileName:
        //      The name you want to use after upload.
        //
        // Returns:
        //     The StorageFolder where you downloaded folder.
        public async Task<bool> UploadFileStreamAsync(string folderIdToStore, string fileName, Stream outstream)
        {
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = 0;
            var progressHandler = new Progress<LiveOperationProgress>(
                (progress) => { progressBar.Value = progress.ProgressPercentage; });

            System.Threading.CancellationTokenSource ctsUpload = new System.Threading.CancellationTokenSource();
            try
            {
                LiveOperationResult result = await this.LiveClient
                    .UploadAsync(folderIdToStore, fileName, outstream, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);
            }
            catch
            {
                ctsUpload.Cancel();
                throw new ShareException(fileName, ShareException.ShareType.UPLOAD);
            }
            return true;
        }

        // Summary:
        //     Get the file meta information from the root to the node of the file tree.
        //
        // Returns:
        //     Root FileObject of OneDrive.
        public async Task<FileObject> Synchronize()
        {
            FileObject fileObject = await GetRootFolderAsync();
            fileObject.FileList = await _GetChildAsync(fileObject);
            return fileObject;
        }

        #region Private Methods
        ///////////////////
        // Private Methods
        ///////////////////

        private async Task<LiveConnectClient> GetLiveConnectClientAsync()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(LIVE_CLIENT_ID);
            string[] scopes = new[] { "wl.basic", "wl.signin", "wl.offline_access", "wl.skydrive", "wl.skydrive_update", "wl.contacts_skydrive" };
            LiveLoginResult liveLoginResult = null;

            // Get Current live connection session
            try
            {
                liveLoginResult = await liveAuthClient.InitializeAsync(scopes);
            }
            catch (LiveAuthException)
            {
                return null;
            }

            // If session doesn't exist, get new one.
            // Otherwise, get the session.
            if (liveLoginResult.Status != LiveConnectSessionStatus.Connected)
            {
                try
                {
                    liveLoginResult = await liveAuthClient.LoginAsync(scopes);
                }
                catch (LiveAuthException)
                {
                    return null;
                }
            }

            // Get Client using session which we get above
            if (liveLoginResult.Session == null)
                return null;
            else
                return new LiveConnectClient(liveLoginResult.Session);
        }


        // Summary:
        //      List mapping method
        //
        // Returns:
        //      List of FileObject from a dictionary.
        private List<FileObject> _GetDataList(IDictionary<string, object> dic)
        {
            List<object> data = (List<object>)dic["data"];
            List<FileObject> list = new List<FileObject>();
            foreach (IDictionary<string, object> content in data)
            {
                FileObject fileObject = ConvertToFileObjectHelper.ConvertToFileObject(content);
                if (fileObject != null)
                    list.Add(fileObject);
            }
            return list;
        }


        // Summary:
        //      Gets the children of the FileObject recursively.
        //
        // Returns:
        //      Children list of given FileObject.
        private async Task<List<FileObject>> _GetChildAsync(FileObject fileObject)
        {
            if (FileObjectViewModel.FOLDER.Equals(fileObject.Type.ToString()))
            {
                List<FileObject> list = await this.GetFilesFromFolderAsync(fileObject.Id);
                foreach (FileObject file in list)
                {
                    file.FileList = await _GetChildAsync(file);
                }
                return list;
            }
            else
            {
                return null;
            }
        }


        
        #endregion

        #region Not Using Methods
        /////////////////////////////////////////////////////
        // CAUTION: NOT STABLE VERSION. THERE MIGHT BE A BUG.
        //
        // Summary:
        //     Download a folder by folder id.
        //
        // Parameters:
        //  sourceFolderId:
        //      The id of the folder you want to download.
        //
        // Returns:
        //     The StorageFolder where you downloaded folder.
        //public async Task<StorageFolder> DownloadFolderAsync(string sourceFolderId, StorageFolder folder)
        //{
        //    FileObject file = await this.GetFileAsync(sourceFolderId);
        //    file.FileList = await this._GetChildAsync(file);

        //    int index = folder.Path.IndexOf("Local");
        //    string folderUriString = ((folder.Path.Substring(index + "Local".Length, folder.Path.Length - (index + "Local".Length))));
        //    folderUriString = folderUriString.Replace("\\", "/");
        //    foreach (FileObject f in file.FileList)
        //    {
        //        if ("folder".Equals(f.Type))
        //        {
        //            StorageFolder innerFolder = await folder.CreateFolderAsync(MyEncoder.Encode(f.Name));
        //            await this.DownloadFolderAsync(f.Id, innerFolder);
        //        }
        //        else
        //        {
        //            await this.DownloadFileAsync(f.Id, new Uri(folderUriString + "/" + f.Name, UriKind.Relative));
        //        }
        //    }

        //    return folder;
        //}
        // Summary:
        //      Model mapping method
        //
        // Returns:
        //      FileObject from a dictionary.
        
        //private FileObject _GetData(IDictionary<string, object> dic)
        //{
        //    string id = (string)(dic["id"] ?? "");
        //    string name = (string)(dic["name"] ?? "");
        //    string parent_id = (string)(dic["parent_id"] ?? "/");
        //    int size = (int)dic["size"];
        //    string type = id.Split('.').First();
        //    string typeDetail = name.Split('.').Last();
        //    string createAt = (string)dic["created_time"] ?? DateTime.Now.ToString();
        //    string updateAt = (string)dic["updated_time"] ?? DateTime.Now.ToString();

        //    return new FileObject(id, name, parent_id, size, type, typeDetail, createAt, updateAt);
        //}

        // Summary:
        //     Download a file by file id.
        //
        // Parameters:
        //  sourceFileId:
        //      The id of the file you want to download.
        //
        //  destinationUri:
        //      The local destination of the downloaded file as an Uri format.
        //
        // Returns:
        //     The downloaded file.
        //public async Task<StorageFile> DownloadFileAsync(string sourceFileId, Uri destinationUri)
        //{

        //    ProgressBar progressBar = new ProgressBar();
        //    progressBar.Value = 0;
        //    var progressHandler = new Progress<LiveOperationProgress>(
        //        (progress) => { progressBar.Value = progress.ProgressPercentage; });

        //    System.Threading.CancellationTokenSource ctsDownload = new System.Threading.CancellationTokenSource();

        //    try
        //    {
        //        LiveOperationResult result = await this.LiveClient.BackgroundDownloadAsync(sourceFileId + "/content", destinationUri, ctsDownload.Token, progressHandler);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.ToString());
        //        return null;
        //    }
        //    return await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/" + destinationUri));
        //}


        // Summary:
        //     Upload files by StorageFile.
        //
        // Parameters:
        //  sourceFolderId:
        //      The id of the place you want to upload.
        //
        //  file:
        //      The file you want to upload.
        //
        // Returns:
        //     The StorageFolder where you downloaded folder.
        //public async Task<bool> UploadFileAsync(string folderIdToStore, StorageFile file)
        //{
        //    ProgressBar progressBar = new ProgressBar();
        //    progressBar.Value = 0;
        //    var progressHandler = new Progress<LiveOperationProgress>(
        //        (progress) => { progressBar.Value = progress.ProgressPercentage; });

        //    System.Threading.CancellationTokenSource ctsUpload = new System.Threading.CancellationTokenSource();
        //    try
        //    {
        //        Stream input = await file.OpenStreamForReadAsync();
        //        LiveOperationResult result = await this.LiveClient
        //            .UploadAsync(folderIdToStore, "plzdo.pdf", input, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);
        //    }
        //    catch (System.Threading.Tasks.TaskCanceledException ex)
        //    {
        //        ctsUpload.Cancel();
        //        System.Diagnostics.Debug.WriteLine("taskcancel : " + ex.ToString());
        //        return false;
        //    }
        //    catch (LiveConnectException exception)
        //    {
        //        ctsUpload.Cancel();
        //        System.Diagnostics.Debug.WriteLine("LiveConnection : " + exception.ToString());
        //        return false;
        //    }
        //    catch (Exception e)
        //    {
        //        ctsUpload.Cancel();
        //        System.Diagnostics.Debug.WriteLine("exception : " + e.ToString());
        //        return false;
        //    }
        //    return true;
        //}
        #endregion
    }
}
