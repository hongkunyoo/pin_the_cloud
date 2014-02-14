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
    public interface CloudManager
    {
        Task<bool> SignIn(DependencyObject context);

        void SignOut();

        Account GetCurrentAccount();

        Task<FileObject> GetRootFolderAsync();

        Task<List<FileObject>> GetRootFilesAsync();

        Task<FileObject> GetFileAsync(string fileId);

        Task<List<FileObject>> GetFilesFromFolderAsync(string folderId);

        Task<StorageFile> DownloadFileAsync(string sourceFileId, Uri destinationUri);

        Task<Stream> DownloadFileThroughStreamAsync(string sourceFileId);

        Task<bool> UploadFileAsync(string folderIdToStore, StorageFile file);

        Task<bool> UploadFileThroughStreamAsync(string folderIdToStore, string fileName, Stream outstream);

        Task<StorageFolder> DownloadFolderAsync(string sourceFolderId, StorageFolder folder);
    }
}
