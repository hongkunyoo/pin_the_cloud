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

namespace PintheCloud
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 생성자
        public MainPage()
        {
            InitializeComponent();

            // ApplicationBar를 지역화하는 샘플 코드
            //BuildLocalizedApplicationBar();
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