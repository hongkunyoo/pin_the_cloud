﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PintheCloud.Converters;
using PintheCloud.Helpers;
using PintheCloud.Models;
using PintheCloud.Pages;
using PintheCloud.Resources;
using PintheCloud.Utilities;
using PintheCloud.ViewModels;
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
        private const string GOOGLE_DRIVE_CLIENT_ID = "109786198225-m8fihmv82b2fmf5k4d69u9039ebn68fn.apps.googleusercontent.com";
        private const string GOOGLE_DRIVE_CLIENT_SECRET = "Tk8M01zlkBRlIsv-1fa9BKiS";
        
        private const string GOOGLE_DRIVE_USER_KEY = "GOOGLE_DRIVE_USER_KEY";
        private const string GOOGLE_DRIVE_SIGN_IN_KEY = "GOOGLE_DRIVE_SIGN_IN_KEY";

        private const string GOOGLE_DRIVE_IMAGE_URI = "/Assets/pajeon/at_here/png/navi_ico_googledrive.png";
        private const string GOOGLE_DRIVE_COLOR_HEX_STRING = "F1AE1D";

        public static Dictionary<string, string> GoogleDocMapper;
        public static Dictionary<string, string> MimeTypeMapper;
        public static Dictionary<string, string> ExtensionMapper;

        private DriveService Service;
        private UserCredential Credential;
        private StorageAccount CurrentAccount;
        private User User;
        private string RootFodlerId = String.Empty;
        private TaskCompletionSource<bool> tcs = null;
        #endregion



        public GoogleDriveManager()
        {
            // Converting strings from google-docs to office files
            GoogleDriveManager.GoogleDocMapper = new Dictionary<string, string>();
            GoogleDriveManager.ExtensionMapper = new Dictionary<string, string>();

            // Document file
            // SpreadSheet file
            // Image file
            // Presentation file 
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.drawing", "image/png");
            GoogleDriveManager.GoogleDocMapper.Add("application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation");

            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.document", "doc");
            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.spreadsheet", "xls");
            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.drawing", "png");
            GoogleDriveManager.ExtensionMapper.Add("application/vnd.google-apps.presentation", "ppt");

            Task setMimeTypeMapperTask = this.SetMimeTypeMapper();
        }


        public async Task<bool> SignIn()
        {
            this.tcs = new TaskCompletionSource<bool>();
            try
            {
                this.Credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = GOOGLE_DRIVE_CLIENT_ID,
                        ClientSecret = GOOGLE_DRIVE_CLIENT_SECRET
                    },
                    new[] { DriveService.Scope.Drive },
                    this._GetUserSession(),
                    CancellationToken.None
                );
                
                this.Service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = this.Credential,
                    ApplicationName = "At Here",
                });

                AboutResource aboutResource = this.Service.About;
                About about = await aboutResource.Get().ExecuteAsync();
                this.User = about.User;

                string name = this.User.DisplayName;
                string id = about.PermissionId;

                // Register account
                StorageAccount account = await App.AccountManager.GetStorageAccountAsync(id);
                if (account == null)
                {
                    account = new StorageAccount(id, StorageAccount.StorageAccountType.GOOGLE_DRIVE, name, 0.0);
                    await App.AccountManager.CreateStorageAccountAsync(account);
                }
                this.CurrentAccount = account;

                // Save sign in setting.
                App.ApplicationSettings[GOOGLE_DRIVE_SIGN_IN_KEY] = true;
                App.ApplicationSettings.Save();
                TaskHelper.AddTask(TaskHelper.STORAGE_EXPLORER_SYNC + this.GetStorageName(), StorageExplorer.Synchronize(this.GetStorageName()));
                this.tcs.SetResult(true);
            }
            catch (Microsoft.Phone.Controls.WebBrowserNavigationException)
            {
                this.tcs.SetResult(false);
            }
            catch (Google.GoogleApiException)
            {
                this.tcs.SetResult(false);
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                this.tcs.SetResult(false);
            }
            catch (Google.Apis.Auth.OAuth2.Responses.TokenResponseException)
            {
                this.tcs.SetResult(false);
            }
            catch(Exception)
            {
                this.tcs.SetResult(false);
            }
            return this.tcs.Task.Result;
        }


        public bool IsSigningIn()
        {
            if (this.tcs != null)
                return !this.tcs.Task.IsCompleted && !App.ApplicationSettings.Contains(GOOGLE_DRIVE_SIGN_IN_KEY);
            else
                return false;
        }


        // Remove user and record
        public void SignOut()
        {
            App.ApplicationSettings.Remove(GOOGLE_DRIVE_USER_KEY);
            App.ApplicationSettings.Remove(GOOGLE_DRIVE_SIGN_IN_KEY);
            StorageExplorer.RemoveKey(this.GetStorageName());
            this.CurrentAccount = null;
        }


        public bool IsPopup()
        {
            return false;
        }


        public bool IsSignIn()
        {
            return App.ApplicationSettings.Contains(GOOGLE_DRIVE_SIGN_IN_KEY);
        }


        public string GetStorageName()
        {
            return AppResources.GoogleDrive;
        }


        public string GetStorageImageUri()
        {
            return GOOGLE_DRIVE_IMAGE_URI;
        }


        public string GetStorageColorHexString()
        {
            return GOOGLE_DRIVE_COLOR_HEX_STRING;
        }


        public async Task<StorageAccount> GetStorageAccountAsync()
        {
            try
            {
                if (this.CurrentAccount == null)
                    await TaskHelper.WaitSignInTask(this.GetStorageName());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this.CurrentAccount;
        }


        // Get a root folder.
        public async Task<FileObject> GetRootFolderAsync()
        {
            FileObject rootFile = new FileObject();
            AboutResource aboutResource = this.Service.About;
            About about = await aboutResource.Get().ExecuteAsync();
            rootFile.Id = about.RootFolderId;
            this.RootFodlerId = about.RootFolderId;
            rootFile.Name = "";
            return rootFile;
        }


        public async Task<List<FileObject>> GetRootFilesAsync()
        {
            FileList fileList = await this.Service.Files.List().ExecuteAsync();
            List<FileObject> childList = new List<FileObject>();
            foreach (Google.Apis.Drive.v2.Data.File file in fileList.Items)
            {
                Debug.WriteLine(file.Title);
                if (this._IsRoot(file) && this._IsValidFile(file))
                    childList.Add(ConvertToFileObjectHelper.ConvertToFileObject(file));
            }
            return childList;
        }


        public async Task<FileObject> GetFileAsync(string fileId)
        {
            Google.Apis.Drive.v2.Data.File file = await this.Service.Files.Get(fileId).ExecuteAsync();
            if (this._IsValidFile(file))
                return ConvertToFileObjectHelper.ConvertToFileObject(file);
            return null;
        }


        public async Task<List<FileObject>> GetFilesFromFolderAsync(string folderId)
        {
            List<FileObject> list = new List<FileObject>();
            
            // Get childern by q
            FilesResource.ListRequest listRequest = this.Service.Files.List();
            listRequest.Q = "'" + folderId + "' in parents and trashed=false and mimeType != 'application/vnd.google-apps.form' and 'me' in owners";
            FileList fileList = await listRequest.ExecuteAsync();
            foreach (Google.Apis.Drive.v2.Data.File file in fileList.Items)
                list.Add(ConvertToFileObjectHelper.ConvertToFileObject(file));
            list.RemoveAll(item => item == null);
            return list;
        }


        public async Task<Stream> DownloadFileStreamAsync(string fileId)
        {
            byte[] inarray = await this.Service.HttpClient.GetByteArrayAsync(fileId);
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

                string extension = fileName.Split('.').Last();
                var insert = this.Service.Files.Insert(file, inputStream, GoogleDriveManager.MimeTypeMapper[extension]);
                var task = await insert.UploadAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }


        public async Task<FileObject> Synchronize()
        {
            FileObject fileObject = await GetRootFolderAsync();
            fileObject.FileList = await _GetChildAsync(fileObject);

            //await Test(fileObject);
            return fileObject;
        }



        #region Private Methods
        private async Task<List<FileObject>> _GetChildAsync(FileObject fileObject)
        {
            if (FileObjectViewModel.FOLDER.Equals(fileObject.Type.ToString()))
            {
                List<FileObject> list = await this.GetFilesFromFolderAsync(fileObject.Id);
                foreach (FileObject file in list)
                    file.FileList = await _GetChildAsync(file);
                return list;
            }
            else
            {
                return null;
            }
        }

        
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
                              .ToArray()
                );
                App.ApplicationSettings[GOOGLE_DRIVE_USER_KEY] = result;
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
                result &= parent.IsRoot.Value;
            return result;
        }


        private bool _IsMine(Google.Apis.Drive.v2.Data.File file)
        {
            bool result = true;
            IList<User> owners = file.Owners;
            
            foreach (User user in owners)
            {
                // TODO Get that values from converted account.
                result &= ((this.User.DisplayName.Equals(user.DisplayName)) && user.IsAuthenticatedUser.Value);
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


        public async Task<FileObject> Test(FileObject root)
        {
            List<FileObject> list = new List<FileObject>();
            Dictionary<string, FileObject> folderMap = new Dictionary<string, FileObject>();
            List<Google.Apis.Drive.v2.Data.File> unhandled = new List<Google.Apis.Drive.v2.Data.File>();

            // Get childern by q
            FilesResource.ListRequest rr = this.Service.Files.List();
            rr.Q = "'root' in parents and trashed=false and mimeType != 'application/vnd.google-apps.form' and 'me' in owners";
            FileList fileList = await rr.ExecuteAsync();
            root.FileList = new List<FileObject>();
            folderMap.Add(root.Id, root);
            foreach (Google.Apis.Drive.v2.Data.File file in fileList.Items)
            {
                System.Diagnostics.Debug.WriteLine(file.Title);
                if (file.Title.Equals("Taylor")) System.Diagnostics.Debugger.Break();
                if (file.MimeType.Contains("application/vnd.google-apps.folder"))
                {
                    folderMap.Add(file.Id, ConvertToFileObjectHelper.ConvertToFileObject(file));
                }

                if (file.Parents != null && file.Parents.Count == 1)
                {

                    if (folderMap.ContainsKey(file.Parents[0].Id))
                    {
                        folderMap[file.Parents[0].Id].FileList.Add(ConvertToFileObjectHelper.ConvertToFileObject(file));
                        if ((file.Parents[0].IsRoot.Value)) System.Diagnostics.Debug.WriteLine("Root Parent!");
                    }
                    else
                    {
                        // for later work.
                        unhandled.Add(file);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("no parent : " + file.Title);
                    //System.Diagnostics.Debugger.Break();
                }
            }


            // TODO : Handle Unhandled files

            foreach (var file in unhandled)
            {
                if (folderMap.ContainsKey(file.Parents[0].Id))
                {
                    folderMap[file.Parents[0].Id].FileList.Add(ConvertToFileObjectHelper.ConvertToFileObject(file));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("unhandled : " + file.Title);
                    //System.Diagnostics.Debugger.Break();
                }
            }
            return root;
        }
        #endregion
    }
}
