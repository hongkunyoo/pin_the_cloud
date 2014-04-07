using Microsoft.Live;
using Microsoft.Phone.Shell;
using PintheCloud.Models;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public interface IStorageManager
    {
        Task<bool> SignIn();
        bool IsSigningIn();
        void SignOut();
        bool IsPopup();
        Task<StorageAccount> GetStorageAccountAsync();
        bool IsSignIn();
        string GetStorageName();
        string GetStorageImageUri();
        string GetStorageColorHexString();
        Task<FileObject> GetRootFolderAsync();
        Task<List<FileObject>> GetRootFilesAsync();
        Task<FileObject> GetFileAsync(string fileId);
        Task<List<FileObject>> GetFilesFromFolderAsync(string folderId);
        Task<Stream> DownloadFileStreamAsync(string sourceFileId);
        Task<bool> UploadFileStreamAsync(string folderIdToStore, string fileName, Stream outstream);
        Task<FileObject> Synchronize();
    }
}
