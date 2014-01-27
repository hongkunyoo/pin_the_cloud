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
        public override Task<bool> LoginMicrosoftSingleSignOnAsync()
        {


            return null;
        }
    }
}
