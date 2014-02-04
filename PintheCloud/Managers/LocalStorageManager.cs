using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public class LocalStorageManager
    {
        public static string SKYDRIVE_FOLDER = "skydrive";
        public static string BLOBSTORAGE_FOLDER = "blobstorage";
        public LocalStorageManager()
        {

        }

        public async Task SetupAsync()
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalStorageManager.SKYDRIVE_FOLDER, CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalStorageManager.BLOBSTORAGE_FOLDER, CreationCollisionOption.ReplaceExisting);
        }
        public async Task<StorageFolder> GetSkyDriveStorageFolderAsync()
        {
            return await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalStorageManager.SKYDRIVE_FOLDER);
        }
        public async Task<StorageFolder> GetBlobStorageFolderAsync()
        {
            return await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalStorageManager.BLOBSTORAGE_FOLDER);
        }
        public async Task<StorageFile> CreateFileToLocalSkyDriveStorageAsync(string path)
        {
            return await this.CreateFileToLocalStorageAsync(path,await this.GetSkyDriveStorageFolderAsync());
        }

        public async Task<StorageFile> CreateFileToLocalBlobStorageAsync(string path)
        {
            return await this.CreateFileToLocalStorageAsync(path, await this.GetBlobStorageFolderAsync());
        }

        private async Task<StorageFile> CreateFileToLocalStorageAsync(string path, StorageFolder blob)
        {
            string folderName;
            StorageFolder folder = blob;
            while (path.Contains("/"))
            {
                folderName = getToken(path, out path);
                folder = await folder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
            }
            return await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
        }
        private string getToken(string path, out string slicedPath)
        {
            slicedPath = path;
            if (path.StartsWith("/"))
                path = path.Substring(1, path.Length - 1);
            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);
            if (path.Contains("/"))
            {
                string[] strlist = path.Split(new Char[] { '/' }, 2);
                slicedPath = strlist[1];
                return strlist[0];
            }
            return null;
        }
        private int count = 0;
        public string getCount()
        {
            string str = "";
            for (int i = 0; i < count; i++)
                str += "  ";
            return str;
        }


        public void PrintFile(StorageFile file)
        {
            if (file != null)
            {
                count++;
                App.HDebug.WriteLine(this.getCount() + file.Name+"("+file.Path+")");
                count--;
            }
        }
        public async Task PrintFolderAsync(StorageFolder folder)
        {
            if (folder != null)
            {
                count++;
                App.HDebug.WriteLine(this.getCount() + folder.Name+"("+folder.Path+")");
                IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();
                IReadOnlyList<StorageFolder> folderList = await folder.GetFoldersAsync();
                foreach (StorageFile file in fileList)
                {
                    PrintFile(file);
                }
                foreach (StorageFolder _folder in folderList)
                {
                    await PrintFolderAsync(_folder);
                }
                count--;
            }
            

        }
    }
}
