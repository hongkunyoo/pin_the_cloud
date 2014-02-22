﻿using Microsoft.Live;
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
using PintheCloud.Workers;
using System.Windows;
using PintheCloud.Pages;

namespace PintheCloud.Managers
{
    // Summary
    //      Implementation of IStorageManager.
    //      It helps to access SkyDrive Storage.
    public class SkyDriveManager : IStorageManager
    {
        #region Variables
        // Summary:
        //     Object to communicate with SkyDrive.
        private const string ACCOUNT_IS_SIGN_IN_KEY = "ACCOUNT_SKY_DRIVE_SIGN_IN_KEY";
        public const string ACCOUNT_ID_KEY = "ACCOUNT_SKY_DRIVE_ID_KEY";
        public const string ACCOUNT_USED_SIZE_KEY = "ACCOUNT_SKY_DRIVE_USED_SIZE_KEY";
        public const string ACCOUNT_BUSINESS_TYPE_KEY = "ACCOUNT_SKY_DRIVE_BUSINESS_TYPE_KEY";

        private SkyDriveAccountWorker AccountWorker = new SkyDriveAccountWorker();
        private LiveConnectClient LiveClient = null;
        private Account CurrentAccount = null;
        #endregion


        public async Task SignIn()
        {
            // Add application settings before work for good UX
            App.ApplicationSettings[ACCOUNT_IS_SIGN_IN_KEY] = true;
            App.ApplicationSettings.Save();

            // If it haven't registerd live client, register
            if (this.LiveClient == null)
            {
                // If it success to register live connect session,
                LiveConnectClient liveClient = await this.AccountWorker.GetLiveConnectClientAsync();
                if (liveClient != null)
                {
                    this.LiveClient = liveClient;
                }
                else
                {
                    this.SignOut();
                    return;
                }
            }


            // Get profile result and Login.
            // If login succeed, Move to explorer page.
            // Otherwise, Hide indicator, Show login fail message box.
            dynamic profileResult = await this.AccountWorker.GetProfileResultAsync(this.LiveClient);
            if (profileResult != null)
            {
                Account account = await this.AccountWorker.SkyDriveSignInAsync(this.LiveClient, profileResult);
                if (account != null)
                    this.CurrentAccount = account;
                else
                    this.SignOut();
            }
            else
            {
                this.SignOut();
            }
        }


        public void SignOut()
        {
            // Remove user record
            LiveAuthClient liveAuthClient = new LiveAuthClient(App.AZURE_CLIENT_ID);
            liveAuthClient.Logout();

            // Remove user is signed in record
            App.ApplicationSettings.Remove(ACCOUNT_IS_SIGN_IN_KEY);
            App.ApplicationSettings.Remove(ACCOUNT_USED_SIZE_KEY);
            App.ApplicationSettings.Remove(ACCOUNT_BUSINESS_TYPE_KEY);

            // Set null account
            this.LiveClient = null;
            this.CurrentAccount = null;
        }


        public Account GetAccount()
        {
            return this.CurrentAccount;
        }


        public string GetAccountIsSignInKey()
        {
            return ACCOUNT_IS_SIGN_IN_KEY;
        }


        public string GetAccountIdKey()
        {
            return ACCOUNT_ID_KEY;
        }

        // Summary:
        //     Gets Root Folder of SkyDrive storage.
        //     It will be used to access the storage in the begining.
        //
        // Returns:
        //     Root Folder of SkyDrive.
        public async Task<FileObject> GetRootFolderAsync()
        {
            FileObject root = FileObjectConverter.ConvertToFileObject((await this.LiveClient.GetAsync("me/skydrive")).Result);
            root.Name = "";
            return root;
        }
        // Summary:
        //     Gets files in Root Folder of SkyDrive storage.
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
            return FileObjectConverter.ConvertToFileObject((await this.LiveClient.GetAsync(fileId)).Result);
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


        #region Private Methods
        ///////////////////
        // Private Methods
        ///////////////////

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
                list.Add(FileObjectConverter.ConvertToFileObject(content));
            }
            return list;
        }
        // Summary:
        //      Gets the children of the FileObject recursively.
        //
        // Returns:
        //      Children list of given FileObject.
        private async Task<List<FileObject>> _GetChildAsync(FileObject fo)
        {
            if ("folder".Equals(fo.Type))
            {
                List<FileObject> list = await this.GetFilesFromFolderAsync(fo.Id);

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
        // Summary:
        //     Get the file meta information from the root to the node of the file tree.
        //
        // Returns:
        //     Root FileObject of SkyDrive.
        private async Task<FileObject> Synchronize()
        {
            FileObject fo = await GetRootFolderAsync();
            fo.FileList = await _GetChildAsync(fo);
            return fo;
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
