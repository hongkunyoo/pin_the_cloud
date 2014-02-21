using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using Windows.ApplicationModel;
using Windows.Storage;

namespace PintheCloud.Managers
{
    public class GoogleDriveManager : IStorageManager
    {
        #region Variables
        private const string CLIENT_ID = "109786198225-m8fihmv82b2fmf5k4d69u9039ebn68fn.apps.googleusercontent.com";
        private const string CLIENT_SECRET = "Tk8M01zlkBRlIsv-1fa9BKiS";
        private const string GOOGLE_DRIVE_USER_KEY = "GOOGLE_DRIVE_USER_KEY";

        private const string ACCOUNT_IS_SIGN_IN_KEY = "ACCOUNT_GOOGLE_DRIVE_SIGN_IN_KEY";
        private const string ACCOUNT_USED_SIZE_KEY = "ACCOUNT_GOOGLE_DRIVE_USED_SIZE_KEY";
        private const string ACCOUNT_BUSINESS_TYPE_KEY = "ACCOUNT_GOOGLE_DRIVE_BUSINESS_TYPE_KEY";

        public static Dictionary<string, string> GoogleDocMapper;
        public static Dictionary<string, string> MimeTypeMapper;
        public static Dictionary<string, string> ExtensionMapper;

        private DriveService service;
        private UserCredential credential;
        private Account CurrentAccount;
        #endregion

        public GoogleDriveManager()
        {
            // Converting strings from google-docs to office files
            GoogleDriveManager.GoogleDocMapper = new Dictionary<string, string>();
            GoogleDriveManager.ExtensionMapper = new Dictionary<string, string>();

            //GoogleDocMapper.Add("application/vnd.google-apps.form", "Not Supported");
            //GoogleDocMapper.Add("application/vnd.google-apps.folder", "Folder");

            // Document file
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            // SpreadSheet file
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            // Image file
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.drawing", "image/png");
            // Presentation file
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation");

            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.document", ".doc");
            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.spreadsheet", ".xls");
            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.drawing", ".png");
            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.presentation", ".ppt");
            
            Task setMimeTypeMapperTask = this.SetMimeTypeMapper();
        }


        public async Task SignIn()
        {
            // Add application settings before work for good UX
            App.ApplicationSettings[ACCOUNT_IS_SIGN_IN_KEY] = true;
            App.ApplicationSettings.Save();

            try
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                      new ClientSecrets
                      {
                          ClientId = CLIENT_ID,
                          ClientSecret = CLIENT_SECRET
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

                // TODO Make Account
                // TODO Insert Account
                // TODO Save Account to this.Account
            }
            catch (Microsoft.Phone.Controls.WebBrowserNavigationException ex)
            {
                this.SignOut();
                Debug.WriteLine(ex.ToString());
            }
            catch (Google.GoogleApiException e)
            {
                this.SignOut();
                Debug.WriteLine(e.ToString());
            }
        }
        

        public void SignOut()
        {
            App.ApplicationSettings.Remove(ACCOUNT_IS_SIGN_IN_KEY);
            App.ApplicationSettings.Remove(ACCOUNT_USED_SIZE_KEY);
            App.ApplicationSettings.Remove(ACCOUNT_BUSINESS_TYPE_KEY);
            this.CurrentAccount = null;
        }


        public Account GetAccount()
        {
            return this.CurrentAccount;
        }


        public string GetAccountIsSignInKey()
        {
            return ACCOUNT_IS_SIGN_IN_KEY;
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


        public async Task<Stream> DownloadFileStreamAsync(string fileId)
        {
            byte[] inarray = await service.HttpClient.GetByteArrayAsync(fileId);
            return new MemoryStream(inarray);
        }


        public async Task<bool> UploadFileStreamAsync(string folderId, string fileName, Stream inputStream)
        {
            try
            {
                Google.Apis.Drive.v2.Data.File file = new Google.Apis.Drive.v2.Data.File();
                file.Title = fileName;

                ParentReference p = new ParentReference();
                p.Id = folderId;
                file.Parents = new List<ParentReference>();
                file.Parents.Add(p);
                
                string extension = ParseHelper.SplitNameAndExtension(fileName)[1];
                var insert = service.Files.Insert(file, inputStream, GoogleDriveManager.MimeTypeMapper[extension]);
                var task = await insert.UploadAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }
        

        #region Private Methods
        private async Task SetMimeTypeMapper()
        {
            StorageFile js = await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFileAsync("mimeType.js");
            JsonTextReader jtr = new JsonTextReader(new StreamReader(await js.OpenStreamForReadAsync()));
            Newtonsoft.Json.JsonSerializer s = new JsonSerializer();
            GoogleDriveManager.MimeTypeMapper = s.Deserialize<Dictionary<string, string>>(jtr);
        }


        private string _GetUserSession()
        {
            if (App.ApplicationSettings.Contains(GOOGLE_DRIVE_USER_KEY))
            {
                return (string)App.ApplicationSettings[GOOGLE_DRIVE_USER_KEY];
            }
            else
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random(DateTime.Now.Millisecond);
                var result = new string(
                    Enumerable.Repeat(chars, 8)
                              .Select(s => s[random.Next(s.Length)])
                              .ToArray());
                App.ApplicationSettings.Add(GOOGLE_DRIVE_USER_KEY, result);
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
                // TODO Get that values from converted account.
                //result &= ((((User)this.CurrentAccount.RawAccount).DisplayName.Equals(user.DisplayName)) && user.IsAuthenticatedUser.Value);
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
        //private async Task DeleteFile(string fileId)
        //{
        //    await service.Files.Delete(fileId).ExecuteAsync();
        //}
        //public async Task<Stream> DownloadFileStreamAsync2(string downloadUrl)
        //{
        //    var downloader = new MediaDownloader(this.service);
        //    MemoryStream ms = new MemoryStream();
        //    var progress = await downloader.DownloadAsync(downloadUrl, ms);
        //    if (progress.Status == DownloadStatus.Completed)
        //    {
        //        return ms;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        #endregion
    }
}
