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

namespace PintheCloud.Managers
{
    public class SkyDriveManager
    {
        private LiveConnectClient LiveClient;
        //private LiveAuthClient LiveAuth;
        //private LiveLoginResult LiveResult;
        private LiveConnectSession LiveSession;
        private string ClientId;
        private string[] Scopes;

        public SkyDriveManager(LiveConnectSession Session)
        {
            this.ClientId = GlobalKeys.AZURE_CLIENT_ID;
            this.LiveSession = Session;
            this.Scopes = new string[] { "wl.basic", "wl.skydrive", "wl.offline_access", "wl.signin", "wl.skydrive_update",
                                                "wl.contacts_skydrive"};
            this.LiveClient = new LiveConnectClient(this.LiveSession);

        }
        public async Task<FileObject> GetRootFolderAsync(){
            FileObject root = _GetData((await this.LiveClient.GetAsync("me/skydrive")).Result);
            root.Name = "";
            return root;
        }
        public async Task<List<FileObject>> GetRootFilesAsync()
        {
            return _GetDataList((await this.LiveClient.GetAsync("me/skydrive/files")).Result);
        }
        public async Task<FileObject> GetFileAsync(string fileId)
        {
            return _GetData((await this.LiveClient.GetAsync(fileId)).Result);
        }
        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            return _GetDataList((await this.LiveClient.GetAsync(folderId + "/files")).Result);
        }
        public async Task<FileObject> Synchronize()
        {
            FileObject fo = await GetRootFolderAsync();
            fo.FileList = await _GetChildAsync(fo);
            return fo;
        }
        
        public async Task<StorageFile> DownloadFileAsync(string sourceFileId, Uri destinationUri)
        {
            
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = 0; 
            var progressHandler = new Progress<LiveOperationProgress>(
                (progress) => { progressBar.Value = progress.ProgressPercentage; });
            
            System.Threading.CancellationTokenSource ctsDownload = new System.Threading.CancellationTokenSource();
            
            try
            {
                LiveOperationResult result = await this.LiveClient.BackgroundDownloadAsync(sourceFileId + "/content", destinationUri, ctsDownload.Token, progressHandler);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
            return await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/"+destinationUri));
        }

        public async Task<Stream> DownloadFileThroughStreamAsync(string sourceFileId)
        {

            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = 0;
            var progressHandler = new Progress<LiveOperationProgress>(
                (progress) => { progressBar.Value = progress.ProgressPercentage; });

            System.Threading.CancellationTokenSource ctsDownload = new System.Threading.CancellationTokenSource();

            try
            {
                LiveDownloadOperationResult result = await this.LiveClient.DownloadAsync(sourceFileId + "/content");
             
                return result.Stream;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
        }
       
        public async Task<StorageFolder> DownloadFolderAsync(string sourceFolder, StorageFolder folder)
        {
            FileObject file = await this.GetFileAsync(sourceFolder);
            file.FileList = await this._GetChildAsync(file);

            int index = folder.Path.IndexOf("Local");
            string folderUriString = ((folder.Path.Substring(index + "Local".Length, folder.Path.Length - (index + "Local".Length))));
            folderUriString = folderUriString.Replace("\\", "/");
            foreach(FileObject f in file.FileList)
            {
                if ("folder".Equals(f.Type))
                {
                    StorageFolder innerFolder = await folder.CreateFolderAsync(MyEncoder.Encode(f.Name));
                    await this.DownloadFolderAsync(f.Id, innerFolder);
                }
                else
                {
                    await this.DownloadFileAsync(f.Id, new Uri(folderUriString + "/" + f.Name, UriKind.Relative));
                }
            }

            return folder;
        }

        public async Task UploadFileAsync(string folderIdToStore, StorageFile file)
        {
            System.Threading.CancellationTokenSource ctsUpload = new System.Threading.CancellationTokenSource();
            try
            {
                ProgressBar progressBar = new ProgressBar();
                progressBar.Value = 0;
                var progressHandler = new Progress<LiveOperationProgress>(
                    (progress) => { progressBar.Value = progress.ProgressPercentage; });
                
                progressBar.Value = 0;

                Stream input = await file.OpenStreamForReadAsync();
                LiveOperationResult result = await this.LiveClient.UploadAsync(folderIdToStore, "plzdo.pdf", input, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);
                
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("taskcancel : "+ex.ToString());

            }
            catch (LiveConnectException exception)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("LiveConnection : "+exception.ToString());
            }
            catch (Exception e)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("exception : " + e.ToString());
            }
        }

        public async Task UploadFileThroughStreamAsync(string folderIdToStore, string fileName, Stream outstream)
        {
            System.Threading.CancellationTokenSource ctsUpload = new System.Threading.CancellationTokenSource();
            try
            {
                ProgressBar progressBar = new ProgressBar();
                progressBar.Value = 0;
                var progressHandler = new Progress<LiveOperationProgress>(
                    (progress) => { progressBar.Value = progress.ProgressPercentage; });
                progressBar.Value = 0;

                LiveOperationResult result = await this.LiveClient.UploadAsync(folderIdToStore, fileName, outstream, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);

            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("taskcancel : " + ex.ToString());

            }
            catch (LiveConnectException exception)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("LiveConnection : " + exception.ToString());
            }
            catch (Exception e)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("exception : " + e.ToString());
            }
        }

        private FileObject _GetData(IDictionary<string, object> dic)
        {
            string id = (string)(dic["id"] ?? "");
            string name = (string)(dic["name"] ?? "");
            string parent_id = (string)(dic["parent_id"] ?? "/");
            int size = (int)dic["size"];
            string type = (string)dic["type"] ?? "";
            string createAt = (string)dic["created_time"] ?? DateTime.Now.ToString();
            string updateAt = (string)dic["updated_time"] ?? DateTime.Now.ToString();

            return new FileObject(id, name, parent_id, size, id.Substring(0, id.IndexOf(".")), type, createAt, updateAt);

        }

        private List<FileObject> _GetDataList(IDictionary<string, object> dic)
        {
            List<object> data = (List<object>)dic["data"];
            List<FileObject> list = new List<FileObject>();
            foreach (IDictionary<string, object> content in data)
            {
                list.Add(this._GetData(content));
            }
            return list;
        }
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
    }
}
