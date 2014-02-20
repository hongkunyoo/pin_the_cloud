using DropNet;
using DropNet.Models;
using PintheCloud.Models;
using PintheCloud.Utilities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public class DropBoxManager
    {
        #region Variables
        private DropNetClient _client;
        private string APP_KEY = "gxjfureco8noyle";
        private string APP_SECRET = "iskekcebjky6vbm";
        private string DROP_BOX_USER = "DROP_BOX_USER";
        private Account CurrentAccount;
        #endregion

        public Task<bool> SignIn()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            _client = new DropNetClient(APP_KEY, APP_SECRET);

            if (App.ApplicationSettings.Contains(DROP_BOX_USER))
            {
                _client.GetTokenAsync(async (userLogin) =>
                {

                    var url = _client.BuildAuthorizeUrl("http://54.214.19.198");
                    MyWebBrowserTask webBrowser = new MyWebBrowserTask(url);
                    await webBrowser.ShowAsync();

                    _client.GetAccessTokenAsync((accessToken) =>
                    {
                        UserLogin user = new UserLogin();
                        user.Token = accessToken.Token;
                        user.Secret = accessToken.Secret;
                        App.ApplicationSettings[DROP_BOX_USER] = user;
                        App.ApplicationSettings.Save();
                        this.CurrentAccount = new Account();
                        this.CurrentAccount.RawAccount = user;
                        this.CurrentAccount.AccountType = Account.StorageAccountType.DROP_BOX;
                        _client.UserLogin = user;
                        tcs.SetResult(true);
                    },
                    (error) =>
                    {
                        tcs.SetResult(false);
                    });
                },
                (error) =>
                {
                    tcs.SetResult(false);
                });
            }
            else
            {
                _client.UserLogin = (UserLogin)App.ApplicationSettings[DROP_BOX_USER];
                tcs.SetResult(true);
            }
            return tcs.Task;
        }
        public void SignOut()
        {
            App.ApplicationSettings.Remove(DROP_BOX_USER);
            this.CurrentAccount = null;
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
                return false;
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
