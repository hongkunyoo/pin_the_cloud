using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Workers
{
    public class AccountInternetAvailableWorker : AccountWorker
    {
        // Login with single sign on way.
        public override async Task<Account> LoginMicrosoftAccountSingleSignOnAsync(LiveConnectSession session, dynamic profileResult)
        {
            Account account = null;
            try
            {
                // Login to mobile service for getting access to DB
                await App.MobileService.LoginWithMicrosoftAccountAsync(session.AuthenticationToken);
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
                account = await base.IsExistedPerson(App.MobileService.CurrentUser.UserId);

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
                base.SaveProfileReslutToAppSettings(account);
            }
            return account;
        }


        // Register Live Connect Session for Live Profile
        public override async Task<LiveConnectSession> GetLiveConnectSessionAsync()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
            //string[] scopes = new[] { "wl.basic, wl.offline_access, wl.skydrive, wl.skydrive_update" };
            string[] scopes = new[] { "wl.basic", "wl.offline_access", "wl.skydrive", "wl.skydrive_update", "wl.signin", "wl.contacts_skydrive" };
            LiveLoginResult liveLoginResult = null;
            LiveConnectSession session = null;

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
                catch (InvalidOperationException)
                {
                    return null;
                }
            }

            // Register session which we get above
            session = liveLoginResult.Session;
            return session;
        }


        // Get User Profile information result using registered live connection session
        public override async Task<dynamic> GetProfileResultAsync(LiveConnectSession session)
        {
            dynamic result = null;
            try
            {
                LiveConnectClient liveClient = new LiveConnectClient(session);
                LiveOperationResult operationResult = await liveClient.GetAsync("me");
                result = operationResult.Result;
            }
            catch (LiveConnectException)
            {
            }
            return result;
        }
    }
}
