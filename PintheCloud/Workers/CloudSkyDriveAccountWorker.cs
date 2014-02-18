using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using PintheCloud.Resources;
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
                this.SignOut();
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
                    account = new Account(App.MobileService.CurrentUser.UserId, App.PLATFORMS[App.SKY_DRIVE_KEY_INDEX], "" + profileResult.name,
                        "" + profileResult.first_name, "" + profileResult.last_name, "" + profileResult.locale,
                        App.MobileService.CurrentUser.MobileServiceAuthenticationToken, 0, AccountType.NORMAL_ACCOUNT_TYPE);
                    try
                    {
                        await App.MobileService.GetTable<Account>().InsertAsync(account);
                    }
                    catch (InvalidOperationException)
                    {
                        this.SignOut();
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
                        this.SignOut();
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
            LiveAuthClient liveAuthClient = new LiveAuthClient(App.AZURE_CLIENT_ID);
            string[] scopes = new[] { "wl.basic", "wl.offline_access", "wl.skydrive", "wl.skydrive_update", "wl.signin", "wl.contacts_skydrive" };
            LiveLoginResult liveLoginResult = null;

            // Get Current live connection session
            try
            {
                liveLoginResult = await liveAuthClient.InitializeAsync(scopes);
            }
            catch (LiveAuthException)
            {
                this.SignOut();
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
                    this.SignOut();
                    return null;
                }
            }

            // Get Client using session which we get above
            if (liveLoginResult.Session == null)
            {
                this.SignOut();
                return null;
            }
            else
            {
                return new LiveConnectClient(liveLoginResult.Session);
            }
        }


        // Get User Profile information result using registered live connection session
        public async Task<dynamic> GetProfileResultAsync(LiveConnectClient liveClient)
        {
            LiveOperationResult operationResult = null;
            try
            {
                operationResult = await liveClient.GetAsync("me");
            }
            catch (LiveConnectException)
            {
                this.SignOut();
                return null;
            }

            if (operationResult.Result == null)
            {
                this.SignOut();
                return null;
            }
            else 
            {
                return operationResult.Result;
            }
        }


        // Get out connection session
        public void SignOut()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(App.AZURE_CLIENT_ID);
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
            App.ApplicationSettings[Account.ACCOUNT_ID_KEYS[App.SKY_DRIVE_KEY_INDEX]] = account.account_platform_id;
            App.ApplicationSettings[Account.ACCOUNT_SKY_DRIVE_USED_SIZE_KEY] = account.account_used_size;
            App.ApplicationSettings[Account.ACCOUNT_SKY_DRIVE_TYPE_NAME_KEY] = account.account_type_name;

            string nickName = null;
            if (!App.ApplicationSettings.TryGetValue<string>(Account.ACCOUNT_NICK_NAME_KEY, out nickName))
                App.ApplicationSettings[Account.ACCOUNT_NICK_NAME_KEY] = AppResources.AtHere;

            App.ApplicationSettings.Save();
        }


        // Save profile information to local isolated App settings.
        private void RemoveProfileReslutFromAppSettings()
        {
            App.ApplicationSettings.Remove(Account.ACCOUNT_IS_SIGN_IN_KEYS[App.SKY_DRIVE_KEY_INDEX]);
            App.ApplicationSettings.Remove(Account.ACCOUNT_ID_KEYS[App.SKY_DRIVE_KEY_INDEX]);
            App.ApplicationSettings.Remove(Account.ACCOUNT_SKY_DRIVE_USED_SIZE_KEY);
            App.ApplicationSettings.Remove(Account.ACCOUNT_SKY_DRIVE_TYPE_NAME_KEY);
        }
    }
}
