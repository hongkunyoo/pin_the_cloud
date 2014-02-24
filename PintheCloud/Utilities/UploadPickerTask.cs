using Microsoft.Phone.Tasks;
using PintheCloud.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace PintheCloud.Utilities
{
    public class UploadPickerTask : ChooserBase<TaskEventArgs>
    {
        private UserControl userControl;

        public override void Show()
        {
            base.Show();

            Popup p = new Popup();
            p.Child = new UploadPickerPopup();
            p.Visibility = System.Windows.Visibility.Visible;
            p.IsOpen = true;

            p.Closed += (sender, args) =>
            {
                this.FireCompleted(this, new TaskEventArgs(), null);
            };
        }
        public Task<TaskEventArgs> ShowAsync()
        {
            TaskCompletionSource<TaskEventArgs> tcs = new TaskCompletionSource<TaskEventArgs>();
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
