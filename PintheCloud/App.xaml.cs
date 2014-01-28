using System;
using System.Diagnostics;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PintheCloud.Resources;
using Microsoft.WindowsAzure.MobileServices;
using System.IO.IsolatedStorage;
using PintheCloud.Managers;
using PintheCloud.ViewModels;
using PintheCloud.Models;
using Microsoft.Live;
using PintheCloud.Managers.AccountManagers;

namespace PintheCloud
{
    public partial class App : Application
    {
        /// <summary>
        /// 전화 응용 프로그램의 루트 프레임에 간단하게 액세스할 수 있습니다.
        /// </summary>
        /// <returns>전화 응용 프로그램의 루트 프레임입니다.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        // Root static instance variable.
        // App
        public static MobileServiceClient MobileService = null;
        public static IsolatedStorageSettings ApplicationSettings = null;
        public static ProgressIndicator ProgressIndicator = null;

        // Model
        public static SpaceViewModel SpaceViewModel = null;

        // Manager
        public static AccountManager CurrentAccountManager = null;


        /// <summary>
        /// Application 개체의 생성자입니다.
        /// </summary>
        public App()
        {
            // Catch되지 않은 예외의 전역 처리기입니다.
            UnhandledException += Application_UnhandledException;

            // 표준 XAML 초기화
            InitializeComponent();

            // 전화 관련 초기화
            InitializePhoneApplication();

            // 언어 표시 초기화
            InitializeLanguage();

            // Initializing public static instances
            MobileService = new MobileServiceClient(
                "https://pinthecloud.azure-mobile.net/",
                "yvulzHAGRgNsGnPLHKcEFCPJcuyzKj23"
            );
            ApplicationSettings = IsolatedStorageSettings.ApplicationSettings;
            ProgressIndicator = new ProgressIndicator();
            SpaceViewModel = new SpaceViewModel();
            CurrentAccountManager = new AccountManagerImplement();


            // 디버깅하는 동안 그래픽 프로파일링 정보를 표시합니다.
            if (Debugger.IsAttached)
            {
                // 현재 프레임 속도 카운터를 표시합니다.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // 각 프레임에서 다시 그려지는 응용 프로그램의 영역을 표시합니다.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // 색이 지정된 오버레이로 가속된 GPU에 전달되는 페이지 영역을 표시하는
                // 비프로덕션 분석 시각화 모드를 설정합니다.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // 응용 프로그램의 유휴 검색을 사용하지 않도록 설정하여 디버거를
                // 사용하는 동안 화면이 꺼지지 않도록 방지합니다.
                // 주의:- 디버그 모드에서만 사용합니다. 사용자 유휴 검색을 해제하는 응용 프로그램은 사용자가 전화를 사용하지 않을 경우에도
                // 계속 실행되어 배터리 전원을 소모합니다.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // 시작 메뉴 등에서 응용 프로그램을 시작할 때 실행할 코드입니다.
        // 이 코드는 응용 프로그램이 다시 활성화될 때는 실행되지 않습니다.
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // 응용 프로그램이 활성화(포그라운드로 이동)될 때 실행할 코드입니다.
        // 이 코드는 응용 프로그램이 처음 시작될 때는 실행되지 않습니다.
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // 응용 프로그램이 비활성화(백그라운드로 전송)될 때 실행할 코드입니다.
        // 이 코드는 응용 프로그램이 닫힐 때는 실행되지 않습니다.
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // 응용 프로그램이 닫힐 때(예: 사용자가 [뒤로]를 누르는 경우) 실행할 코드입니다.
        // 이 코드는 응용 프로그램이 비활성화될 때는 실행되지 않습니다.
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // 탐색이 실패할 때 실행할 코드입니다.
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // 탐색이 실패했습니다. 중단하고 디버거를 실행합니다.
                Debugger.Break();
            }
        }

