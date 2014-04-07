using DropNet;
using DropNet.Models;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Converters;
using PintheCloud.Exceptions;
using PintheCloud.Helpers;
using PintheCloud.Models;
using PintheCloud.Pages;
using PintheCloud.Resources;
using PintheCloud.Utilities;
using PintheCloud.ViewModels;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public class DropboxManager : IStorageManager
    {
        #region Variables
        private const string DROPBOX_CLIENT_KEY = "gxjfureco8noyle";
        private const string DROPBOX_CLIENT_SECRET = "iskekcebjky6vbm";
        public const string DROPBOX_AUTH_URI = "http://www.pinthecloud.com/index.html";

        private const string DROPBOX_USER_KEY = "DROPBOX_USER_KEY";
        private const string DROPBOX_SIGN_IN_KEY = "DROPBOX_SIGN_IN_KEY";

        private const string DROPBOX_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_ico_dropbox.png";
        private const string DROPBOX_COLOR_HEX_STRING = "26A4DD";

        private Stack<List<FileObject>> FoldersTree = new Stack<List<FileObject>>();
        private Stack<FileObjectViewItem> FolderRootTree = new Stack<FileObjectViewItem>();

        private DropNetClient _client = null;
        private StorageAccount CurrentAccount = null;
        private TaskCompletionSource<bool> tcs = null;
        #endregion


        private Task<StorageAccount> GetMyAccountAsync()
        {
            TaskCompletionSource<StorageAccount> tcs = new TaskCompletionSource<StorageAccount>();
            this._client.AccountInfoAsync((info) => {
                tcs.SetResult(new StorageAccount(info.uid.ToString(), StorageAccount.StorageAccountType.DROPBOX, info.display_name, 0.0));
            }, (fail) => {
                tcs.SetException(new Exception("Dropbox Account Info Get Failed"));
            });
            return tcs.Task;
        }


        public Task<bool> SignIn()
        {
            // Get dropbox _client.
            this.tcs = new TaskCompletionSource<bool>();
            this._client = new DropNetClient(DROPBOX_CLIENT_KEY, DROPBOX_CLIENT_SECRET);

            // If dropbox user exists, get it.
            // Otherwise, get from user.
            UserLogin dropboxUser = null;
            if (App.ApplicationSettings.Contains(DROPBOX_USER_KEY))
                dropboxUser = (UserLogin)App.ApplicationSettings[DROPBOX_USER_KEY];


            if (dropboxUser != null)
            {
                this._client.UserLogin = dropboxUser;
                this._client.AccountInfoAsync((info) =>
                {
                    this.CurrentAccount = new StorageAccount(info.uid.ToString(), StorageAccount.StorageAccountType.DROPBOX, info.display_name, 0.0);
                    TaskHelper.AddTask(TaskHelper.STORAGE_EXPLORER_SYNC + this.GetStorageName(), StorageExplorer.Synchronize(this.GetStorageName()));
                    tcs.SetResult(true);                
                }, (fail) =>
                {
                    this.CurrentAccount = null;
                    tcs.SetResult(false);
                });
            }
            else
            {
                this._client.GetTokenAsync(async (userLogin) =>
                {
                    string authUri = this._client.BuildAuthorizeUrl(DROPBOX_AUTH_URI);
                    DropboxWebBrowserTask webBrowser = new DropboxWebBrowserTask(authUri);
                    await webBrowser.ShowAsync();

                    this._client.GetAccessTokenAsync(async (accessToken) =>
                    {
                        UserLogin user = new UserLogin();
                        user.Token = accessToken.Token;
                        user.Secret = accessToken.Secret;
                        this._client.UserLogin = user;

                        // Save dropbox user got and sign in setting.
                        try
                        {
                            this.CurrentAccount = await this.GetMyAccountAsync();

                            StorageAccount account = await App.AccountManager.GetStorageAccountAsync(this.CurrentAccount.Id);
                            if (account == null)
                                await App.AccountManager.CreateStorageAccountAsync(this.CurrentAccount);

                            App.ApplicationSettings[DROPBOX_SIGN_IN_KEY] = true;
                            App.ApplicationSettings[DROPBOX_USER_KEY] = user;
                            App.ApplicationSettings.Save();
                            TaskHelper.AddTask(TaskHelper.STORAGE_EXPLORER_SYNC + this.GetStorageName(), StorageExplorer.Synchronize(this.GetStorageName()));
                            tcs.SetResult(true);
                        }
                        catch
                        {
                            tcs.SetResult(false);
                        }
                    },
                    (error) =>
                    {
                        Debug.WriteLine(error.ToString());
                        tcs.SetResult(false);
                    });
                },
               (error) =>
               {
                   tcs.SetResult(false);
               });
            }
            return tcs.Task;
        }


        public bool IsSigningIn()
        {
            if (this.tcs != null)
                return !this.tcs.Task.IsCompleted && !App.ApplicationSettings.Contains(DROPBOX_SIGN_IN_KEY);
            else
                return false;
        }


        public bool IsPopup()
        {
            return true;
        }


        // Remove user and record
        public void SignOut()
        {
            App.ApplicationSettings.Remove(DROPBOX_USER_KEY);
            App.ApplicationSettings.Remove(DROPBOX_SIGN_IN_KEY);
            this.FoldersTree.Clear();
            this.FolderRootTree.Clear();
            this._client = null;
            this.CurrentAccount = null;
        }


        public bool IsSignIn()
        {
            return App.ApplicationSettings.Contains(DROPBOX_SIGN_IN_KEY);
        }


        public string GetStorageName()
        {
            return AppResources.Dropbox;
        }


        public string GetStorageImageUri()
        {
            return DROPBOX_IMAGE_URI;
        }


        public string GetStorageColorHexString()
        {
            return DROPBOX_COLOR_HEX_STRING;
        }


        public async Task<StorageAccount> GetStorageAccountAsync()
        {
            TaskCompletionSource<StorageAccount> tcs = new TaskCompletionSource<StorageAccount>();
            if (this.CurrentAccount == null)
                await TaskHelper.WaitSignInTask(this.GetStorageName());
            tcs.SetResult(this.CurrentAccount);
            return tcs.Task.Result;
        }


        public Task<FileObject> GetRootFolderAsync()
        {
            return this.GetFileAsync("/");
        }
        public Task<List<FileObject>> GetRootFilesAsync()
        {
            return this.GetFilesFromFolderAsync("/");
        }
        public async Task<FileObject> GetFileAsync(string fileId)
        {
            MetaData metaTask = await this._client.GetMetaDataTask(fileId);
            return ConvertToFileObjectHelper.ConvertToFileObject(metaTask);
        }
        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            MetaData metaTask = await this._client.GetMetaDataTask(folderId);
            List<FileObject> list = new List<FileObject>();

            if (metaTask.Contents == null) return list;

            foreach (MetaData m in metaTask.Contents)
            {
                list.Add(ConvertToFileObjectHelper.ConvertToFileObject(m));
            }

            return list;
        }
        public Task<Stream> DownloadFileStreamAsync(string sourceFileId)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();
            this._client.GetFileAsync(sourceFileId, new Action<IRestResponse>((response) =>
            {
                MemoryStream stream = new MemoryStream(response.RawBytes);
                tcs.SetResult(stream);
            }), new Action<DropNet.Exceptions.DropboxException>((ex) =>
            {
                tcs.SetException(new Exception("failed"));
                throw new ShareException(sourceFileId, ShareException.ShareType.DOWNLOAD);
            }));

            return tcs.Task;
        }
        public async Task<bool> UploadFileStreamAsync(string folderIdToStore, string fileName, Stream outstream)
        {
            try
            {
                //MetaData d = await _client.UploadFileTask(folderIdToStore, fileName, CreateStream(outstream).ToArray());
                MetaData d = await this._client.UploadFileTask(folderIdToStore, fileName, outstream);
                return true;
            }
            catch
            {
                throw new ShareException(fileName, ShareException.ShareType.UPLOAD);
            }
        }

        public async Task<FileObject> Synchronize()
        {
            FileObject fileObject = await GetRootFolderAsync();
            fileObject.FileList = await _GetChildAsync(fileObject);
            return fileObject;
        }

        #region Private Methods

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

        private bool _Streaming(Stream input, Stream output)
        {
            byte[] buffer = new byte[1024000];
            int count = 0;
            try
            {
                while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
                {
                    output.Write(buffer, 0, count);
                }
                input.Close();
                output.Close();
                return true;
            }
            catch
            {
                throw new Exception("Create Streaming Failed");
            }
        }
        #endregion
    }
}
