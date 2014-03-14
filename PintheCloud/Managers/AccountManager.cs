using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using PintheCloud.Helpers;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public class AccountManager
    {
        public PtcAccount myAccount { get; set; }
        private string PTCACCOUNT_ID = "PTCACCOUNT_ID";

        public bool IsSignIn()
        {
            return App.ApplicationSettings.Contains(PTCACCOUNT_ID);
        }
        public string GetPtcId()
        {
            return (string)App.ApplicationSettings[PTCACCOUNT_ID];
        }

        public async Task<bool> SignIn()
        {
            if (!this.IsSignIn()) return false;

            await this.GetPtcAccountAsync();

            using (var itr = StorageHelper.GetStorageEnumerator())
            {
                while (itr.MoveNext())
                {
                    if (itr.Current.IsSignIn())
                    {
                        App.TaskHelper.AddSignInTask(itr.Current.GetStorageName(), itr.Current.SignIn());
                    }
                }
            }
            return true;
        }

        public async Task<bool> CreateNewPtcAccountAsync(PtcAccount account)
        {
            MSPtcAccount mspa = this.ConvertToMSPtcAccount(account);
            //List<MSStorageAccount> saList = new List<MSStorageAccount>();
            //foreach (var i in account.StorageAccount)
            //{
            //    saList.Add(this.ConvertToMSStorageAccount(i.Value));
            //}
            try
            {
                //PtcAccount p = await this.GetPtcAccountAsync(account.Email);
                //if (p != null) return false;

                await App.MobileService.GetTable<MSPtcAccount>().InsertAsync(mspa);
                //foreach (var i in saList)
                //{
                //    await App.MobileService.GetTable<MSStorageAccount>().InsertAsync(i);
                //}
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            App.ApplicationSettings[PTCACCOUNT_ID] = account.Email;
            App.ApplicationSettings.Save();
            this.myAccount = account;
            return true;
        }
        public async Task<bool> DeletePtcAccount(PtcAccount account)
        {
            MSPtcAccount mspa = this.ConvertToMSPtcAccount(account);
            List<MSStorageAccount> saList = new List<MSStorageAccount>();
            foreach (var i in account.StorageAccount)
            {
                saList.Add(this.ConvertToMSStorageAccount(i.Value));
            }
            try
            {
                await App.MobileService.GetTable<MSPtcAccount>().DeleteAsync(mspa);
                foreach (var i in saList)
                {
                    await App.MobileService.GetTable<MSStorageAccount>().DeleteAsync(i);
                }
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            App.ApplicationSettings.Remove(PTCACCOUNT_ID);
            App.ApplicationSettings.Save();
            this.myAccount = null;
            return true;
        }
        public async Task<bool> UpdatePtcAccount(PtcAccount account)
        {
            MSPtcAccount mspa = this.ConvertToMSPtcAccount(account);
            List<MSStorageAccount> saList = new List<MSStorageAccount>();
            foreach (var i in account.StorageAccount)
            {
                saList.Add(this.ConvertToMSStorageAccount(i.Value));
            }
            try
            {
                await App.MobileService.GetTable<MSPtcAccount>().UpdateAsync(mspa);
                foreach (var i in saList)
                {
                    await App.MobileService.GetTable<MSStorageAccount>().UpdateAsync(i);
                }
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            this.myAccount = account;
            return true;
        }
        public async Task<bool> GetPtcAccountAsync()
        {
            if (App.ApplicationSettings.Contains(PTCACCOUNT_ID))
            {
                await this.GetPtcAccountAsync((string)App.ApplicationSettings[PTCACCOUNT_ID]);
                return true;
            }
            else
                return false;
        }
        public PtcAccount GetPtcAccount()
        {
            return this.myAccount;
        }
        private async Task<PtcAccount> GetPtcAccountAsync(string accountId)
        {
            MobileServiceCollection<MSPtcAccount, MSPtcAccount> msAccounts = null;
            try
            {
                msAccounts = await App.MobileService.GetTable<MSPtcAccount>()
                    .Where(a => a.email == accountId)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
                throw new Exception("AccountManager.GetAccount() ERROR");
            }

            if (msAccounts.Count == 1)
            {
                PtcAccount account = this.ConvertToPtcAccount(msAccounts.First());
                account.StorageAccount = await this.GetStorageAccountsAsync(account.Email);
                this.myAccount = account;
                return account;
            }
            else if (msAccounts.Count > 1)
                throw new Exception("AccountManager.GetAccount() ERROR");
            else
                return null;
        }

       #region Private Methods
        private async Task<Dictionary<string, StorageAccount>> GetStorageAccountsAsync(String ptc_account_id)
        {
            Dictionary<string, StorageAccount> map = new Dictionary<string,StorageAccount>();
            MobileServiceCollection<MSStorageAccount, MSStorageAccount> sas = null;
            try
            {
                sas = await App.MobileService.GetTable<MSStorageAccount>()
                    .Where(a => a.ptc_account_id == ptc_account_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
                throw new Exception("AccountManager.GetAccount() ERROR");
            }

            foreach(var i in sas)
            {
                map.Add(i.account_platform_id_type, this.ConvertToStorageAccount(i));
            }
            return map;
        }
       

        public MSStorageAccount ConvertToMSStorageAccount(StorageAccount sa)
        {
            MSStorageAccount mssa = new MSStorageAccount(sa.Id, sa.StorageName, sa.UserName ,sa.UsedSize);
            return mssa;
        }
        public MSPtcAccount ConvertToMSPtcAccount(PtcAccount pa)
        {
            MSPtcAccount mspa = new MSPtcAccount(pa.Name, pa.Email, pa.PhoneNumber, pa.ProfilePassword, 0.0, pa.AccountType.Id, "for_later_develop");
            return mspa;
        }
        public MSAccountType ConvertToMSAccountType(StorageAccountType sat)
        {
            MSAccountType msat = new MSAccountType();
            msat.account_type_id = sat.Id;
            msat.account_type_name = sat.AccountTypeName;
            msat.account_type_max_size = sat.MaxSize;
            return msat;
        }
        public StorageAccount ConvertToStorageAccount(MSStorageAccount mssa)
        {
            StorageAccount sa = new StorageAccount();
            sa.Id = mssa.account_platform_id;
            sa.StorageName = mssa.account_platform_id_type;
            sa.UserName = mssa.account_name;
            sa.UsedSize = mssa.account_used_size;
            return sa;
        }
        private StorageAccountType ConvertToStorageAccountType(MSAccountType msat)
        {
            StorageAccountType sat = new StorageAccountType();
            sat.Id = msat.account_type_id;
            sat.AccountTypeName = msat.account_type_name;
            sat.MaxSize = msat.account_type_max_size;
            return sat;    
        }
        private PtcAccount ConvertToPtcAccount(MSPtcAccount mspa)
        {
            PtcAccount account = new PtcAccount();
            account.Name = mspa.name;
            account.Email = mspa.email;
            account.PhoneNumber = mspa.phone_number;
            account.ProfilePassword = mspa.profile_password;
            account.UsedSize = mspa.used_size;
            account.AccountType = new StorageAccountType();
            return account;
        }
        #endregion
    }

    #region Mobile Service Models
    /// <summary>
    /// Mobile Service Storage Account
    /// This class will be stored in the mobile service table
    /// </summary>
    public class MSStorageAccount
    {
        public enum StorageAccountType { ONE_DRIVE, DROPBOX, GOOGLE_DRIVE }
        public const string ACCOUNT_MAIN_PLATFORM_TYPE_KEY = "ACCOUNT_MAIN_PLATFORM_TYPE_KEY";
        public const string LOCATION_ACCESS_CONSENT_KEY = "LOCATION_ACCESS_CONSENT_KEY";
        public const string ACCOUNT_DEFAULT_SPOT_NAME_KEY = "ACCOUNT_DEFAULT_SPOT_NAME_KEY";


        public string id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id")]
        public string account_platform_id { get; set; }

        [JsonProperty(PropertyName = "account_platform_id_type")]
        public string account_platform_id_type { get; set; }

        [JsonProperty(PropertyName = "account_name")]
        public string account_name { get; set; }

        [JsonProperty(PropertyName = "account_used_size")]
        public double account_used_size { get; set; }
        [JsonProperty(PropertyName = "ptc_account_id")]
        public string ptc_account_id { get; set; }

        public MSStorageAccount(string account_platform_id, string account_platform_id_type, string account_name, 
            double account_used_size)
        {
            this.account_platform_id = account_platform_id;
            this.account_platform_id_type = account_platform_id_type;
            this.account_name = account_name;
            this.account_used_size = account_used_size;
            this.ptc_account_id = "";
        }
    }
    /// <summary>
    /// Mobile Service AccountType
    /// </summary>
    public class MSAccountType
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "account_type_name")]
        public string account_type_name { get; set; }

        [JsonProperty(PropertyName = "account_type_max_size")]
        public double account_type_max_size { get; set; }
        [JsonProperty(PropertyName = "account_type_id")]
        public string account_type_id { get; set; }

    }
    /// <summary>
    /// Mobile Service PtcAccount
    /// This class will be stored in the mobile service table
    /// </summary>
    public class MSPtcAccount
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string email { get; set; }

        [JsonProperty(PropertyName = "phone_number")]
        public string phone_number { get; set; }

        [JsonProperty(PropertyName = "profile_password")]
        public string profile_password { get; set; }

        [JsonProperty(PropertyName = "used_size")]
        public double used_size { get; set; } 

        [JsonProperty(PropertyName = "account_type_id")]
        public string account_type_id { get; set; }

        [JsonProperty(PropertyName = "token_id")]
        public string token_id { get; set; }

        public MSPtcAccount(string name, string email, string phone_number, string profile_password, double used_size, string account_type_id, string token_id)
        {
            this.name = name;
            this.email = email;
            this.phone_number = phone_number;
            this.profile_password = profile_password;
            this.used_size = used_size;
            this.account_type_id = account_type_id;
            this.token_id = token_id;
        }
    }
    #endregion
}
