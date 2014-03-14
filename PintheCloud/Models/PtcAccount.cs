using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using PintheCloud.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class PtcAccount
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePassword { get; set; }
        public double UsedSize { get; set; }
        public StorageAccountType AccountType { get; set; }
        public IDictionary<string, StorageAccount> StorageAccount { get; set; }
        public IDictionary<string,string> token { get; set; }

        public PtcAccount()
        {
            StorageAccount = new Dictionary<string, StorageAccount>();
            token = new Dictionary<string, string>();
            AccountType = new StorageAccountType();
        }

        public async Task<bool> CreateStorageAccountAsync(StorageAccount sa)
        {
            MSStorageAccount mssa = App.AccountManager.ConvertToMSStorageAccount(sa);
            mssa.ptc_account_id = this.Email;
            try
            {
                await App.MobileService.GetTable<MSStorageAccount>().InsertAsync(mssa);
                StorageAccount.Add(sa.StorageName, sa);
                return true;
            }
            catch(Exception ex)
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
                return App.AccountManager.ConvertToStorageAccount(accounts.First());
            else if (accounts.Count > 1)
                throw new Exception("AccountManager.GetAccount() ERROR");
            else
                return null;
        }
        public StorageAccount GetStorageAccount(string storageName)
        {
            if (StorageAccount.ContainsKey(storageName))
                return StorageAccount[storageName];
            else
                return null;
        }
    }
}
