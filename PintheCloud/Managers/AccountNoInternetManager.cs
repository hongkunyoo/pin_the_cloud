﻿using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public class AccountNoInternetManager : AccountManager
    {
        // Login with isolated storage information
        public override Task<bool> LoginMicrosoftSingleSignOnAsync()
        {
            string account_platform_id = App.ApplicationSettings[Account.ACCOUNT_PLATFORM_ID].ToString();
            string account_platform_id_type = App.ApplicationSettings[Account.ACCOUNT_PLATFORM_ID_TYPE].ToString();
            string account_name = App.ApplicationSettings[Account.ACCOUNT_NAME].ToString();
            string account_first_name = App.ApplicationSettings[Account.ACCOUNT_FIRST_NAME].ToString();
            string account_last_name = App.ApplicationSettings[Account.ACCOUNT_LAST_NAME].ToString();
            string account_local = App.ApplicationSettings[Account.ACCOUNT_LOCAL].ToString();
            string account_token = App.ApplicationSettings[Account.ACCOUNT_TOKEN].ToString();
            double account_used_size = (double) App.ApplicationSettings[Account.ACCOUNT_USED_SIZE];
            string account_type_name = App.ApplicationSettings[Account.ACCOUNT_TYPE_NAME].ToString();

            Account account = new Account(account_platform_id, account_platform_id_type, account_name, account_first_name, 
                account_last_name, account_local, account_token, account_used_size, account_type_name);
            App.CurrentAccount = account;

            App.MobileService.CurrentUser = new MobileServiceUser(account_platform_id);
            App.MobileService.CurrentUser.MobileServiceAuthenticationToken = account_token;

            return Task<bool>.FromResult(true);
        }

        // No Internet. No Session. Return true for process.
        public override Task<bool> RegisterLiveConnectionSessionAsync()
        {
            return Task<bool>.FromResult(true);
        }
    }
}
