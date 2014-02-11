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

            /*
            this.LiveClient.BackgroundDownloadProgressChanged +=
                new EventHandler<LiveDownloadProgressChangedEventArgs>(OnBackgroundDownloadProgressChanged);
            this.LiveClient.BackgroundDownloadCompleted +=
                new EventHandler<LiveOperationCompletedEventArgs>(OnBackgroundDownloadCompleted);
            this.LiveClient.BackgroundUploadProgressChanged +=
                new EventHandler<LiveUploadProgressChangedEventArgs>(OnBackgroundUploadProgressChanged);
            this.LiveClient.BackgroundUploadCompleted +=
                new EventHandler<LiveOperationCompletedEventArgs>(OnBackgroundUploadCompleted);
            this.LiveClient.AttachPendingTransfers();
            */
        }

        private FileObject getData(IDictionary<string, object> dic)
        {
            string id = (string)(dic["id"] ?? "");   
            string name = (string)(dic["name"] ?? "");
            string parent_id = (string)(dic["parent_id"] ?? "/");
            int size = (int)dic["size"];
            string type = (string)dic["type"] ?? "";
            string createAt = (string)dic["created_time"] ?? DateTime.Now.ToString();
            string updateAt = (string)dic["updated_time"] ?? DateTime.Now.ToString();

            return new FileObject(id, name, parent_id, size, id.Substring(0,id.IndexOf(".")),type, createAt, updateAt);
            
        }

        private List<FileObject> getDataList(IDictionary<string, object> dic)
        {
            List<object> data = (List<object>)dic["data"];
            List<FileObject> list = new List<FileObject>();
            foreach (IDictionary<string, object> content in data)
            {
                list.Add(this.getData(content));
            }
            return list;
        }

        public async Task<FileObject> GetRootFolderAsync(){
            FileObject root = getData((await this.LiveClient.GetAsync("me/skydrive")).Result);
            root.Name = "";
            return root;
        }

        public async Task<List<FileObject>> GetRootFilesAsync()
        {
            return getDataList((await this.LiveClient.GetAsync("me/skydrive/files")).Result);
        }

        public async Task<FileObject> GetFileAsync(string fileId)
        {
            return getData((await this.LiveClient.GetAsync(fileId)).Result);
        }

        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            return getDataList((await this.LiveClient.GetAsync(folderId + "/files")).Result);
        }

        private async Task<List<FileObject>> GetChildAsync(FileObject fo)
        {
            if ("folder".Equals(fo.Type))
            {
                List<FileObject> list = await this.GetFilesFromFolderAsync(fo.Id);
                
                foreach (FileObject file in list)
                {
                    file.FileList = await GetChildAsync(file);
                }
                return list;
            }
            else
            {
                return null;
            }
            
        }

        public async Task<FileObject> Synchronize()
        {
            FileObject fo = await GetRootFolderAsync();
            fo.FileList = await GetChildAsync(fo);
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
                //destinationUri = new Uri(MyEncoder.Encode(destinationUri.ToString()), UriKind.Relative);
                LiveOperationResult result = await this.LiveClient.BackgroundDownloadAsync(sourceFileId + "/content", destinationUri, ctsDownload.Token, progressHandler);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }
            return await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/"+destinationUri));
        }
       
        public async Task<StorageFolder> DownloadFolderAsync(string sourceFolder, StorageFolder folder)
        {
            FileObject file = await this.GetFileAsync(sourceFolder);
            file.FileList = await this.GetChildAsync(file);

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

                //folderIdToStore = "folder.b96a113f78eb1c6f.B96A113F78EB1C6F!1856";
                //uploadFile = new Uri("",UriKind.Relative);
                //await this.LiveClient.BackgroundUploadAsync("folder.8c8ce076ca27823f.8C8CE076CA27823F!134",
                //    "MyUploadedPicture.jpg", file, true, ctsUpload.Token, progressHandler);
                MyDebug.WriteLine("start");
                //LiveOperationResult result = await this.LiveClient.BackgroundUploadAsync(folderIdToStore, uploadFile, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);
                Stream input = await file.OpenStreamForReadAsync();
                LiveOperationResult result = await this.LiveClient.UploadAsync(folderIdToStore, "plzdo.pdf", input, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);
                MyDebug.WriteLine("Done");
                IEnumerator<KeyValuePair<string,object>> e = result.Result.GetEnumerator();


                //await this.LiveClient.UploadAsync("path", "fileName", input, OverwriteOption.Overwrite, ctsUpload.Token, progressHandler);
                

                while (e.MoveNext())
                {
                    MyDebug.WriteLine(e.Current.Key);
                    MyDebug.WriteLine(e.Current.Value);
                }
                
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                //this.infoTextBlock.Text = "Upload cancelled.";
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("taskcancel : "+ex.ToString());

            }
            catch (LiveConnectException exception)
            {
                //this.infoTextBlock.Text = "Error uploading file: " + exception.Message;
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("LiveConnection : "+exception.ToString());
            }
            catch (Exception e)
            {
                ctsUpload.Cancel();
                System.Diagnostics.Debug.WriteLine("exception : " + e.ToString());
            }


        }
    }
}
