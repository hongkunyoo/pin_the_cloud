﻿using Microsoft.Phone.Tasks;
using PintheCloud.Managers;
using PintheCloud.Pages;
using PintheCloud.Popups;
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
        public string Uri { get; set; }
        private UserControl _this;


        public DropboxWebBrowserTask()
        {
        }


        public DropboxWebBrowserTask(string uri)
        {
            this.Uri = uri;
        }


        public DropboxWebBrowserTask(string uri, UserControl _this)
        {
            this.Uri = uri;
            this._this = _this;
        }


        public async override void Show()
        {
            base.Show();
            Popup popup = new Popup();
            DropBoxSignInPopup childPop = new DropBoxSignInPopup(popup, this.Uri);
            await childPop.ClearCache();
            popup.Child = childPop;
            popup.Visibility = Visibility.Visible;
            popup.IsOpen = true;
            popup.Closed += (sender, args) =>
            {
                this.FireCompleted(this, new DropboxWebBrowserResult(), null);
            };
            EventHelper.AddEventHandler(EventHelper.POPUP_CLOSE, () => {
                popup.IsOpen = false;
            });
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
