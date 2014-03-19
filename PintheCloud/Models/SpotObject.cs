using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using PintheCloud.Helpers;
using PintheCloud.Managers;
using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PintheCloud.Models
{
    public class SpotObject
    {
        public string Id { get; set; }
        public string SpotName { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; }
        public string PtcAccountId { get; set; }
        public string PtcAccountName { get; set; }
        public double SpotDistance { get; set; }
        public string Password { get; set; }
        public bool IsPrivate { get; set; }
        public string CreateAt { get; set; }

        private List<FileObject> fileObjectList;
        private List<ProfileObject> profileObjectList;
        private List<NoteObject> noteObjectList;


        public SpotObject()
        {

        }


        public SpotObject(string SpotName, double Latitude, double Longtitude, string PtcAccountId, string PtcAccountName, double SpotDistance, bool IsPrivate, string Password, string CreateAt)
        {
            this.SpotName = SpotName;
            this.Latitude = Latitude;
            this.Longtitude = Longtitude;
            this.PtcAccountId = PtcAccountId;
            this.PtcAccountName = PtcAccountName;
            this.SpotDistance = SpotDistance;
            this.Password = Password;
            this.IsPrivate = IsPrivate;
            this.CreateAt = CreateAt;
        }


        public FileObject GetFileObject(string fileObjectId)
        {
            if (fileObjectList == null) System.Diagnostics.Debugger.Break();
            using (var itr = fileObjectList.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    if (itr.Current.Id.Equals(fileObjectId))
                        return itr.Current;
                }
            }
            return null;
        }


        public ProfileObject GetProfileObject(string profileObjectId)
        {
            using (var itr = profileObjectList.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    if (itr.Current.Id.Equals(profileObjectId))
                        return itr.Current;
                }
            }
            return null;
        }


        public NoteObject GetNoteObject(string noteObjectId)
        {
            using (var itr = noteObjectList.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    if (itr.Current.Id.Equals(noteObjectId))
                        return itr.Current;
                }
            }
            return null;
        }


        #region Managing Contents Async Methods
        public async Task<bool> AddFileObjectAsync(FileObject fo)
        {
            /////////////////////////////////////////
            // TODO : Need to add Storage Capacity
            /////////////////////////////////////////
            try
            {
                IStorageManager StorageManager = Switcher.GetCurrentStorage();
                string sourceId = fo.Id;
                if (StorageManager.GetStorageName().Equals(AppResources.GoogleDrive))
                    sourceId = fo.DownloadUrl;
                await App.BlobStorageManager.UploadFileStreamAsync(this.PtcAccountId, this.Id, fo.Name, await StorageManager.DownloadFileStreamAsync(fo.Id));
                return true;
            }
            catch
            {
                return false;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////
            /// In the future, Maybe there will be codes for the mobile service table insertion. BUT Not NOW
            /// /////////////////////////////////////////////////////////////////////////////////////////////
        }

        public async Task<bool> DeleteFileObjectAsync(FileObject fo)
        {
            //////////////////////////////////////////////
            // TODO : Need to substract Storage Capacity
            //////////////////////////////////////////////

            return await App.BlobStorageManager.DeleteFileAsync(fo.Id);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /// In the future, Maybe there will be codes for the mobile service table insertion. BUT Not NOW
            /// /////////////////////////////////////////////////////////////////////////////////////////////
        }

        public async Task<List<FileObject>> ListFileObjectsAsync()
        {
            return await App.BlobStorageManager.GetFilesFromSpotAsync(this.PtcAccountId, this.Id);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            /// In the future, Maybe there will be codes for the mobile service table insertion. BUT Not NOW
            /// /////////////////////////////////////////////////////////////////////////////////////////////
        }

        public async Task<bool> PutProfileObjectAsync(ProfileObject po)
        {
            try
            {
                MSProfileObject mspo = po.ToMSObject();
                mspo.spot_id = this.Id;
                await App.MobileService.GetTable<MSProfileObject>().InsertAsync(mspo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ProfileObject>> ListProfileObjectsAsync()
        {
            List<ProfileObject> list = new List<ProfileObject>();
            MobileServiceCollection<MSProfileObject, MSProfileObject> msprofileList = null;
            try
            {
                msprofileList = await App.MobileService.GetTable<MSProfileObject>()
                     .Where(item => item.spot_id == this.Id)
                     .ToCollectionAsync();
            }
            catch
            {
                return null;
            }
            for (var i = 0; i < msprofileList.Count; i++)
            {
                list.Add(ProfileObject.ConvertToProfileObject(msprofileList[i]));
            }
            return list;
        }
        public async Task<bool> AddNoteObjectAsync(NoteObject no)
        {
            MSNoteObject msno = NoteObject.ConvertToMSNoteObject(no);
            msno.spot_id = this.Id;
            try
            {
                await App.MobileService.GetTable<MSNoteObject>().InsertAsync(msno);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddLogObjectAsync(LogObject log)
        {
            MSLogObject msno = LogObject.ConvertToMSLogObject(log);
            msno.spot_id = this.Id;
            try
            {
                await App.MobileService.GetTable<MSLogObject>().InsertAsync(msno);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<LogObject>> ListLogObjectsAsync()
        {
            List<LogObject> list = new List<LogObject>();
            MobileServiceCollection<MSLogObject, MSLogObject> mobileList = null;
            try
            {
                mobileList = await App.MobileService.GetTable<MSLogObject>()
                     .Where(item => item.spot_id == this.Id)
                     .ToCollectionAsync();
            }
            catch
            {
                return null;
            }
            for (var i = 0; i < mobileList.Count; i++)
            {
                list.Add(LogObject.ConvertToLogObject(mobileList[i]));
            }
            return list;
        }

        public async Task<bool> SaveLogObjectsAsync(string location)
        {
            List<LogObject> list = await ListLogObjectsAsync();

            /////////////////////////////////////////////////////////////
            // TODO : Need to Save to Cloud Storage With Certain Format
            /////////////////////////////////////////////////////////////

            return true;
        }
        #endregion

        public static MSSpotObject ConvertToMSSpotObject(SpotObject so)
        {
            return new MSSpotObject(so.SpotName, so.Latitude, so.Longtitude, so.PtcAccountId, so.PtcAccountName, so.SpotDistance, so.IsPrivate, so.Password, so.CreateAt);
        }


        public static SpotObject ConvertToSpotObject(MSSpotObject msso)
        {
            SpotObject so = new SpotObject();
            so.Id = msso.id;
            so.SpotName = msso.spot_name;
            so.Latitude = msso.spot_latitude;
            so.Longtitude = msso.spot_longtitude;
            so.PtcAccountId = msso.account_id;
            so.PtcAccountName = msso.account_name;
            so.SpotDistance = msso.spot_distance;
            so.Password = msso.spot_password;
            so.IsPrivate = msso.is_private;
            so.CreateAt = msso.create_at;
            return so;
        }
    }

    #region Mobile Service Model
    public class MSSpotObject
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "spot_name")]
        public string spot_name { get; set; }

        [JsonProperty(PropertyName = "spot_latitude")]
        public double spot_latitude { get; set; }

        [JsonProperty(PropertyName = "spot_longtitude")]
        public double spot_longtitude { get; set; }

        [JsonProperty(PropertyName = "ptcaccount_id")]
        public string account_id { get; set; }

        [JsonProperty(PropertyName = "ptcaccount_name")]
        public string account_name { get; set; }

        [JsonProperty(PropertyName = "spot_distance")]
        public double spot_distance { get; set; }

        [JsonProperty(PropertyName = "spot_password")]
        public string spot_password { get; set; }

        [JsonProperty(PropertyName = "is_private")]
        public bool is_private { get; set; }

        [JsonProperty(PropertyName = "create_at")]
        public string create_at { get; set; }

        public MSSpotObject(string spot_name, double spot_latitude, double spot_longtitude, 
            string account_id, string account_name, double spot_distance, bool is_private, string spot_password, string create_at = null)
        {
            this.spot_name = spot_name;
            this.spot_latitude = spot_latitude;
            this.spot_longtitude = spot_longtitude;
            this.account_id = account_id;
            this.account_name = account_name;
            this.spot_distance = spot_distance;
            this.is_private = is_private;
            this.spot_password = spot_password;
            if (create_at == null || string.Empty.Equals(create_at))
                create_at = DateTime.Now.ToString();
            this.create_at = create_at;
        }
    }
    #endregion
}
