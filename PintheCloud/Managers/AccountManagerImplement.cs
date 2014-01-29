using Microsoft.Live;
using PintheCloud.Models;
using PintheCloud.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers
{
    public class AccountManagerImplement : AccountManager
    {
        /*** Instance ***/

        private LiveConnectSession Session = null;
        private dynamic ProfileResult = null;
        private Account CurrentAccount = null;
        public Account GetCurrentAcccount()
        {
            return this.CurrentAccount;
        }

        private AccountWorker CurrentAccountWorker;
        public void SetAccountWorker(AccountWorker CurrentAccountWorker)
        {
            this.CurrentAccountWorker = CurrentAccountWorker;
        }



        /*** Implementation ***/

        public async Task<bool> SetLiveConnectSessionAsync()
        {
            this.Session = await this.CurrentAccountWorker.GetLiveConnectSessionAsync();
            if(this.Session == null)
                return false;
            else
                return true;
        }

        public async Task<bool> SetProfileResultAsync()
        {
            this.ProfileResult = await this.CurrentAccountWorker.GetProfileResultAsync(this.Session);
            if (this.ProfileResult == null)
                return false;
            else
                return true;
        }

        public async Task<bool> LoginMicrosoftAccountSingleSignOnAsync()
        {
            this.CurrentAccount = await this.CurrentAccountWorker.LoginMicrosoftAccountSingleSignOnAsync(this.Session, this.ProfileResult);
            if (this.CurrentAccount == null)
                return false;
            else
                return true;
        }

        public void Logout()
        {
            this.CurrentAccountWorker.Logout();
        }
    }
}
