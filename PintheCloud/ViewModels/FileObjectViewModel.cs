using PintheCloud.Models;
using PintheCloud.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.ViewModels
{
    public class FileObjectViewModel : INotifyPropertyChanged
    {
        // Instances
        public const string CHECK_NOT_IMAGE_URI = "/Assets/pajeon/at_here/png/general_checkbox.png";
        public const string CHECK_IMAGE_URI = "/Assets/pajeon/at_here/png/general_checkbox_p.png";
        public const string TRANSPARENT_IMAGE_URI = "/Assets/pajeon/at_here/png/general_transparent.png";

        public const string DELETE_IMAGE_URI = "/Assets/pajeon/at_here/png/upload_list_delete.png";
        public const string IS_PRIVATE_IMAGE_URI = "IS_PRIVATE_IMAGE_URI";

        public const string DELETING_IMAGE_URI = "DELETING_SPOT_IMAGE_URI";
        public const string UPLOADING_IMAGE_URI = "UPLOADING_FILE_IMAGE_URI";
        public const string DOWNLOADING_IMAGE_URI = "DOWNLOADING_FILE_IMAGE_URI";
        public const string FAIL_IMAGE_URI = "/Assets/pajeon/at_here/png/general_fail.png";

        public const string FOLDER = "folder";


        public ObservableCollection<FileObjectViewItem> Items { get; set; }

        // Mutex
        public bool IsDataLoaded { get; set; }
        public bool IsDataLoading { get; set; }

        public FileObjectViewModel()
        {
            this.Items = new ObservableCollection<FileObjectViewItem>();
        }


        public void SetItems(List<FileObject> fileObjectList, bool select)
        {
            // If items have something, clear.
            this.Items.Clear();

            // Convert jarray spaces to space view items and set to view model
            foreach (FileObject fileObject in fileObjectList)
            {
                // Set new file view item
                FileObjectViewItem fileObjectViewItem = new FileObjectViewItem();
                fileObjectViewItem.Id = fileObject.Id;
                fileObjectViewItem.Name = fileObject.Name;
                fileObjectViewItem.DownloadUrl = fileObject.DownloadUrl;

                // Set type
                if (fileObject.Type == FileObject.FileObjectType.FOLDER)
                    fileObjectViewItem.ThumnailType = FOLDER;
                else
                    fileObjectViewItem.ThumnailType = fileObject.Extension;

                // Set Size and Size Unit
                double size = fileObject.Size;
                double kbUnit = 1024.0;
                double mbUnit = Math.Pow(kbUnit, 2);
                double gbUnit = Math.Pow(kbUnit, 3);
                if ((size / gbUnit) >= 1)  // GB
                {
                    fileObjectViewItem.Size = (Math.Round((size / gbUnit) * 10.0) / 10.0).ToString().Replace(',','.');
                    fileObjectViewItem.SizeUnit = AppResources.GB;
                }
                else if ((size / mbUnit) >= 1)  // MB
                {
                    fileObjectViewItem.Size = (Math.Round((size / mbUnit) * 10.0) / 10.0).ToString().Replace(',', '.');
                    fileObjectViewItem.SizeUnit = AppResources.MB;
                }
                else if ((size / kbUnit) >= 1)  // KB
                {
                    fileObjectViewItem.Size = (Math.Round(size / kbUnit)).ToString().Replace(',', '.');
                    fileObjectViewItem.SizeUnit = AppResources.KB;
                }
                else if ((size / kbUnit) < 1)  // Bytes
                {
                    fileObjectViewItem.Size = size.ToString().Replace(',', '.');
                    fileObjectViewItem.SizeUnit = AppResources.Bytes;
                }
                else if (fileObject.Type == FileObject.FileObjectType.GOOGLE_DOC) // Google Doc
                {
                    fileObjectViewItem.Size = String.Empty;
                    fileObjectViewItem.SizeUnit = AppResources.GoogleDoc;
                }


                // If select is on, set check image.
                // Otherwise, set transparent image.
                if (select)
                {
                    if (fileObject.Type == FileObject.FileObjectType.FOLDER)
                        fileObjectViewItem.SelectFileImage = TRANSPARENT_IMAGE_URI;
                    else
                        fileObjectViewItem.SelectFileImage = CHECK_NOT_IMAGE_URI;
                }
                else
                {
                    fileObjectViewItem.SelectFileImage = TRANSPARENT_IMAGE_URI;
                }

                this.Items.Add(fileObjectViewItem);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
