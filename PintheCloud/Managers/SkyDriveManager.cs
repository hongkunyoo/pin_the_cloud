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

        private FileObject getData(IDictionary<string, object> dic)
        {
            string id = (string)(dic["id"] ?? "");   
            string name = (string)(dic["name"] ?? "");
            string parent_id = (string)(dic["parent_id"] ?? "");
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
                /*
                string name;
                string[] path = ParseHelper.Parse(destinationUri.ToString(),ParseHelper.Mode.FULL_PATH,out name);
                string[] _name = ParseHelper.SplitName(name);
                string newUriString = "";
                foreach (string s in path)
                {
                    newUriString += "/" + s;
                }
                newUriString += "/" + MyEncoder.Encode(_name[0]);
                newUriString += "." + _name[1];
                Uri newUri = new Uri(newUriString, UriKind.Relative);
                */
                string ss = destinationUri.ToString();
                ss = System.Uri.EscapeUriString(ss).Replace("%", ";");

                destinationUri = new Uri(ss, UriKind.Relative);
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
    }
}
