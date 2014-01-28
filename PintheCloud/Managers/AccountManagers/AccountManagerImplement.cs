using Microsoft.Live;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers.AccountManagers
{
    public class AccountManagerImplement : AccountManager
    {
        private Account CurrentAccount = null;
        public Account GetCurrentAcccount()
        {
            return this.CurrentAccount;
        }

        private LiveConnectSession Session = null;
        public LiveConnectSession GetLiveConnectSession()
        {
            return this.Session;
        }

        private AccountWorker accountWorker;
        public void SetAccountManager(AccountWorker accountWorker)
        {
            this.accountWorker = accountWorker;
        }

        public async Task<bool> LoginMicrosoftAccountSingleSignOnAsync()
        {
            this.CurrentAccount = await this.accountWorker.LoginMicrosoftAccountSingleSignOnAsync();
            if (this.CurrentAccount == null)
                return false;
            else
                return true;
        }

        public async Task<bool> GetLiveConnectSessionAsync()
        {
            this.Session = await this.accountWorker.GetLiveConnectSessionAsync();
            if(this.Session == null)
                return false;
            else
                return true;
        }
    }
}
