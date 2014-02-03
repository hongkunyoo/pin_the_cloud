using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace PintheCloud.Managers
{
    public class BlobManager
    {
        public static string BLOB_CONNECTION = "DefaultEndpointsProtocol=http;AccountName=rfrost77;AccountKey=0GgFw4h4JiCklbD5avI2YBitfyuOj9GZQQpRaMEDZEhRtPohzlsUlAOie3SfZE3aMXFCcto1hi/Gu1Ppw4ZuRg==";
        
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudBlobContainer container;

        public BlobManager()
        {
            this.storageAccount = CloudStorageAccount.Parse(BlobManager.BLOB_CONNECTION);
            this.blobClient = this.storageAccount.CreateCloudBlobClient();
            //this.container = blobClient.GetContainerReference(App.CurrentAccountManager.GetCurrentAcccount().account_name.Replace(" ", string.Empty).ToLower());
            this.container = blobClient.GetContainerReference("images");
        }
        public async Task<bool> UploadFile(string sourcePath, StorageFile uploadfile)
        {
            try
            {
                await this.container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(sourcePath);
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
        public async Task GetFilesFromRootAsync()
        {
            await this.container.CreateIfNotExistsAsync();

            // how ListBlobsSegmentedAsync works
            BlobResultSegment blobListSegment = await container.ListBlobsSegmentedAsync(null);
            await PrintResult(blobListSegment);
        }
        public async Task GetFilesFromFolder(string path)
        {
            
        }
        private async Task PrintResult(BlobResultSegment br){
            foreach (IListBlobItem item in br.Results)
            {
                // findout every properties in blob
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    
                    System.Diagnostics.Debug.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);

                }
                else if (item.GetType() == typeof(CloudPageBlob))
                {
                    CloudPageBlob pageBlob = (CloudPageBlob)item;

                    System.Diagnostics.Debug.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                }
                else if (item.GetType() == typeof(CloudBlobDirectory))
                {
                    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                    BlobResultSegment rb = await directory.ListBlobsSegmentedAsync(null);
                    
                    System.Diagnostics.Debug.WriteLine("rb : "+ rb);
                    await PrintResult(rb);
                    
                    System.Diagnostics.Debug.WriteLine("Directory: {0}", directory.Uri);
                }
            }
        }
        private async Task<StorageFile> DownloadFile(string sourcePath, StorageFile downloadFile)
        {
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(sourcePath);
            using (Stream s = await downloadFile.OpenStreamForWriteAsync())
            {
                await blockBlob.DownloadToStreamAsync(s);
            }
            return downloadFile;
        }
        private async Task<bool> DeleteFile(string sourcePath)
        {
            try
            {
                await container.CreateIfNotExistsAsync();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(sourcePath);
                await blockBlob.DeleteAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
