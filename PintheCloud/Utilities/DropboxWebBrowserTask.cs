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
    public class DropboxWebBrowserTask : ChooserBase<DropboxWebBrowserResult>
    {
        private UserControl _this;
        private string uri;

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


        public DropboxWebBrowserTask()
        {
        }


        public DropboxWebBrowserTask(string uri)
        {
            this.uri = uri;
        }


        public DropboxWebBrowserTask(string uri, UserControl _this)
        {
            this.uri = uri;
            this._this = _this;
        }


        public override void Show()
        {
            base.Show();

            Popup p = new Popup();
            p.Child = new DropBoxSignInPopup(p, this.uri);
            p.Visibility = System.Windows.Visibility.Visible;
            p.IsOpen = true;

            p.Closed += (sender, args) =>
            {
                this.FireCompleted(this, new DropboxWebBrowserResult(), null);
            };
        }


        public Task<DropboxWebBrowserResult> ShowAsync()
        {
            TaskCompletionSource<DropboxWebBrowserResult> tcs = new TaskCompletionSource<DropboxWebBrowserResult>();
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
