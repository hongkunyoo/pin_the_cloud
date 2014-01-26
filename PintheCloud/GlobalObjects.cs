using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using PintheCloud.Managers;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud
{
    public class GlobalObjects
    {

        /*** App***/

        private static MobileServiceClient mobileService = null;
        public static MobileServiceClient MobileService
        {
            get
            {
                // 필요할 때까지 만들기를 지연합니다.
                if (mobileService == null)
                    mobileService = new MobileServiceClient(
                        "https://pinthecloud.azure-mobile.net/",
                        "yvulzHAGRgNsGnPLHKcEFCPJcuyzKj23"
                        );

                return mobileService;
            }
        }


        private static IsolatedStorageSettings applicationSettings = null;
        public static IsolatedStorageSettings ApplicationSettings
        {
            get
            {
                // 필요할 때까지 만들기를 지연합니다.
                if (applicationSettings == null)
                    applicationSettings = IsolatedStorageSettings.ApplicationSettings;

                return applicationSettings;
            }
        }


        private static ProgressIndicator progressIndicator = null;
        public static ProgressIndicator ProgressIndicator
        {
            get
            {
                // 필요할 때까지 만들기를 지연합니다.
                if (progressIndicator == null)
                    progressIndicator = new ProgressIndicator();

                return progressIndicator;
            }
        }



        /*** Manager***/

        private static AccountManager accountManager = null;
        public static AccountManager AccountManager
        {
            get
            {
                // 필요할 때까지 만들기를 지연합니다.
                if (accountManager == null)
                    accountManager = new AccountManager();

                return accountManager;
            }
        }


        private static FileManager fileManager = null;
        public static FileManager FileManager
        {
            get
            {
                // 필요할 때까지 만들기를 지연합니다.
                if (fileManager == null)
                    fileManager = new FileManager();

                return fileManager;
            }
        }



        /*** View Models ***/

        private static SpaceViewModel spaceViewModel = null;
        public static SpaceViewModel SpaceViewModel
        {
            get
            {
                // 필요할 때까지 만들기를 지연합니다.
                if (spaceViewModel == null)
                    spaceViewModel = new SpaceViewModel();

                return spaceViewModel;
            }
        }
    }
}
