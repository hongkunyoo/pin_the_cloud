using DropNet;
using DropNet.Models;
using PintheCloud.Models;
using PintheCloud.Resources;
using PintheCloud.Utilities;
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
        private const string APP_KEY = "gxjfureco8noyle";
        private const string APP_SECRET = "iskekcebjky6vbm";
        private const string APP_AUTH_URI = "http://54.214.19.198";
        private const string DROPBOX_USER_KEY = "DROPBOX_USER_KEY";

        private DropNetClient _client = null;
        private Account CurrentAccount = null;
        #endregion

        private Task<Account> GetMyAccountAsync()
        {
            TaskCompletionSource<Account> tcs = new TaskCompletionSource<Account>();
            this._client.AccountInfoAsync((info) => {
                tcs.SetResult(new Account(info.uid.ToString(),Account.StorageAccountType.DROPBOX,info.display_name,0.0,AccountType.NORMAL_ACCOUNT_TYPE));
            }, (fail) => {
                tcs.SetException(new Exception("Account Info Get Failed"));
            });

            return tcs.Task;
        }

        public async Task SignIn()
        {
            // Add application settings before work for good UX
            App.ApplicationSettings[DROPBOX_USER_KEY] = null;
            App.ApplicationSettings.Save();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            _client = new DropNetClient(APP_KEY, APP_SECRET);

            UserLogin dropboxUser = null;
            App.ApplicationSettings.TryGetValue<UserLogin>(DROPBOX_USER_KEY, out dropboxUser);
            if (dropboxUser == null)
            {
                _client.GetTokenAsync(async (userLogin) =>
                {
                    string authUri = _client.BuildAuthorizeUrl(APP_AUTH_URI);
                    DropboxWebBrowserTask webBrowser = new DropboxWebBrowserTask(authUri);
                    await webBrowser.ShowAsync();

                    _client.GetAccessTokenAsync(async (accessToken) =>
                    {
                        UserLogin user = new UserLogin();
                        user.Token = accessToken.Token;
                        user.Secret = accessToken.Secret;
                        _client.UserLogin = user;

                        App.ApplicationSettings[DROPBOX_USER_KEY] = user;
                        App.ApplicationSettings.Save();

                        tcs.SetResult(true);
                        this.CurrentAccount = await GetMyAccountAsync();
                    },
                    (error) =>
                    {
                        this.SignOut();
                        tcs.SetResult(false);
                    });
                },
                (error) =>
                {
                    this.SignOut();
                    tcs.SetResult(false);
                });
            }
            else
            {
                _client.UserLogin = dropboxUser;
                this.CurrentAccount = await GetMyAccountAsync();
                tcs.SetResult(true);
            }
            await tcs.Task;
        }


        public void SignOut()
        {
            // Remove user record
            App.ApplicationSettings.Remove(DROPBOX_USER_KEY);
            App.ApplicationSettings.Save();

            // Set null account
            this._client = null;
            this.CurrentAccount = null;
        }


        public bool IsSignIn()
        {
            return App.ApplicationSettings.Contains(DROPBOX_USER_KEY);
        }


        public string GetStorageName()
        {
            return AppResources.Dropbox;
        }


        public Account GetAccount()
        {
            return this.CurrentAccount;
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
            MetaData metaTask = await _client.GetMetaDataTask(fileId);
            return FileObjectConverter.ConvertToFileObject(metaTask);
        }
        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            MetaData metaTask = await _client.GetMetaDataTask(folderId);
            List<FileObject> list = new List<FileObject>();

            if (metaTask.Contents == null) return list;

            foreach (MetaData m in metaTask.Contents)
            {
                list.Add(FileObjectConverter.ConvertToFileObject(m));
            }

            return list;
        }
        public Task<Stream> DownloadFileStreamAsync(string sourceFileId)
        {
            TaskCompletionSource<Stream> tcs = new TaskCompletionSource<Stream>();
            _client.GetFileAsync(sourceFileId, new Action<IRestResponse>((response) =>
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
                MetaData d = await _client.UploadFileTask(folderIdToStore, fileName, outstream);
                return true;
            }
            catch
            {
                throw new ShareException(fileName, ShareException.ShareType.UPLOAD);
            }
        }

        #region Private Methods
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

        #region Not Using Methods
        //public FileObject ConvertToFileObject(MetaData metaTask)
        //{
        //    FileObject file = new FileObject();

        //    file.Name = metaTask.Name;
        //    file.CreateAt = metaTask.ModifiedDate.ToString(); //14/02/2014 15:48:13
        //    file.UpdateAt = metaTask.ModifiedDate.ToString(); //14/02/2014 15:48:13
        //    file.Id = metaTask.Path; // Full path
        //    file.ParentId = metaTask.Path;
        //    //file.Size = Convert.ToInt32(metaTask.Size);
        //    //file.Size = metaTask.bytes <- int
        //    //file.Size = metaTask.Size <- string
        //    file.SizeUnit = "GB";
        //    file.ThumnailType = "";
        //    file.Type = ""; // Is_Dir = true | false
        //    file.TypeDetail = metaTask.Extension; // .png
        //    file.UpdateAt = metaTask.ModifiedDate.ToString();

        //    return file;
        //}


        //public async Task<StorageFile> DownloadFileAsync(string sourceFileId, StorageFile targetFile)
        //{
        //    TaskCompletionSource<StorageFile> tcs = new TaskCompletionSource<StorageFile>();
        //    Stream output = await targetFile.OpenStreamForWriteAsync();

        //    _client.GetFileAsync(sourceFileId, new Action<IRestResponse>((response) =>
        //    {
        //        MemoryStream input = new MemoryStream(response.RawBytes);
        //        this._Streaming(input, output);
        //        tcs.SetResult(targetFile);
        //    }), new Action<DropNet.Exceptions.DropboxException>((ex) =>
        //    {
        //        tcs.SetException(new Exception("failed"));
        //    }));

        //    return await tcs.Task;
        //}

        //public async Task<bool> UploadFileAsync(string folderIdToStore, StorageFile file)
        //{
        //    try
        //    {
        //        MetaData d = await _client.UploadFileTask(folderIdToStore, file.Name, await file.OpenStreamForReadAsync());
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        #endregion
    }
}
