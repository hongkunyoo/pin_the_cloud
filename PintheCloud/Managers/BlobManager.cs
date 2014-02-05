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

namespace PintheCloud.Managers
{
    public class BlobManager
    {
        public static string BLOB_CONNECTION = "DefaultEndpointsProtocol=http;AccountName=rfrost77;AccountKey=0GgFw4h4JiCklbD5avI2YBitfyuOj9GZQQpRaMEDZEhRtPohzlsUlAOie3SfZE3aMXFCcto1hi/Gu1Ppw4ZuRg==";
        
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer container;
        private string account;

        public BlobManager()
        {
            this.storageAccount = CloudStorageAccount.Parse(BlobManager.BLOB_CONNECTION);
            this.blobClient = this.storageAccount.CreateCloudBlobClient();
            this.account = App.AccountManager.GetCurrentAcccount().account_platform_id.Replace(" ", string.Empty);
            //this.account = "rfrost77@gmail.com";
            this.container = blobClient.GetContainerReference("images");
        }
        
        public async Task<List<FileObject>> GetRootFilesAsync(string spaceId)
        {
            CloudBlobDirectory directory = container.GetDirectoryReference("" + this.account + "/" + spaceId);
            return await this.getFileObjectListFromDirectoryAsync(directory);
        }
        public async Task<FileObject> GetFile(string id)
        {
            CloudBlockBlob blockBlob = (CloudBlockBlob)await container.GetBlobReferenceFromServerAsync(id);
            return this.getFileObjectFromBlob(blockBlob);
        }
        public async Task<FileObject> GetFile(string spaceId, string sourcePath)
        {
            return await this.GetFile(this.account + "/" + spaceId + "/" + sourcePath);
        }
        public Task<List<FileObject>> GetFilesFromFolderAsync(string id)
        {
            CloudBlobDirectory directory = container.GetDirectoryReference(id);
            return this.getFileObjectListFromDirectoryAsync(directory);
        }
        public Task<List<FileObject>> GetFilesFromFolderAsync(string spaceId, string sourcePath)
        {
            return this.GetFilesFromFolderAsync(this.account + "/" + spaceId + "/" + sourcePath);
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
        public async Task<StorageFile> DownloadFileAsync(string spaceId, string sourcePath, StorageFile downloadFile)
        {
            return await this.DownloadFileAsync(this.account + "/" + spaceId + "/" + sourcePath,downloadFile);
        }
        public async Task<bool> UploadFileAsync(string id, StorageFile uploadfile)
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
                return false;
            }
            return true;
        }
        public async Task<bool> UploadFileAsync(string spaceId, string sourcePath, StorageFile uploadfile)
        {
            return await this.UploadFileAsync(this.account + "/" + spaceId + "/" + sourcePath, uploadfile);
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
        public async Task<bool> DeleteFileAsync(string spaceId, string sourcePath)
        {
            return await this.DeleteFileAsync(this.account + "/" + spaceId + "/" + sourcePath);
        }



        private void getParentId(string fullPath, out string name, out string prefix, out string parentId)
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

        private FileObject getFileObjectFromBlob(CloudBlockBlob blob)
        {
            string id = blob.Name;
            string name, parentId, prefix;

            this.getParentId(id, out name, out prefix, out parentId);
            
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
        private async Task<List<FileObject>> getFileObjectListFromDirectoryAsync(CloudBlobDirectory directory)
        {
            List<FileObject> list = new List<FileObject>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment blobListSegment = await directory.ListBlobsSegmentedAsync(null);
                list.AddRange(getDataList(blobListSegment.Results));
                token = blobListSegment.ContinuationToken;
            } while (token != null);

            return list;
        }

        private FileObject getData(IListBlobItem item)
        {
            if (item.GetType() == typeof(CloudBlockBlob))
            {
                return this.getFileObjectFromBlob((CloudBlockBlob)item);
            }
            else if (item.GetType() == typeof(CloudBlobDirectory))
            {
                CloudBlobDirectory directory = (CloudBlobDirectory)item;

                string id = directory.Prefix;
                string name, parentId, prefix;

                this.getParentId(id, out name, out prefix, out parentId);
                int size = 0;
                string type = "folder";
                string typeDetail = "folder";
                string createAt = null;
                string updateAt = null;

                return (new FileObject(id, name, parentId, size, type, typeDetail, createAt, updateAt));
            }
            return null;
        }
        private List<FileObject> getDataList(IEnumerable<IListBlobItem> result)
        {
            List<FileObject> list = new List<FileObject>();
            foreach (IListBlobItem item in result)
            {
                FileObject fo = this.getData(item);
                if (fo != null) list.Add(fo);
            }
            return list;
        }
    }
}
