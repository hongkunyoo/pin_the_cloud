using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using PintheCloud.Helpers;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (!App.ApplicationSettings.Contains(PTCACCOUNT_ID))
                System.Diagnostics.Debugger.Break();

            return (string)App.ApplicationSettings[PTCACCOUNT_ID];
        }

        public async Task<bool> SignIn()
        {
            if (!this.IsSignIn()) return false;

            PtcAccount account = await this.GetPtcAccountAsync();
            if (account == null) return false;
            this.myAccount = account;
            return true;
        }

        public void SignOut()
        {
            App.ApplicationSettings.Remove(PTCACCOUNT_ID);
            App.ApplicationSettings.Save();
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
            try
            {
                PtcAccount p = await this.GetPtcAccountAsync(account.Email);
                if (p != null) return false;
                await App.MobileService.GetTable<MSPtcAccount>().InsertAsync(mspa);
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
            //List<MSStorageAccount> saList = new List<MSStorageAccount>();
            //foreach (var i in account.StorageAccounts)
            //{
            //    saList.Add(StorageAccount.ConvertToMSStorageAccount(i.Value));
            //}
            try
            {
                await App.MobileService.GetTable<MSPtcAccount>().DeleteAsync(mspa);
                //foreach (var i in saList)
                //{
                //    await App.MobileService.GetTable<MSStorageAccount>().DeleteAsync(i);
                //}
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
            //foreach (var i in account.StorageAccounts)
            //{
            //    saList.Add(StorageAccount.ConvertToMSStorageAccount(i.Value));
            //}
            try
            {
                await App.MobileService.GetTable<MSPtcAccount>().UpdateAsync(mspa);
                //foreach (var i in saList)
                //{
                //    await App.MobileService.GetTable<MSStorageAccount>().UpdateAsync(i);
                //}
            }
            catch (MobileServiceInvalidOperationException)
            {
                return false;
            }
            this.myAccount = account;
            return true;
        }
        public async Task<PtcAccount> GetPtcAccountAsync()
        {
            TaskCompletionSource<PtcAccount> tcs = new TaskCompletionSource<PtcAccount>();
            if (this.myAccount == null)
            {
                if (App.ApplicationSettings.Contains(PTCACCOUNT_ID))
                {
                    PtcAccount account = await this.GetPtcAccountAsync((string)App.ApplicationSettings[PTCACCOUNT_ID]);
                    if (account == null)
                    {
                        tcs.SetResult(null);
                        return tcs.Task.Result;
                    }
                    tcs.SetResult(account);
                    return tcs.Task.Result;
                }
                else
                {
                    tcs.SetResult(null);
                    return tcs.Task.Result;
                }
            }
            else
            {
                tcs.SetResult(this.myAccount);
                return tcs.Task.Result;
            }
        }


        public async Task<PtcAccount> GetPtcAccountAsync(string accountId, string password = null)
        {
            System.Linq.Expressions.Expression<Func<MSPtcAccount, bool>> lamda = (a => a.email == accountId);
            if(password != null)
                 lamda = (a => a.email == accountId && a.profile_password == password);

            MobileServiceCollection<MSPtcAccount, MSPtcAccount> list = null;
            try
            {
                list = await App.MobileService.GetTable<MSPtcAccount>()
                    .Where(lamda)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
                throw new Exception("AccountManager.GetAccount() ERROR");
            }

            if (list.Count == 1)
            {
                PtcAccount account = PtcAccount.ConvertToPtcAccount(list.First());
                this.myAccount = account;
                return account;
            }
            else if (list.Count > 1)
                throw new Exception("AccountManager.GetAccount() ERROR");
            else
                return null;
        }


        public async Task<bool> CreateStorageAccountAsync(StorageAccount sa)
        {
            MSStorageAccount mssa = StorageAccount.ConvertToMSStorageAccount(sa);
            mssa.ptc_account_id = GetPtcId();
            try
            {
                await App.MobileService.GetTable<MSStorageAccount>().InsertAsync(mssa);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        public async Task<StorageAccount> GetStorageAccountAsync(string storageAccountId)
        {
            MobileServiceCollection<MSStorageAccount, MSStorageAccount> accounts = null;
            try
            {
                accounts = await App.MobileService.GetTable<MSStorageAccount>()
                    .Where(a => a.account_platform_id == storageAccountId)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());
                throw new Exception("AccountManager.GetAccount() ERROR");
            }

            if (accounts.Count == 1)
                return StorageAccount.ConvertToStorageAccount(accounts.First());
            else if (accounts.Count > 1)
                throw new Exception("AccountManager.GetAccount() ERROR");
            else
                return null;
        }


       #region Private Methods

        //private async Task<Dictionary<string, StorageAccount>> GetStorageAccountsAsync(String ptc_account_id)
        //{
        //    Dictionary<string, StorageAccount> map = new Dictionary<string,StorageAccount>();
        //    MobileServiceCollection<MSStorageAccount, MSStorageAccount> sas = null;
        //    try
        //    {
        //        sas = await App.MobileService.GetTable<MSStorageAccount>()
        //            .Where(a => a.ptc_account_id == ptc_account_id)
        //            .ToCollectionAsync();
        //    }
        //    catch (MobileServiceInvalidOperationException)
        //    {
        //        throw new Exception("AccountManager.GetAccount() ERROR");
        //    }

        //    foreach(var i in sas)
        //    {
        //        map.Add(i.account_platform_id_type, StorageAccount.ConvertToStorageAccount(i));
        //    }
        //    return map;
        //}
        #endregion
    }
}
