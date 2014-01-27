using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public class AccountYesInternetManager : AccountManager
    {
        // Login with single sign on way.
        public override async Task<bool> LoginMicrosoftSingleSignOnAsync()
        {
            bool result = false;

            // If it success to get user's profile result,
            dynamic profileResult = await base.GetProfileResultAsync();
            if (profileResult != null)
            {
                try
                {
                    // Login to mobile service for getting access to DB
                    await App.MobileService.LoginWithMicrosoftAccountAsync(App.Session.AuthenticationToken);
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
                    Account account = await base.isExistedPerson(App.MobileService.CurrentUser.UserId);
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
                    base.RemoveProfileReslutFromAppSettings();
                    base.SaveProfileReslutToAppSettings(account);
                    App.CurrentAccount = account;
                    result = true;
                }
            }
            return result;
        }


        // Register Live Connect Session for Live Profile
        public override async Task<bool> RegisterLiveConnectionSessionAsync()
        {
            bool result = false;
            try
            {
                // Get live connection session
                LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalKeys.AZURE_CLIENT_ID);
                LiveLoginResult liveLoginResult = await liveAuthClient.LoginAsync(new[] { "wl.basic, wl.skydrive" });
                if (liveLoginResult.Status == LiveConnectSessionStatus.Connected)
                {
                    // Register the session which we get above
                    App.Session = liveLoginResult.Session;
                    result = true;
                }
            }
            catch (LiveAuthException)
            {
            }
            return result;
        }
    }
}
