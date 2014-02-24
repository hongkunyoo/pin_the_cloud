using PintheCloud.Helpers;
using PintheCloud.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Managers
{
    /// <summary>
    /// Helps to handle every kind of local storage issues.
    /// Don't need to think of whether the folder exists of not.
    /// If not, it will make the folder automatically for you.
    /// Just past the path "/directory/subdir/file" then it will give you the exact file.
    /// </summary>
    public class LocalStorageManager
    {
        /// <summary>
        /// Default OneDrive directory.
        /// Every OneDrive file will be downloaded here.
        /// </summary>
        public static string ONE_DRIVE_DIRECTORY = "Shared/Transfers/";
        /// <summary>
        /// Default OneDrive folder.
        /// </summary>
        public static string ONE_DRIVE_FOLDER = "skydrive";
        /// <summary>
        /// Default Blob Storage folder.
        /// </summary>
        public static string BLOB_STORAGE_FOLDER = "blobstorage";
        /// <summary>
        /// Constructor. Creating each local storages.
        /// </summary>
        /// <returns></returns>
        public async Task SetupAsync()
        {
            await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Shared")).GetFolderAsync("Transfers")).CreateFolderAsync(LocalStorageManager.ONE_DRIVE_FOLDER, CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalStorageManager.BLOB_STORAGE_FOLDER, CreationCollisionOption.ReplaceExisting);
        }
        /// <summary>
        /// Get SkyDrive default local location.
        /// </summary>
        /// <returns>Default SkyDrive Folder</returns>
        public async Task<StorageFolder> GetSkyDriveStorageFolderAsync()
        {
            return await (await (await ApplicationData.Current.LocalFolder.GetFolderAsync("Shared")).GetFolderAsync("Transfers")).GetFolderAsync(LocalStorageManager.ONE_DRIVE_FOLDER);
        }
        /// <summary>
        /// Get Blob Storage default local location.
        /// </summary>
        /// <returns>Default Blob Storage Folder</returns>
        public async Task<StorageFolder> GetBlobStorageFolderAsync()
        {
            return await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalStorageManager.BLOB_STORAGE_FOLDER);
        }
        /// <summary>
        /// Creating SkyDriveStorage File
        /// </summary>
        /// <returns>The file requested</returns>
        public async Task<StorageFile> CreateFileToLocalSkyDriveStorageAsync(string path)
        {
            return await this.CreateFileToLocalStorageAsync(path,await this.GetSkyDriveStorageFolderAsync());
        }
        /// <summary>
        /// Creating Blob Storage File
        /// </summary>
        /// <returns>The file requested</returns>
        public async Task<StorageFile> CreateFileToLocalBlobStorageAsync(string path)
        {
            return await this.CreateFileToLocalStorageAsync(path, await this.GetBlobStorageFolderAsync());
        }
        /// <summary>
        /// Creating SkyDrive Folder by path
        /// </summary>
        /// <returns>The folder requested</returns>
        public async Task<StorageFolder> CreateFolderToSkyDriveStorage(string path)
        {
            return await this.CreateFolderToLocalStorageAsync(path, await this.GetSkyDriveStorageFolderAsync());
        }
        /// <summary>
        /// Get SkyDrive File by path
        /// </summary>
        /// <returns>The file requested</returns>
        public async Task<StorageFile> GetSkyDriveStorageFileAsync(string path)
        {
            string name;
            string[] _path = ParseHelper.ParsePathAndName(path, ParseHelper.Mode.FULL_PATH, out name);
            StorageFolder folder = await this.GetSkyDriveStorageFolderAsync();
            foreach (string p in _path)
            {
                folder = await folder.GetFolderAsync(p);
            }
            return await folder.GetFileAsync(name);
        }
        /// <summary>
        /// Get SkyDrive Download Uri by path
        /// </summary>
        /// <returns>The Uri for download location</returns>
        public async Task<Uri> GetSkyDriveDownloadUriFromPath(string path)
        {
            string name;
            string ori_path = path;
            string[] list = ParseHelper.ParsePathAndName(path, ParseHelper.Mode.FULL_PATH, out name);
            StorageFolder folder = await this.GetSkyDriveStorageFolderAsync();
            foreach (string s in list)
            {
                folder = await folder.CreateFolderAsync(s, CreationCollisionOption.OpenIfExists);
            }

            return new Uri(PtcEncoder.Encode("/" + LocalStorageManager.ONE_DRIVE_DIRECTORY + LocalStorageManager.ONE_DRIVE_FOLDER + (ori_path.StartsWith("/") ? ori_path : "/" + ori_path)), UriKind.Relative);
        }

        /// <summary>
        /// Private Methods & Testing Methods.
        /// </summary>
        private async Task<StorageFile> CreateFileToLocalStorageAsync(string path, StorageFolder folder)
        {
            string name;
            string[] list = ParseHelper.ParsePathAndName(path, ParseHelper.Mode.FULL_PATH, out name); // changed DIRECTORY to FULL_PATH

            foreach (string s in list)
            {
                folder = await folder.CreateFolderAsync(s, CreationCollisionOption.OpenIfExists);
            }
            return await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
        }
        private async Task<StorageFolder> CreateFolderToLocalStorageAsync(string path, StorageFolder folder)
        {
            string name;
            string[] list = ParseHelper.ParsePathAndName(path, ParseHelper.Mode.DIRECTORY, out name);

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
                Debug.WriteLine(this.getCount() + PtcEncoder.Decode(file.Name) + "(" + file.Path + ")");
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
                Debug.WriteLine(this.getCount() + "folder : " + PtcEncoder.Decode(folder.Name) + "(" + folder.Path + ")");
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
