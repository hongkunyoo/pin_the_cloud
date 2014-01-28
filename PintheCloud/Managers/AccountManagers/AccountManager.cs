using Microsoft.Live;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Managers.AccountManagers
{
    public interface AccountManager
    {
        Account GetCurrentAcccount();
        LiveConnectSession GetLiveConnectSession();
        void SetAccountManager(AccountWorker accountManager);

        Task<bool> LoginMicrosoftAccountSingleSignOnAsync();
        Task<bool> GetLiveConnectSessionAsync();
    }
}
