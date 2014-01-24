using PintheCloud.Resources;

namespace PintheCloud
{
    /// <summary>
    /// 문자열 리소스에 대한 액세스를 제공합니다.
    /// </summary>
    public class LocalizedStrings
    {
        private static AppResources _localizedResources = new AppResources();

        public AppResources LocalizedResources { get { return _localizedResources; } }
    }
}