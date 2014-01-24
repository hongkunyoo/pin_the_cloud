using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud
{
    public class AccountManager
    {
        private LiveConnectSession Session = null;


        // Login with single sign on way.
        public async Task<bool> LoginMicrosoftSingleSignOnAsync()
        {
            bool result = false;
            
            // If it success to register live connect session,
            if (await this.RegisterLiveConnectionSession())
            {
                // If it success to get user's profile result,
                dynamic profileResult = await this.GetProfileResult();
                if (profileResult != null)
                {
                    try
                    {
                        // Login to mobile service for getting access to DB
                        await App.MobileService.LoginWithMicrosoftAccountAsync(this.Session.AuthenticationToken);

                        
                        // If it success to get access to mobile service, 
                        // make final account
                        // save it's information to mobile service DB
                        // save it's information to isolated storage.
                        if (App.MobileService.CurrentUser != null)
                        {
                            Account account = new Account(App.MobileService.CurrentUser.UserId, GlobalVariables.MICROSOFT, ""+profileResult.name, 
                                ""+profileResult.first_name, ""+profileResult.last_name, ""+profileResult.locale, 
                                App.MobileService.CurrentUser.MobileServiceAuthenticationToken, 0, GlobalVariables.NORMAL_ACCOUNT_TYPE);

                            // ERROR, because just for now error in azure mobile service server script.
                            // Could not find global symbol, as like tables...
                            await App.MobileService.GetTable<Account>().InsertAsync(account); 
                            this.SaveProfileReslutToAppSettings(account);
                            result = true;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }
            return result;
        }


        // Register Live Connect Session for Live Profile
        public async Task<bool> RegisterLiveConnectionSession()
        {
            bool result = false;
            try
            {
                // Get live connection session
                LiveAuthClient liveAuthClient = new LiveAuthClient(GlobalVariables.AZURE_CLIENT_ID);
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
        public async Task<dynamic> GetProfileResult()
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
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_IS_LOGIN, true);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_PLATFROM_ID, account.account_platform_id);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_PLATFROM_ID_TYPE, account.account_platform_id_type);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_NAME, account.account_name);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_FIRST_NAME, account.account_first_name);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_LAST_NAME, account.account_last_name);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_LOCAL, account.account_locale);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_TOKEN, account.account_token);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_USED_SIZE, account.account_used_size);
            App.ApplicationSettings.Add(GlobalVariables.ACCOUNT_TYPE_NAME, account.account_type_name);
        }
    }
}
