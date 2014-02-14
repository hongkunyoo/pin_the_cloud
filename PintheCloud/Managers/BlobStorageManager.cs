using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using PintheCloud.Models;
using PintheCloud.Utilities;
using Windows.System;

namespace PintheCloud.Managers
{
    public class BlobStorageManager
    {
        //private static string BLOB_CONNECTION = "DefaultEndpointsProtocol=http;AccountName=rfrost77;AccountKey=0GgFw4h4JiCklbD5avI2YBitfyuOj9GZQQpRaMEDZEhRtPohzlsUlAOie3SfZE3aMXFCcto1hi/Gu1Ppw4ZuRg==";
        private string BLOB_CONNECTION = "DefaultEndpointsProtocol=http;AccountName=pinthecloud;AccountKey=BjLf1uc1g7Ll1rTlZJ4GR/gsAjmLx/7oHlQNnsd1olxrmHpNtDc91+cEXbMo1HyCDF+O//uc1V8uS1YK02Ad6Q==";
        private string CONTAINER_NAME = "space-container";
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer container;
        //private string account;

        public BlobStorageManager()
        {
            this.storageAccount = CloudStorageAccount.Parse(this.BLOB_CONNECTION);
            this.blobClient = this.storageAccount.CreateCloudBlobClient();
            this.container = blobClient.GetContainerReference(this.CONTAINER_NAME);
        }
        
        public async Task<FileObject> GetFileAsync(string id)
        {
            CloudBlockBlob blockBlob = (CloudBlockBlob)await container.GetBlobReferenceFromServerAsync(id);
            return this._GetFileObjectFromBlob(blockBlob);
        }
        public async Task<FileObject> GetFileAsync(string account, string spaceId, string sourcePath)
        {
            sourcePath = ParseHelper.TrimSlash(sourcePath);
            return await this.GetFileAsync(account + "/" + spaceId + "/" + sourcePath);
        }
        
        public Task<List<FileObject>> GetFilesFromFolderAsync(string account, string spaceId, string sourcePath)
        {
            sourcePath = ParseHelper.TrimSlash(sourcePath);
            return this._GetFilesFromFolderByIdAsync(account + "/" + spaceId + "/" + sourcePath);
        }
        public Task<List<FileObject>> GetFilesFromFolderAsync(string account, string spaceId)
        {
            return this._GetFilesFromFolderByIdAsync(account + "/" + spaceId);
        }
        public async Task<StorageFile> DownloadFileAsync(string id, StorageFile downloadFile)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(id);
            using (Stream s = await downloadFile.OpenStreamForWriteAsync())
            {
                await blockBlob.DownloadToStreamAsync(s);
            }
            return downloadFile;
        }
        public async Task<StorageFile> DownloadFileAsync(string account, string spaceId, string sourcePath, StorageFile downloadFile)
        {
            sourcePath = ParseHelper.TrimSlash(sourcePath);
            return await this.DownloadFileAsync(account + "/" + spaceId + "/" + sourcePath, downloadFile);
        }

