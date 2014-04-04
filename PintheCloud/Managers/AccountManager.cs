using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using PintheCloud.Helpers;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
                if (p != null) 
                    return false;
                await App.MobileService.GetTable<MSPtcAccount>().InsertAsync(mspa);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                throw ex;
            }

            this.SavePtcId(account.Email, account.ProfilePassword);
            this.myAccount = account;
            return true;
        }


        public async Task<bool> DeletePtcAccount(PtcAccount account)
        {
            MSPtcAccount mspa = PtcAccount.ConvertToMSPtcAccount(account);
            try
            {
                await App.MobileService.GetTable<MSPtcAccount>().DeleteAsync(mspa);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                throw ex;
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
            try
            {
                await App.MobileService.GetTable<MSPtcAccount>().UpdateAsync(mspa);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                throw ex;
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
                    try
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
                    catch (MobileServiceInvalidOperationException)
                    {
                        tcs.SetResult(null);
                        return tcs.Task.Result;
                    }
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
            Expression<Func<MSPtcAccount, bool>> lamda = (a => a.email == accountId);
            if(password != null)
                 lamda = (a => a.email == accountId && a.profile_password == password);

            MobileServiceCollection<MSPtcAccount, MSPtcAccount> list = null;
            try
            {
                list = await App.MobileService.GetTable<MSPtcAccount>()
                    .Where(lamda)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                throw ex;
            }

            if (list.Count >= 1)
            {
                PtcAccount account = PtcAccount.ConvertToPtcAccount(list.First());
                this.myAccount = account;
                return account;
            }
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
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                throw ex;
            }

            return true;
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
                throw ex;
            }

            if (accounts.Count >= 1)
                return StorageAccount.ConvertToStorageAccount(accounts.First());
            else
                return null;
        }
    }
}
