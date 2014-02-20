using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using PintheCloud.Models;
using PintheCloud.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public class GoogleDriveManager
    {
        private DriveService service;
        private User user;
        public static Dictionary<string, string> googleDocMapper;

        public GoogleDriveManager()
        {
            // Converting strings from google-docs to office files
            googleDocMapper = new Dictionary<string, string>();

            googleDocMapper.Add("application/vnd.google-apps.form", "Not Supported");
            googleDocMapper.Add("application/vnd.google-apps.folder", "Folder");

            // Document file
            googleDocMapper.Add("application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            // SpreadSheet file
            googleDocMapper.Add("application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            // Image file
            googleDocMapper.Add("application/vnd.google-apps.drawing", "image/png");
            // Presentation file
            googleDocMapper.Add("application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
        }
        public async Task SignIn()
        {
            UserCredential credential;
            try
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                      new ClientSecrets
                      {
                          ClientId = "109786198225-m8fihmv82b2fmf5k4d69u9039ebn68fn.apps.googleusercontent.com",
                          ClientSecret = "Tk8M01zlkBRlIsv-1fa9BKiS"
                      },
                      new[] { DriveService.Scope.Drive },
                      "user",
                      CancellationToken.None);

                this.service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "athere",
                });
                this.user = await this._GetUser();
            }
            catch (Microsoft.Phone.Controls.WebBrowserNavigationException ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            catch (Google.GoogleApiException e)
            {
                Debug.WriteLine(e.ToString());
            }
            
        }

        private async Task<User> _GetUser()
        {
            AboutResource aboutResource = service.About;
            About about = await aboutResource.Get().ExecuteAsync();
            return about.User;
        }

        public async Task<FileObject> GetRootFolderAsync()
        {
            FileList fileList = await this.service.Files.List().ExecuteAsync();
            FileObject rootFile = new FileObject();
            List<FileObject> childList = new List<FileObject>();
            foreach(Google.Apis.Drive.v2.Data.File file in fileList.Items)
            {
                if (this._IsRoot(file) && this._IsValidFile(file))
                {
                    PrintFile(file);
                    childList.Add(FileObjectConverter.ConvertToFileObject(file));
                }
            }
            rootFile.FileList = childList;
            rootFile.Name = "/";
            return rootFile;
        }
        public async Task<FileObject> GetFileAsync(string fileId)
        {
            Google.Apis.Drive.v2.Data.File file = await service.Files.Get(fileId).ExecuteAsync();
            return FileObjectConverter.ConvertToFileObject(file);
        }
        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {

            Google.Apis.Drive.v2.Data.File file = await service.Files.Get(folderId).ExecuteAsync();
            List<FileObject> list = new List<FileObject>();
            PrintFile(file);

            return list;
        }
        public async Task<StorageFile> DownloadFile(Google.Apis.Drive.v2.Data.File file)
        {
            byte[] inarray = await service.HttpClient.GetByteArrayAsync(file.DownloadUrl);
            MemoryStream input = new MemoryStream(inarray);
            byte[] buffer = new byte[1024000];
            StorageFile downfile = await ApplicationData.Current.LocalFolder.CreateFileAsync(file.OriginalFilename, CreationCollisionOption.ReplaceExisting);
            Stream output = await downfile.OpenStreamForWriteAsync();
            int count = 0;
            while ((count = input.Read(buffer, 0, buffer.Length)) != 0)
            {
                output.Write(buffer, 0, count);
            }
            input.Close();
            output.Close();
            return downfile;
        }
        private async Task DownloadFile(DriveService service, string url)
        {
            int DownloadChunkSize = 0;
            string UploadFileName = "";
            string DownloadDirectoryName = "";


            var downloader = new MediaDownloader(service);
            downloader.ChunkSize = DownloadChunkSize;

            // add a delegate for the progress changed event for writing to console on changes
            downloader.ProgressChanged += Download_ProgressChanged;

            // figure out the right file type base on UploadFileName extension
            var lastDot = UploadFileName.LastIndexOf('.');
            var fileName = DownloadDirectoryName + @"\Download" +
                (lastDot != -1 ? "." + UploadFileName.Substring(lastDot + 1) : "");

            using (var fileStream = new System.IO.FileStream(fileName,
                System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                var progress = await downloader.DownloadAsync(url, fileStream);
                if (progress.Status == DownloadStatus.Completed)
                {
                    Console.WriteLine(fileName + " was downloaded successfully");
                }
                else
                {
                    Console.WriteLine("Download {0} was interpreted in the middle. Only {1} were downloaded. ",
                        fileName, progress.BytesDownloaded);
                }
            }
        }
        private Task<IUploadProgress> UploadFileAsync(DriveService service)
        {
            string UploadFileName = "uploadfilename";
            string ContentType = "";

            var title = UploadFileName;
            if (title.LastIndexOf('\\') != -1)
            {
                title = title.Substring(title.LastIndexOf('\\') + 1);
            }

            var uploadStream = new System.IO.FileStream(UploadFileName, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);

            var insert = service.Files.Insert(new Google.Apis.Drive.v2.Data.File { Title = title }, uploadStream, ContentType);

            insert.ChunkSize = FilesResource.InsertMediaUpload.MinimumChunkSize * 2;
            insert.ProgressChanged += Upload_ProgressChanged;
            insert.ResponseReceived += Upload_ResponseReceived;

            var task = insert.UploadAsync();

            task.ContinueWith(t =>
            {
                // NotOnRanToCompletion - this code will be called if the upload fails
                Console.WriteLine("Upload Filed. " + t.Exception);
            }, TaskContinuationOptions.NotOnRanToCompletion);

            task.ContinueWith(t =>
            {
                uploadStream.Dispose();
            });

            return task;
        }
        private async Task DeleteFile(DriveService service, Google.Apis.Drive.v2.Data.File file)
        {
            Console.WriteLine("Deleting file '{0}'...", file.Id);
            await service.Files.Delete(file.Id).ExecuteAsync();
            Console.WriteLine("File was deleted successfully");
        }
        static void Download_ProgressChanged(IDownloadProgress progress)
        {
            Console.WriteLine(progress.Status + " " + progress.BytesDownloaded);
        }
        static void Upload_ProgressChanged(IUploadProgress progress)
        {
            Console.WriteLine(progress.Status + " " + progress.BytesSent);
        }
        static void Upload_ResponseReceived(Google.Apis.Drive.v2.Data.File file)
        {
            //uploadedFile = file;
        }
        private bool _IsValidFile(Google.Apis.Drive.v2.Data.File file)
        {
            return (this._IsMine(file) && !this._IsTrashed(file) && !"application/vnd.google-apps.form".Equals(file.MimeType));
        }
        private bool _IsRoot(Google.Apis.Drive.v2.Data.File file)
        {
            bool result = true;
            IList<ParentReference> parents = file.Parents;
            if (parents == null) return false;
            foreach (ParentReference parent in parents)
            {
                result &= parent.IsRoot.Value;
            }
            return result;
        }
        private bool _IsMine(Google.Apis.Drive.v2.Data.File file)
        {
            bool result = true;
            IList<User> owners = file.Owners;
            
            foreach (User user in owners)
            {
                if (this.user == null || user == null) Debugger.Break();
                result &= ((this.user.DisplayName.Equals(user.DisplayName)) && user.IsAuthenticatedUser.Value);
            }
            return result;
        }
        private bool _IsTrashed(Google.Apis.Drive.v2.Data.File file)
        {
            if (file.ExplicitlyTrashed == null) return false;
            return file.ExplicitlyTrashed.Value;
        }
        private bool _IsGoolgeDoc(Google.Apis.Drive.v2.Data.File file)
        {
            if (file.MimeType.Contains("application/vnd.google-apps")) return true;
            return false;
        }
        private bool _IsAbleToDownload(Google.Apis.Drive.v2.Data.File file)
        {
            return !string.Empty.Equals(file.DownloadUrl);
        }




        private void PrintFile(Google.Apis.Drive.v2.Data.File file)
        {
            Debug.WriteLine("DownloadUrl : " + file.DownloadUrl);
            Debug.WriteLine("FileSize : " + file.FileSize);
            Debug.WriteLine(">>>>>>>>>>Title : " + file.Title);
            Debug.WriteLine("ExplicitlyTrashed : " + file.ExplicitlyTrashed);
            Debug.WriteLine("FileExtension : " + file.FileExtension);
            Debug.WriteLine("IconLink : " + file.IconLink);
            Debug.WriteLine("Id : " + file.Id);
            Debug.WriteLine("MimeType : " + file.MimeType);
            Debug.WriteLine("ModifiedDate : " + file.ModifiedDate);
            Debug.WriteLine("ThumbnailLink : " + file.ThumbnailLink);
            Debug.WriteLine("ExportLinks : " + file.ExportLinks);

            
            if (file.ExportLinks != null)
            {
                Debug.WriteLine("----ExportLinks----");
                ICollection<string> keys = file.ExportLinks.Keys;
                foreach (string key in keys)
                {
                    Debug.WriteLine(key + " : " + file.ExportLinks[key]);
                }
            }
            

            Debug.WriteLine("----OwerNames----");
            foreach (string n in file.OwnerNames)
            {
                Debug.WriteLine("ownersName : " + n);
                Debug.WriteLine("--------------");
            }
            Debug.WriteLine("----Onwers----");
            foreach (User u in file.Owners)
            {
                Debug.WriteLine("DisplayName : " + u.DisplayName);
                Debug.WriteLine("IsAuthenticatedUser : " + u.IsAuthenticatedUser);
                Debug.WriteLine("Kind : " + u.Kind);
                Debug.WriteLine("PermissionId : " + u.PermissionId);
                Debug.WriteLine("Picture : " + u.Picture);
                Debug.WriteLine("ToString : " + u.ToString());
                Debug.WriteLine("---------------------------");
            }
            Debug.WriteLine("----Parents----");
            foreach (ParentReference pr in file.Parents)
            {
                Debug.WriteLine("Id : " + pr.Id);
                Debug.WriteLine("IsRoot : " + pr.IsRoot);
                Debug.WriteLine("Kind : " + pr.Kind);
                Debug.WriteLine("ParentLink : " + pr.ParentLink);
                Debug.WriteLine("SelfLink : " + pr.SelfLink);
                Debug.WriteLine("ToString : " + pr.ToString());
                Debug.WriteLine("---------------------------");
            }
            Debug.WriteLine("Thumbnail : " + file.Thumbnail);
            Debug.WriteLine("=====================================================");

        }
    }
}
