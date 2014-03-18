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
        public IDictionary<string, StorageAccount> StorageAccounts { get; set; }
        public IDictionary<string,string> token { get; set; }

        public PtcAccount()
        {
            StorageAccounts = new Dictionary<string, StorageAccount>();
            token = new Dictionary<string, string>();
            AccountType = new StorageAccountType();
        }

        public async Task<bool> CreateStorageAccountAsync(StorageAccount sa)
        {
            MSStorageAccount mssa = StorageAccount.ConvertToMSStorageAccount(sa);
            mssa.ptc_account_id = this.Email;
            try
            {
                await App.MobileService.GetTable<MSStorageAccount>().InsertAsync(mssa);
                StorageAccounts.Add(sa.StorageName, sa);
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }
        private async Task<StorageAccount> GetStorageAccountAsync(string storageAccountId)
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

        public StorageAccount GetStorageAccountById(string storageAccountId)
        {
            if(storageAccountId == null || string.Empty.Equals(storageAccountId)) return null;
            using (var itr = StorageAccounts.GetEnumerator())
            {
                while (itr.MoveNext())
                {
                    if (storageAccountId.Equals(itr.Current.Value.Id))
                        return itr.Current.Value;
                }
            }
            return null;
        }

        public StorageAccount GetStorageAccountByName(string storageName)
        {
            if (StorageAccounts.ContainsKey(storageName))
                return StorageAccounts[storageName];
            else
                return null;
        }
        public IEnumerator<KeyValuePair<string, StorageAccount>> GetStorageAccountEnumerator()
        {
            return StorageAccounts.GetEnumerator();
        }

        public static MSPtcAccount ConvertToMSPtcAccount(PtcAccount pa)
        {
            MSPtcAccount mspa = new MSPtcAccount(pa.Name, pa.Email, pa.PhoneNumber, pa.ProfilePassword, 0.0, pa.AccountType.Id, "for_later_develop");
            return mspa;
        }

        public static PtcAccount ConvertToPtcAccount(MSPtcAccount mspa)
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
}
