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
    public class CloudSkyDriveAccountWorker
    {
        /*** Public ***/

        // Login with single sign on way.
        public async Task<Account> SignInSkyDriveAccountSingleSignOnAsync(LiveConnectClient liveClient, dynamic profileResult)
        {
            Account account = null;
            try
            {
                // Login to mobile service for getting access to DB
                await App.MobileService.LoginWithMicrosoftAccountAsync(liveClient.Session.AuthenticationToken);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            // If it success to get access to mobile service, 
            // Make final account
            if (App.MobileService.CurrentUser != null)
            {
                // Check duplication.
                // Insert if it is not exists already in DB,
                // Otherwise update account.
                account = await this.IsExistedPerson(App.MobileService.CurrentUser.UserId);

                if (account == null)  // First Login.
                {
                    account = new Account(App.MobileService.CurrentUser.UserId, GlobalKeys.MICROSOFT, "" + profileResult.name,
                        "" + profileResult.first_name, "" + profileResult.last_name, "" + profileResult.locale,
                        App.MobileService.CurrentUser.MobileServiceAuthenticationToken, 0, AccountType.NORMAL_ACCOUNT_TYPE);
                    try
                    {
                        await App.MobileService.GetTable<Account>().InsertAsync(account);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }

                else  // Second or more Login.
                {
                    // Get new account information.
                    account.account_platform_id = App.MobileService.CurrentUser.UserId;
                    account.account_name = profileResult.name;
                    account.account_first_name = profileResult.first_name;
                    account.account_last_name = profileResult.last_name;
                    account.account_locale = profileResult.locale;
                    account.account_token = App.MobileService.CurrentUser.MobileServiceAuthenticationToken;
                    try
                    {
                        await App.MobileService.GetTable<Account>().UpdateAsync(account);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }

                // If it success to insert account to DB,
                // Save it's information to isolated storage.
                this.SaveProfileReslutToAppSettings(account);
            }
            return account;
        }


        // Register Live Connect Session for Live Profile
        public async Task<LiveConnectClient> GetLiveConnectClientAsync()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
            string[] scopes = new[] { "wl.basic", "wl.offline_access", "wl.skydrive", "wl.skydrive_update", "wl.signin", "wl.contacts_skydrive" };
            LiveLoginResult liveLoginResult = null;

            // Get Current live connection session
            try
            {
                liveLoginResult = await liveAuthClient.InitializeAsync(scopes);
            }
            catch (LiveAuthException)
            {
                return null;
            }

            // If session doesn't exist, get new one.
            // Otherwise, get the session.
            if (liveLoginResult.Status != LiveConnectSessionStatus.Connected)
            {
                try
                {
                    liveLoginResult = await liveAuthClient.LoginAsync(scopes);
                }
                catch (LiveAuthException)
                {
                    return null;
                }
            }

            // Get Client using session which we get above
            return new LiveConnectClient(liveLoginResult.Session);
        }


        // Get User Profile information result using registered live connection session
        public async Task<dynamic> GetProfileResultAsync(LiveConnectClient liveClient)
        {
            dynamic result = null;
            try
            {
                LiveOperationResult operationResult = await liveClient.GetAsync("me");
                result = operationResult.Result;
            }
            catch (LiveConnectException)
            {
            }
            return result;
        }


        // Get out connection session
        public void SignOut()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
            liveAuthClient.Logout();
            this.RemoveProfileReslutFromAppSettings();
        }



        /*** private ***/

        // Check whether it exists in DB
        private async Task<Account> IsExistedPerson(string account_platform_id)
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
        private void SaveProfileReslutToAppSettings(Account account)
        {
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
        private void RemoveProfileReslutFromAppSettings()
        {
            App.ApplicationSettings.Remove(Account.ACCOUNT_SKY_DRIVE_IS_LOGIN);
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
