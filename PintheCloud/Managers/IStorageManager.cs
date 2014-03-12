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
        StorageAccount GetStorageAccount();
        bool IsSignIn();
        string GetStorageName();
        string GetStorageImageUri();
        string GetStorageColorHexString();
        Stack<FileObjectViewItem> GetFolderRootTree();
        Stack<List<FileObject>> GetFoldersTree();
        Task<FileObject> GetRootFolderAsync();
        Task<List<FileObject>> GetRootFilesAsync();
        Task<FileObject> GetFileAsync(string fileId);
        Task<List<FileObject>> GetFilesFromFolderAsync(string folderId);
        Task<Stream> DownloadFileStreamAsync(string sourceFileId);
        Task<bool> UploadFileStreamAsync(string folderIdToStore, string fileName, Stream outstream);

        # region Not Using Methos
        //Task<StorageFile> DownloadFileAsync(string sourceFileId, Uri destinationUri);
        //Task<StorageFolder> DownloadFolderAsync(string sourceFolderId, StorageFolder folder);
        //Task<bool> UploadFileAsync(string folderIdToStore, StorageFile file);
        # endregion
    }
}
