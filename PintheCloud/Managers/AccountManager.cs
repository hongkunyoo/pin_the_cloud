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
        LiveConnectSession GetLiveConnectSession();
        dynamic GetProfileResult();
        Account GetCurrentAcccount();
        void SetAccountWorker(AccountWorker accountManager);

        Task<bool> SetLiveConnectSessionAsync();
        Task<bool> SetProfileResultAsync();
        Task<bool> LoginMicrosoftAccountSingleSignOnAsync();
        void Logout();
    }
}
