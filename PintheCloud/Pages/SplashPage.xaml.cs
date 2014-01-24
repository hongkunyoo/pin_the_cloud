using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Resources;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Net.NetworkInformation;

namespace PintheCloud
{
    public partial class SplashPage : PhoneApplicationPage
    {
        // 생성자
        public SplashPage()
        {
            InitializeComponent();

            // ApplicationBar를 지역화하는 샘플 코드
            //BuildLocalizedApplicationBar();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Get bool variable whether this account have logined or not.
            bool accountIsLogin = false;
            App.ApplicationSettings.TryGetValue<bool>(GlobalVariables.ACCOUNT_IS_LOGIN, out accountIsLogin);
            if (!accountIsLogin)  // First Login, Show Login Button.
            {
                uiMicrosoftLoginButton.Visibility = Visibility.Visible;
            }
            else  // Second or more Login, Goto Explorer Page after some secconds.
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                NavigationService.Navigate(new Uri(GlobalVariables.EXPLORER_PAGE, UriKind.Relative));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private async void uiMicrosoftLoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // If Internet is good, go login, 
            // otherwise show no internet message box.
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (await App.AccountManager.LoginMicrosoftSingleSignOnAsync())
                    NavigationService.Navigate(new Uri(GlobalVariables.EXPLORER_PAGE, UriKind.Relative));
            }
            else
            {
                GlobalManager.showNoInternetMessageBox();
            }
        }


        // 지역화된 ApplicationBar를 빌드하는 샘플 코드
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 페이지의 ApplicationBar를 ApplicationBar의 새 인스턴스로 설정합니다.
        //    ApplicationBar = new ApplicationBar();

        //    // 새 단추를 만들고 텍스트 값을 AppResources의 지역화된 문자열로 설정합니다.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // AppResources의 지역화된 문자열을 사용하여 새 메뉴 항목을 만듭니다.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}