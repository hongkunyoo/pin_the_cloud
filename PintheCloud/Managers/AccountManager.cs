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


        /*** Public ***/

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
                        await App.MobileService.LoginWithMicrosoftAccountAsync(this.Session.AuthenticationToken);
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }

                    // If it success to get access to mobile service, 
                    // Make final account
                    if (App.MobileService.CurrentUser != null)
                    {
                        // Check duplication.
                        // Insert if it is not exists already in DB,
                        // Otherwise update account.
                        Account account = await this.isExistedPerson(App.MobileService.CurrentUser.UserId);
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
                                return false;
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
                                return false;
                            }
                        }

                        // If it success to insert account to DB,
                        // Save it's information to isolated storage.
                        this.SaveProfileReslutToAppSettings(account);
                        result = true;
                    }
                }
            }
            return result;
        }



        /*** Protected ***/

        /*** Private ***/

        


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

        // Check whether it exists in DB
        private async Task<Account> isExistedPerson(string account_platform_id)
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
