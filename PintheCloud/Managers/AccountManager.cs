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
        private string PTCACCOUNT_PW = "PTCACCOUNT_PW";

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

        public void SavePtcId(string email, string password)
        {
            App.ApplicationSettings[PTCACCOUNT_ID] = email;
            App.ApplicationSettings[PTCACCOUNT_PW] = password;
            App.ApplicationSettings.Save();
        }

        public async Task<bool> CreateNewPtcAccountAsync(PtcAccount account)
        {
            MSPtcAccount mspa = PtcAccount.ConvertToMSPtcAccount(account);
            //List<MSStorageAccount> saList = new List<MSStorageAccount>();
            //foreach (var i in account.StorageAccount)
            //{
            //    saList.Add(this.ConvertToMSStorageAccount(i.Value));
            //}
            try
            {
                PtcAccount p = await this.GetPtcAccountAsync(account.Email);
                if (p != null) return false;

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
            this.SavePtcId(account.Email, account.ProfilePassword);
            this.myAccount = account;
            return true;
        }
        public async Task<bool> DeletePtcAccount(PtcAccount account)
        {
            MSPtcAccount mspa = PtcAccount.ConvertToMSPtcAccount(account);
            List<MSStorageAccount> saList = new List<MSStorageAccount>();
            foreach (var i in account.StorageAccount)
            {
                saList.Add(StorageAccount.ConvertToMSStorageAccount(i.Value));
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
            MSPtcAccount mspa = PtcAccount.ConvertToMSPtcAccount(account);
            List<MSStorageAccount> saList = new List<MSStorageAccount>();
            foreach (var i in account.StorageAccount)
            {
                saList.Add(StorageAccount.ConvertToMSStorageAccount(i.Value));
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
        public async Task<PtcAccount> GetPtcAccountAsync(string accountId, string password = null)
        {
            System.Linq.Expressions.Expression<Func<MSPtcAccount, bool>> lamda = (a => a.email == accountId);
            if(password != null)
                 lamda = (a => a.email == accountId || a.profile_password == password);
            
            MobileServiceCollection<MSPtcAccount, MSPtcAccount> msAccounts = null;
            try
            {
                msAccounts = await App.MobileService.GetTable<MSPtcAccount>()
                    .Where(lamda)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
                throw new Exception("AccountManager.GetAccount() ERROR");
            }

            if (msAccounts.Count == 1)
            {
                PtcAccount account = PtcAccount.ConvertToPtcAccount(msAccounts.First());
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
                map.Add(i.account_platform_id_type, StorageAccount.ConvertToStorageAccount(i));
            }
            return map;
        }
        #endregion
    }
}