        // 처리되지 않은 예외에 대해 실행할 코드입니다.
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // 처리되지 않은 예외가 발생했습니다. 중단하고 디버거를 실행합니다.
                Debugger.Break();
            }
        }

        #region 전화 응용 프로그램 초기화

        // 이중 초기화를 사용하지 않습니다.
        private bool phoneApplicationInitialized = false;

        // 이 메서드에 추가 코드를 추가하지 않습니다.
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // 프레임을 만들지만 프레임을 아직 RootVisual로 설정하지 않습니다. 이렇게 하면
            // 응용 프로그램이 렌더링할 준비가 될 때까지 시작 화면이 활성 상태를 유지합니다.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // 탐색 오류를 처리합니다.
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // 페이지 스택을 다시 설정해야 하는지 확인하려면
            RootFrame.Navigated += CheckForResetNavigation;

            // 다시 초기화하지 않습니다.
            phoneApplicationInitialized = true;
        }

        // 이 메서드에 추가 코드를 추가하지 않습니다.
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // 응용 프로그램을 렌더링하도록 루트 Visual을 설정합니다.
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // 더 이상 필요하지 않으므로 이 처리기를 제거합니다.
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        private void CheckForResetNavigation(object sender, NavigationEventArgs e)
        {
            // 앱에서 '다시 설정' 탐색을 받았으면 확인해야 합니다.
            // 다음 탐색에서 백스택을 지우기 위해 다시 설정 요청을 처리합니다.
            if (e.NavigationMode == NavigationMode.Reset)
                RootFrame.Navigated += ClearBackStackAfterReset;
        }

        private void ClearBackStackAfterReset(object sender, NavigationEventArgs e)
        {
            // 이벤트를 등록 취소하므로 이벤트가 다시 호출되지 않습니다.
            RootFrame.Navigated -= ClearBackStackAfterReset;

            // '새'(앞) 탐색 및 '새로 고침' 탐색에 대한 스택만 지웁니다.
            if (e.NavigationMode != NavigationMode.New && e.NavigationMode != NavigationMode.Refresh)
                return;

            // UI 일관성을 위해 전체 페이지 스택을 지웁니다.
            while (RootFrame.RemoveBackEntry() != null)
            {
                ; // 아무 작업도 하지 않습니다.
            }
        }

        #endregion

        // 지역화된 리소스 문자열에 정의된 대로 앱의 글꼴 및 흐름 방향을 초기화합니다.
        //
        // 응용 프로그램의 글꼴이 지원되는 언어와 일치하고 이러한 각 언어의 FlowDirection이
        // 기존 방향을 따르는지 확인하려면 각 resx 파일에서 ResourceLanguage 및
        // ResourceFlowDirection을 초기화하여 해당 값을 필터의 문화권과 일치시켜야
        // 합니다. 예:
        //
        // AppResources.es-ES.resx
        //    ResourceLanguage 값은 "es-ES"여야 합니다.
        //    ResourceFlowDirection 값은 "LeftToRight"여야 합니다.
        //
        // AppResources.ar-SA.resx
        //     ResourceLanguage 값은 "ar-SA"여야 합니다.
        //     ResourceFlowDirection 값은 "RightToLeft"여야 합니다.
        //
        // Windows Phone 앱 지역화에 대한 자세한 내용은 http://go.microsoft.com/fwlink/?LinkId=262072를 참조하십시오.
        //
        private void InitializeLanguage()
        {
            try
            {
                // 지원되는 각 언어에 대해 ResourceLanguage 리소스 문자열로
                // 정의된 표시 언어에 맞게 글꼴을 설정하십시오.
                //
                // 휴대폰의 표시 언어가 지원되지 않을 경우
                // 중립 언어의 글꼴로 대체됩니다.
                //
                // 컴파일러 오류가 발생하면 리소스 파일에서 ResourceLanguage가 누락된
                // 것입니다.
                RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                // 지원되는 각 언어에 대해 ResourceFlowDirection 리소스 문자열을
                // 기반으로 루트 프레임 아래에 있는 모든 요소의 FlowDirection을
                // 설정하십시오.
                //
                // 컴파일러 오류가 발생하면 리소스 파일에서 ResourceFlowDirection이 누락된
                // 것입니다.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                RootFrame.FlowDirection = flow;
            }
            catch
            {
                // 여기에서 예외가 catch되는 경우 ResourceLangauge가 지원되는
                // 언어 코드로 올바르게 설정되지 않았거나 ResourceFlowDirection이
                // LeftToRight 또는 RightToLeft 이외의 값으로 설정되었기 때문일 수
                // 있습니다.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}