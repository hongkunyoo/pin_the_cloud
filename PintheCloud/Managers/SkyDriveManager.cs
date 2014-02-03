using Microsoft.Live;
using PintheCloud.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public class SkyDriveManager
    {
        private LiveConnectClient LiveClient;
        private LiveAuthClient LiveAuth;
        private LiveLoginResult LiveResult;
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

        private FileObject getData(IDictionary<string, object> dic)
        {
            string id = (string)(dic["id"] ?? "");   
            string name = (string)(dic["name"] ?? "");
            string parent_id = (string)(dic["parent_id"] ?? "");
            int size = (int)dic["size"];
            string type = (string)dic["type"] ?? "";
            string createAt = (string)dic["created_time"] ?? DateTime.Now.ToString();
            string updateAt = (string)dic["updated_time"] ?? DateTime.Now.ToString();

            return new FileObject(id, name, parent_id, size, type, createAt, updateAt);
            
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
            return getData((await this.LiveClient.GetAsync("me/skydrive")).Result);
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

        private async Task<List<FileObject>> GetChild(FileObject fo)
        {
            if ("folder".Equals(fo.Type))
            {
                List<FileObject> list = await this.GetFilesFromFolderAsync(fo.Id);
                
                foreach (FileObject file in list)
                {
                    file.FileList = await GetChild(file);
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
            fo.FileList = await GetChild(fo);
            return fo;
        }

        public async Task<bool> Download(string sourceFileId, Uri destinationUri)
        {
            
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = 0; 
            var progressHandler = new Progress<LiveOperationProgress>(
                (progress) => { progressBar.Value = progress.ProgressPercentage; });
            
            System.Threading.CancellationTokenSource ctsDownload = new System.Threading.CancellationTokenSource();

            try
            {
                await this.LiveClient.BackgroundDownloadAsync(sourceFileId + "/content", destinationUri, ctsDownload.Token, progressHandler);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
    }
}
