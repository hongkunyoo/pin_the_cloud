using PintheCloud.Utilities;
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
        public static string SKYDRIVE_DIRECTORY = "Shared/Transfers/";
        public static string SKYDRIVE_FOLDER = "skydrive";
        public static string BLOBSTORAGE_FOLDER = "blobstorage";

        public async Task SetupAsync()
        {
            await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Shared")).GetFolderAsync("Transfers")).CreateFolderAsync(LocalStorageManager.SKYDRIVE_FOLDER, CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalStorageManager.BLOBSTORAGE_FOLDER, CreationCollisionOption.ReplaceExisting);
        }
        public async Task<StorageFolder> GetSkyDriveStorageFolderAsync()
        {
            return await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Shared")).GetFolderAsync("Transfers")).GetFolderAsync(LocalStorageManager.SKYDRIVE_FOLDER);
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

        public async Task<StorageFolder> CreateFolderToSkyDriveStorage(string path)
        {
            return await this.CreateFolderToLocalStorageAsync(path, await this.GetSkyDriveStorageFolderAsync());
        }
        public async Task<StorageFile> GetSkyDriveStorageFileAsync(string path)
        {
            string name;
            string[] _path = ParseHelper.Parse(path, ParseHelper.Mode.FULL_PATH, out name);
            StorageFolder folder = await this.GetSkyDriveStorageFolderAsync();
            foreach (string p in _path)
            {
                folder = await folder.GetFolderAsync(p);
            }
            return await folder.GetFileAsync(name);
        }

        public async Task<Uri> GetSkyDriveDownloadUriFromPath(string path)
        {
            string name;
            string ori_path = path;
            string[] list = ParseHelper.Parse(path, ParseHelper.Mode.FULL_PATH, out name);
            //await this.CreateFileToLocalStorageAsync(path, await this.GetSkyDriveStorageFolderAsync());
            StorageFolder folder = await this.GetSkyDriveStorageFolderAsync();
            foreach (string s in list)
            {
                folder = await folder.CreateFolderAsync(s, CreationCollisionOption.OpenIfExists);
            }

            return new Uri("/" + LocalStorageManager.SKYDRIVE_DIRECTORY + LocalStorageManager.SKYDRIVE_FOLDER + (ori_path.StartsWith("/") ? ori_path : "/" + ori_path), UriKind.Relative);
        }
        private async Task<StorageFile> CreateFileToLocalStorageAsync(string path, StorageFolder folder)
        {
            string name;
            string[] list = ParseHelper.Parse(path, ParseHelper.Mode.DIRECTORY, out name);

            foreach (string s in list)
            {
                folder = await folder.CreateFolderAsync(s, CreationCollisionOption.OpenIfExists);
            }
            return await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
        }
        private async Task<StorageFolder> CreateFolderToLocalStorageAsync(string path, StorageFolder folder)
        {
            string name;
            string[] list = ParseHelper.Parse(path, ParseHelper.Mode.DIRECTORY, out name);

            foreach (string s in list)
            {
                folder = await folder.CreateFolderAsync(s, CreationCollisionOption.OpenIfExists);
            }
            return folder;
        }


        private int count = 0;
        private string getCount()
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
                MyDebug.WriteLine(this.getCount() + MyEncoder.Decode(file.Name) + "(" + file.Path + ")");
                count--;
            }
        }
        public void PrintFiles(IReadOnlyList<StorageFile> files)
        {
            if (files != null)
            {
                count++;
                foreach (StorageFile file in files)
                {
                    PrintFile(file);
                }
                count--;
            }
        }
        public async void PrintFolders(IReadOnlyList<StorageFolder> folders)
        {
            if (folders != null)
            {
                count++;
                foreach (StorageFolder folder in folders)
                {
                    await PrintFolderAsync(folder);
                }
                count--;
            }
        }
        public async Task PrintFolderAsync(StorageFolder folder)
        {
            if (folder != null)
            {
                count++;
                MyDebug.WriteLine(this.getCount() + "folder : " + MyEncoder.Decode(folder.Name) + "(" + folder.Path + ")");
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
