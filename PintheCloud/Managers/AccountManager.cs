using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PintheCloud.Managers
{
    public abstract class AccountManager
    {
        /*** Abstract ***/
        public abstract Task<bool> LoginMicrosoftSingleSignOnAsync();
        public abstract Task<bool> RegisterLiveConnectionSessionAsync();


        /*** Public ***/

        // Get out connection session
        public void Logout()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
            liveAuthClient.Logout();
            this.RemoveProfileReslutFromAppSettings();
        }



        /*** Protected ***/

        // Get User Profile information result using registered live connection session
        protected async Task<dynamic> GetProfileResultAsync()
        {
            dynamic result = null;
            try
            {
                LiveConnectClient liveClient = new LiveConnectClient(App.Session);
                LiveOperationResult operationResult = await liveClient.GetAsync("me");
                result = operationResult.Result;
            }
            catch (LiveConnectException)
            {
            }
            return result;
        }


        // Check whether it exists in DB
        protected async Task<Account> isExistedPerson(string account_platform_id)
        {
            MobileServiceCollection<Account, Account> accounts = null;
            try
            {
                accounts = await App.MobileService.GetTable<Account>()
                    .Where(a => a.account_platform_id == account_platform_id)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException)
            {
            }

            if (accounts.Count > 0)
                return accounts.First();
            else
                return null;
        }


        // Save profile information to local isolated App settings.
        protected void SaveProfileReslutToAppSettings(Account account)
        {
            App.ApplicationSettings.Add(Account.ACCOUNT_IS_LOGIN, true);
            App.ApplicationSettings.Add(Account.ACCOUNT_PLATFROM_ID, account.account_platform_id);
            App.ApplicationSettings.Add(Account.ACCOUNT_PLATFROM_ID_TYPE, account.account_platform_id_type);
            App.ApplicationSettings.Add(Account.ACCOUNT_NAME, account.account_name);
            App.ApplicationSettings.Add(Account.ACCOUNT_FIRST_NAME, account.account_first_name);
            App.ApplicationSettings.Add(Account.ACCOUNT_LAST_NAME, account.account_last_name);
            App.ApplicationSettings.Add(Account.ACCOUNT_LOCAL, account.account_locale);
            App.ApplicationSettings.Add(Account.ACCOUNT_TOKEN, account.account_token);
            App.ApplicationSettings.Add(Account.ACCOUNT_USED_SIZE, account.account_used_size);
            App.ApplicationSettings.Add(Account.ACCOUNT_TYPE_NAME, account.account_type_name);
        }


        // Save profile information to local isolated App settings.
        protected void RemoveProfileReslutFromAppSettings()
        {
            App.ApplicationSettings.Remove(Account.ACCOUNT_IS_LOGIN);
            App.ApplicationSettings.Remove(Account.ACCOUNT_PLATFROM_ID);
            App.ApplicationSettings.Remove(Account.ACCOUNT_PLATFROM_ID_TYPE);
            App.ApplicationSettings.Remove(Account.ACCOUNT_NAME);
            App.ApplicationSettings.Remove(Account.ACCOUNT_FIRST_NAME);
            App.ApplicationSettings.Remove(Account.ACCOUNT_LAST_NAME);
            App.ApplicationSettings.Remove(Account.ACCOUNT_LOCAL);
            App.ApplicationSettings.Remove(Account.ACCOUNT_TOKEN);
            App.ApplicationSettings.Remove(Account.ACCOUNT_USED_SIZE);
            App.ApplicationSettings.Remove(Account.ACCOUNT_TYPE_NAME);
        }



        /*** Private ***/

    }
}
