using Microsoft.Phone.Tasks;
using PintheCloud.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PintheCloud.Utilities
{
    public class MyWebBrowserTask : ChooserBase<MyWebBrowserResult>
    {
        private UserControl _this;
        private string uri;
        private string url;

        public string Uri
        {
            get
            {
                return uri;
            }
            set
            {
                uri = value;
            }
        }
        public MyWebBrowserTask()
        {

        }
        public MyWebBrowserTask(string uri)
        {
            this.uri = uri;
        }
        public MyWebBrowserTask(string uri, UserControl _this)
        {
            this.uri = uri;
            this._this = _this;
        }


        public override void Show()
        {
            base.Show();

            Popup p = new Popup();
            p.Child = new MyPopup(p, this.uri);
            p.Visibility = System.Windows.Visibility.Visible;
            p.IsOpen = true;

            p.Closed += (sender, args) =>
            {
                this.FireCompleted(this, new MyWebBrowserResult(), null);
            };
        }
        public Task<MyWebBrowserResult> ShowAsync()
        {
            
            TaskCompletionSource<MyWebBrowserResult> tcs = new TaskCompletionSource<MyWebBrowserResult>();
            this.Completed += (sender, e) =>
            {
                if (e.Error != null)
                    tcs.SetException(e.Error);
                else
                    tcs.SetResult(e);
            };
            this.Show();
            return tcs.Task;
        }
    }
}
