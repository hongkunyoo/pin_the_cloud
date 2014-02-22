using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Managers;
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
    public class SkyDriveAccountWorker
    {
        /*** Public ***/

        // Login with single sign on way.
        public async Task<Account> SkyDriveSignInAsync(LiveConnectClient liveClient, dynamic profileResult)
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
                    account = new Account(App.MobileService.CurrentUser.UserId, Account.PLATFORM_NAMES[(int)Account.StorageAccountType.SKY_DRIVE], 
                        "" + profileResult.name, 0, AccountType.NORMAL_ACCOUNT_TYPE);
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
                    try
                    {
                        await App.MobileService.GetTable<Account>().UpdateAsync(account);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }


                // Save profile information to local isolated App settings.
                App.ApplicationSettings[SkyDriveManager.ACCOUNT_ID_KEY] = account.account_platform_id;
                App.ApplicationSettings[SkyDriveManager.ACCOUNT_USED_SIZE_KEY] = account.account_used_size;
                App.ApplicationSettings[SkyDriveManager.ACCOUNT_BUSINESS_TYPE_KEY] = account.account_business_type;
                App.ApplicationSettings.Save();
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
            if (liveLoginResult.Session == null)
                return null;
            else
                return new LiveConnectClient(liveLoginResult.Session);
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
                return null;
            }

            if (operationResult.Result == null)
                return null;
            else 
                return operationResult.Result;
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
    }
}
