using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Managers
{
    interface IStorageManager
    {
        Task<FileObject> GetRootFolderAsync();
        Task<FileObject> GetFileAsync(string fileId);
        Task<List<FileObject>> GetFilesFromFolderAsync(string folderId);
        Task<StorageFolder> DownloadFolderAsync(string sourceFolder, StorageFolder folder);
    }
}
