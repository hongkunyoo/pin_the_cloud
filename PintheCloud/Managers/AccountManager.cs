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
    public class AccountManager
    {
        private LiveConnectSession Session = null;


        // Login with single sign on way.
        public async Task<bool> LoginMicrosoftSingleSignOnAsync()
        {
            bool result = false;
            
            // If it success to register live connect session,
            if (await this.RegisterLiveConnectionSessionAsync())
            {
                // If it success to get user's profile result,
                dynamic profileResult = await this.GetProfileResultAsync();
                if (profileResult != null)
                {
                    try
                    {
                        // Login to mobile service for getting access to DB
                        await GlobalObjects.MobileService.LoginWithMicrosoftAccountAsync(this.Session.AuthenticationToken);
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    // If it success to get access to mobile service, 
                    // Make final account
                    if (GlobalObjects.MobileService.CurrentUser != null)
                    {
                        Account account = new Account(GlobalObjects.MobileService.CurrentUser.UserId, GlobalKeys.MICROSOFT, "" + profileResult.name,
                            "" + profileResult.first_name, "" + profileResult.last_name, "" + profileResult.locale,
                            GlobalObjects.MobileService.CurrentUser.MobileServiceAuthenticationToken, 0, GlobalKeys.NORMAL_ACCOUNT_TYPE);

                        // Insert if it is not exists already in DB,
                        // Otherwise update account.
                        try
                        {
                            await GlobalObjects.MobileService.GetTable<Account>().InsertAsync(account);

                            // If it success to insert account to DB,
                            // Save it's information to isolated storage.
                            this.SaveProfileReslutToAppSettings(account);
                            result = true;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
            return result;
        }


        // Register Live Connect Session for Live Profile
        public async Task<bool> RegisterLiveConnectionSessionAsync()
        {
            bool result = false;
            try
            {
                // Get live connection session
                LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
                LiveLoginResult liveLoginResult = await liveAuthClient.LoginAsync(new[] { "wl.basic" });
                if (liveLoginResult.Status == LiveConnectSessionStatus.Connected)
                {
                    // Register the session which we get above
                    this.Session = liveLoginResult.Session;
                    result = true;
                }
            }
            catch (LiveAuthException)
            {
            }
            return result;
        }


        // Get User Profile information result using registered live connection session
        public async Task<dynamic> GetProfileResultAsync()
        {
            dynamic result = null;
            try
            {
                LiveConnectClient liveClient = new LiveConnectClient(this.Session);
                LiveOperationResult operationResult = await liveClient.GetAsync("me");
                result = operationResult.Result;
            }
            catch (LiveConnectException)
            {
            }
            return result;
        }


        // Save profile information to local isolated App settings.
        public void SaveProfileReslutToAppSettings(Account account)
        {
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_IS_LOGIN, true);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_PLATFROM_ID, account.account_platform_id);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_PLATFROM_ID_TYPE, account.account_platform_id_type);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_NAME, account.account_name);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_FIRST_NAME, account.account_first_name);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_LAST_NAME, account.account_last_name);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_LOCAL, account.account_locale);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_TOKEN, account.account_token);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_USED_SIZE, account.account_used_size);
            GlobalObjects.ApplicationSettings.Add(GlobalKeys.ACCOUNT_TYPE_NAME, account.account_type_name);
        }

        // Get out connection session
        public void Logout()
        {
            LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
            liveAuthClient.Logout();
            this.RemoveProfileReslutFromAppSettings();
        }

        // Save profile information to local isolated App settings.
        public void RemoveProfileReslutFromAppSettings()
        {
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_IS_LOGIN);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_PLATFROM_ID);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_PLATFROM_ID_TYPE);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_NAME);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_FIRST_NAME);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_LAST_NAME);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_LOCAL);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_TOKEN);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_USED_SIZE);
            GlobalObjects.ApplicationSettings.Remove(GlobalKeys.ACCOUNT_TYPE_NAME);
        }
    }
}
