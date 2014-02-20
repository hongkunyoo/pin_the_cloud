﻿using Google.Apis.Auth.OAuth2;
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
        #region Variables
        private DriveService service;
        private Account CurrentAccount;
        public static Dictionary<string, string> GoogleDocMapper;
        public static Dictionary<string, string> ExtensionMapper;
        private string GOOGLE_DRIVE_USER = "GOOGLE_DRIVE_USER";

        UserCredential credential;
        #endregion

        public GoogleDriveManager()
        {
            // Converting strings from google-docs to office files
            GoogleDocMapper = new Dictionary<string, string>();

            //GoogleDocMapper.Add("application/vnd.google-apps.form", "Not Supported");
            //GoogleDocMapper.Add("application/vnd.google-apps.folder", "Folder");

            // Document file
            GoogleDocMapper.Add("application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            // SpreadSheet file
            GoogleDocMapper.Add("application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            // Image file
            GoogleDocMapper.Add("application/vnd.google-apps.drawing", "image/png");
            // Presentation file
            GoogleDocMapper.Add("application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation");

            /*
            // Convert from extension to mimeType
            ExtensionMapper = new Dictionary<string, string>();
            // MP3
            ExtensionMapper.Add("","");
            // PDF
            ExtensionMapper.Add("", "");
            // PPT
            ExtensionMapper.Add("", "");
            // XLS
            ExtensionMapper.Add("", "");
            // DOC
            ExtensionMapper.Add("", "");
            // PNG
            ExtensionMapper.Add("", "");
            // JPG
            ExtensionMapper.Add("", "");
            // MP4
            ExtensionMapper.Add("", "");
            // HWP
            ExtensionMapper.Add("", "");
            // WMA
            ExtensionMapper.Add("", "");
             * */
        }
        public async Task SignIn()
        {
            
            try
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                      new ClientSecrets
                      {
                          ClientId = "109786198225-m8fihmv82b2fmf5k4d69u9039ebn68fn.apps.googleusercontent.com",
                          ClientSecret = "Tk8M01zlkBRlIsv-1fa9BKiS"
                      },
                      new[] { DriveService.Scope.Drive },
                      this._GetUserSession(),
                      CancellationToken.None);
                this.service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "athere",
                });
                AboutResource aboutResource = service.About;
                About about = await aboutResource.Get().ExecuteAsync();
                this.CurrentAccount = new Account();
                this.CurrentAccount.RawAccount = about.User;
                this.CurrentAccount.AccountType = Account.StorageAccountType.GOOGLE_DRIVE;
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
        
        public void SignOut()
        {
            App.ApplicationSettings.Remove(GOOGLE_DRIVE_USER);
        }

        public Account GetAccount()
        {
            return this.CurrentAccount;
        }
        public async Task<FileObject> GetRootFolderAsync()
        {
            FileObject rootFile = new FileObject();
            AboutResource aboutResource = service.About;
            About about = await aboutResource.Get().ExecuteAsync();
            rootFile.Id = about.RootFolderId;
            rootFile.Name = "/";
            return rootFile;
        }
        public async Task<List<FileObject>> GetRootFilesAsync()
        {
            FileList fileList = await this.service.Files.List().ExecuteAsync();
            List<FileObject> childList = new List<FileObject>();
            foreach (Google.Apis.Drive.v2.Data.File file in fileList.Items)
            {
                if (this._IsRoot(file) && this._IsValidFile(file))
                {
                    childList.Add(FileObjectConverter.ConvertToFileObject(file));
                }
            }
            return childList;
        }
        public async Task<FileObject> GetFileAsync(string fileId)
        {
            Google.Apis.Drive.v2.Data.File file = await service.Files.Get(fileId).ExecuteAsync();
            return FileObjectConverter.ConvertToFileObject(file);
        }
        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            List<FileObject> list = new List<FileObject>();
            ChildList childList = await service.Children.List(folderId).ExecuteAsync();
            foreach(ChildReference child in childList.Items){
                list.Add(await this.GetFileAsync(child.Id));
            }
            return list;
        }

        public async Task<Stream> DownloadFileStreamAsnyc(string fileId)
        {
            byte[] inarray = await service.HttpClient.GetByteArrayAsync(fileId);
            return new MemoryStream(inarray);
        }
        private async Task<Stream> DownloadFileStreamAsnyc2(string downloadUrl)
        {
            var downloader = new MediaDownloader(this.service);
            MemoryStream ms = new MemoryStream();
            var progress = await downloader.DownloadAsync(downloadUrl, ms);
            if (progress.Status == DownloadStatus.Completed)
            {
                return ms;
            }
            else
            {
                return null;
            }
        }
        private async Task<bool> UploadFileAsync(string folderId, string fileName, Stream inputStream)
        {
            try
            {
                Google.Apis.Drive.v2.Data.File file = new Google.Apis.Drive.v2.Data.File();
                file.Title = fileName;

                ParentReference p = new ParentReference();
                p.Id = folderId;
                file.Parents.Add(p);

                string extension = ParseHelper.SplitNameAndExtension(fileName)[1];
                var insert = service.Files.Insert(file, inputStream, GoogleDriveManager.ExtensionMapper[extension]);
                var task = await insert.UploadAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        


        #region Private Methods
        private string _GetUserSession()
        {
            if (App.ApplicationSettings.Contains(GOOGLE_DRIVE_USER))
            {
                return (string)App.ApplicationSettings[GOOGLE_DRIVE_USER];
            }
            else
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random(DateTime.Now.Millisecond);
                var result = new string(
                    Enumerable.Repeat(chars, 8)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());
                App.ApplicationSettings.Add(GOOGLE_DRIVE_USER, result);
                App.ApplicationSettings.Save();
                return result;
            }
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
                result &= ((((User)this.CurrentAccount.RawAccount).DisplayName.Equals(user.DisplayName)) && user.IsAuthenticatedUser.Value);
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
        #endregion

        #region Test Methods
        private void PrintFile(Google.Apis.Drive.v2.Data.File file)
        {
            Debug.WriteLine(">>>>>>>>>>Title : " + file.Title);
            Debug.WriteLine("DownloadUrl : " + file.DownloadUrl);
            Debug.WriteLine("FileSize : " + file.FileSize);
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
        #endregion

        #region Not Using Methods
        private async Task DeleteFile(string fileId)
        {
            await service.Files.Delete(fileId).ExecuteAsync();
        }
        #endregion
    }
}