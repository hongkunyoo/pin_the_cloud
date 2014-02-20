using Microsoft.Live;
using Microsoft.Phone.Shell;
using PintheCloud.Models;
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
        Task SignIn();
        void SignOut();
        Account GetAccount();
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