        public async Task<Stream> DownloadFileThroughStreamAsync(string id)
        {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(id);
            return await blockBlob.OpenReadAsync();
        }
        /*
        private async Task<string> _UploadFileThroughStreamAsync(string account, string id, Stream stream)
        {
            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(account+"/"+id);
                using (Stream s = stream)
                {
                    await blockBlob.UploadFromStreamAsync(s);
                }
            }
            catch
            {
                return null;
            }
            return account + id;
        }
        */
        public async Task<string> UploadFileThroughStreamAsync(string account, string spaceId, string file, Stream stream)
        {
            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(account + "/" + spaceId + "/" + file);
                using (Stream s = stream)
                {
                    await blockBlob.UploadFromStreamAsync(s);
                }
            }
            catch
            {
                return null;
            }
            return account + "/" + spaceId + "/" + file;
        }

        public async Task<string> UploadFileAsync(string account, string spaceId, StorageFile uploadfile)
        {
            return await this._UploadFileAsyncById(account + "/" + spaceId + "/" + MyEncoder.Decode(uploadfile.Name), uploadfile);
        }
        public async Task<string> UploadFileAsync(string account, string spaceId, string sourcePath, StorageFile uploadfile)
        {
            if ("".Equals(sourcePath))
            {
                return await this.UploadFileAsync(account, spaceId, uploadfile);
            }
            else
            {
                sourcePath = ParseHelper.TrimSlash(sourcePath);
                return await this._UploadFileAsyncById(account + "/" + spaceId + "/" + sourcePath + "/" + MyEncoder.Decode(uploadfile.Name), uploadfile);
            }
            
        }
        public async Task<bool> DeleteFileAsync(string id)
        {
            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(id);
                await blockBlob.DeleteAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        public async Task<bool> DeleteFileAsync(string account, string spaceId, string sourcePath)
        {
            return await this.DeleteFileAsync(account + "/" + spaceId + "/" + sourcePath);
        }


        private async Task<List<FileObject>> _GetFilesFromFolderByIdAsync(string id)
        {
            return await this._GetFileObjectListFromBlobClient(this.CONTAINER_NAME + "/" + id + "/");
        }

        private async Task<string> _UploadFileAsyncById(string id, StorageFile uploadfile)
        {
            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(id);
                using (Stream s = await uploadfile.OpenStreamForReadAsync())
                {
                    await blockBlob.UploadFromStreamAsync(s);
                }
            }
            catch
            {
                return null;
            }
            return id;
        }
        private void _GetParentId(string fullPath, out string name, out string prefix, out string parentId)
        {
            if (fullPath.EndsWith("/"))
                fullPath = fullPath.Substring(0, fullPath.Length - 1);
            name = fullPath.Substring(fullPath.LastIndexOf("/") + 1, fullPath.Length - fullPath.LastIndexOf("/") - 1);
            prefix = fullPath.Substring(0, fullPath.LastIndexOf("/") + 1);

            string temp = fullPath.Substring(0, fullPath.LastIndexOf("/"));
            parentId = temp.Substring(0, temp.LastIndexOf("/") + 1);

            if (!parentId.Substring(0, parentId.LastIndexOf("/")).Contains("/"))
            {

                parentId = prefix;
            }
        }

        private FileObject _GetFileObjectFromBlob(CloudBlockBlob blob)
        {
            string id = blob.Name;
            string name, parentId, prefix;

            this._GetParentId(id, out name, out prefix, out parentId);
            
            int size = (int)blob.Properties.Length;
            string type;
            string typeDetail = blob.Properties.ContentType;
            if (name.Contains("."))
            {
                type = name.Substring(name.LastIndexOf(".") + 1, name.Length - name.LastIndexOf(".") - 1);
            }
            else
            {
                type = typeDetail;
            }
            
            string createAt = blob.Properties.LastModified.ToString();
            string updateAt = blob.Properties.LastModified.ToString();

            return (new FileObject(id, name, parentId, size, type, typeDetail, createAt, updateAt));
        }
        private async Task<List<FileObject>> _GetFileObjectListFromBlobClient(string prefix)
        {
            List<FileObject> list = new List<FileObject>();
            BlobContinuationToken token = null;

            do
            {
                BlobResultSegment blobListSegment = await this.blobClient.ListBlobsSegmentedAsync(prefix, token);

                list.AddRange(_GetDataList(blobListSegment.Results));
                token = blobListSegment.ContinuationToken;
            } while (token != null);
            return list;
        }


        private FileObject _GetData(IListBlobItem item)
        {
            if (item.GetType() == typeof(CloudBlockBlob))
            {
                return this._GetFileObjectFromBlob((CloudBlockBlob)item);
            }
            else if (item.GetType() == typeof(CloudBlobDirectory))
            {
                CloudBlobDirectory directory = (CloudBlobDirectory)item;

                string id = directory.Prefix;
                string name, parentId, prefix;

                this._GetParentId(id, out name, out prefix, out parentId);
                int size = 0;
                string type = "folder";
                string typeDetail = "folder";
                string createAt = null;
                string updateAt = null;

                return (new FileObject(id, name, parentId, size, type, typeDetail, createAt, updateAt));
            }
            return null;
        }
        private List<FileObject> _GetDataList(IEnumerable<IListBlobItem> result)
        {
            List<FileObject> list = new List<FileObject>();
            foreach (IListBlobItem item in result)
            {
                FileObject fo = this._GetData(item);
                if (fo != null) list.Add(fo);
            }
            return list;
        }
    }
}
