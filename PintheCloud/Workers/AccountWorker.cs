using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PintheCloud.Workers
{
    public abstract class AccountWorker
    {
        /*** Abstract ***/

        public abstract Task<LiveConnectSession> GetLiveConnectSessionAsync();
        public abstract Task<dynamic> GetProfileResultAsync(LiveConnectSession session);
        public abstract Task<Account> LoginMicrosoftAccountSingleSignOnAsync(LiveConnectSession session, dynamic profileResult);
        


        /*** Public ***/

        // Get out connection session
        public void Logout()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
            liveAuthClient.Logout();
            this.RemoveProfileReslutFromAppSettings();
        }



        /*** Protected ***/

        // Check whether it exists in DB
        protected async Task<Account> IsExistedPerson(string account_platform_id)
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
            App.ApplicationSettings[Account.ACCOUNT_IS_LOGIN] = true;
            App.ApplicationSettings[Account.ACCOUNT_PLATFORM_ID] = account.account_platform_id;
            App.ApplicationSettings[Account.ACCOUNT_PLATFORM_ID_TYPE] = account.account_platform_id_type;
            App.ApplicationSettings[Account.ACCOUNT_NAME] = account.account_name;
            App.ApplicationSettings[Account.ACCOUNT_FIRST_NAME] = account.account_first_name;
            App.ApplicationSettings[Account.ACCOUNT_LAST_NAME] = account.account_last_name;
            App.ApplicationSettings[Account.ACCOUNT_LOCAL] = account.account_locale;
            App.ApplicationSettings[Account.ACCOUNT_TOKEN] = account.account_token;
            App.ApplicationSettings[Account.ACCOUNT_USED_SIZE] = account.account_used_size;
            App.ApplicationSettings[Account.ACCOUNT_TYPE_NAME] = account.account_type_name;
            App.ApplicationSettings.Save();
        }


        // Save profile information to local isolated App settings.
        protected void RemoveProfileReslutFromAppSettings()
        {
            App.ApplicationSettings.Remove(Account.ACCOUNT_IS_LOGIN);
            App.ApplicationSettings.Remove(Account.ACCOUNT_PLATFORM_ID);
            App.ApplicationSettings.Remove(Account.ACCOUNT_PLATFORM_ID_TYPE);
            App.ApplicationSettings.Remove(Account.ACCOUNT_NAME);
            App.ApplicationSettings.Remove(Account.ACCOUNT_FIRST_NAME);
            App.ApplicationSettings.Remove(Account.ACCOUNT_LAST_NAME);
            App.ApplicationSettings.Remove(Account.ACCOUNT_LOCAL);
            App.ApplicationSettings.Remove(Account.ACCOUNT_TOKEN);
            App.ApplicationSettings.Remove(Account.ACCOUNT_USED_SIZE);
            App.ApplicationSettings.Remove(Account.ACCOUNT_TYPE_NAME);
            App.ApplicationSettings.Remove(Account.LOCATION_ACCESS);
        }
    }
}
