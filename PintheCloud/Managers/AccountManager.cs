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
    public interface AccountManager
    {
        Account GetCurrentAcccount();
        void SetAccountWorker(AccountWorker CurrentAccountManager);

        Task<bool> SetLiveConnectSessionAsync();
        LiveConnectSession GetLiveConnectSession();
        Task<bool> SetProfileResultAsync();
        Task<bool> LoginMicrosoftAccountSingleSignOnAsync();
        void Logout();
    }
}
