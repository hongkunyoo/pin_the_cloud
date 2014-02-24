using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public static class AccountManager
    {

        public static async Task<Account> GetAccountAsync(string accountId)
        {
            MobileServiceCollection<Account, Account> accounts = null;
            try
            {
                accounts = await App.MobileService.GetTable<Account>()
                    .Where(a => a.account_platform_id == accountId)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
                throw new Exception("AccountManager.GetAccount() ERROR");
            }

            if (accounts.Count == 1)
                return accounts.First();
            else if (accounts.Count > 1)
                throw new Exception("AccountManager.GetAccount() ERROR");
            else
                return null;
        }
        public static async Task<bool> CreateAccountAsync(string accountId, string accountUserName, Account.StorageAccountType type)
        {
            Account account = new Account(accountId, type, accountUserName, 0, AccountType.NORMAL_ACCOUNT_TYPE);
            try
            {
                await App.MobileService.GetTable<Account>().InsertAsync(account);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            
            return true;
        }


        //private async Task<bool> IsExistedPerson(string accountId)
        //{
        //    MobileServiceCollection<Account, Account> accounts = null;
        //    try
        //    {
        //        accounts = await App.MobileService.GetTable<Account>()
        //            .Where(a => a.account_platform_id == accountId)
        //            .ToCollectionAsync();
        //    }
        //    catch (MobileServiceInvalidOperationException)
        //    {
        //        return false;
        //    }

        //    if (accounts.Count > 0)
        //        return true;
        //    else
        //        return false;
        //}
    }
}
