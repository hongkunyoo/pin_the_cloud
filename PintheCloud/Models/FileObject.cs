using PintheCloud.Resources;
using PintheCloud.Utilities;
using PintheCloud.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    /// <summary>
    /// Model Class for storing file meta information from each kind of storages.
    /// Every files will be handled by this object to provide a abstraction.
    /// </summary>
    public class FileObject : INotifyPropertyChanged
    {
        // Instances
        public static string CHECK_NOT_IMAGE_PATH = "/Assets/pajeon/png/general_checkbox.png";
        public static string CHECK_IMAGE_PATH = "/Assets/pajeon/png/general_checkbox_p.png";
        public static string TRANSPARENT_PATH = "/Assets/pajeon/png/general_transparent.png";
        
        /// <summary>
        /// The id to Get, Upload, Download
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The name of the file
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The parent Id to get back to the parent tree.
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// The size of the file
        /// </summary>
        public double Size { get; set; }
        /// <summary>
        /// file or folder
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The file extension such as mp3, pdf
        /// </summary>
        public string TypeDetail { get; set; }
        /// <summary>
        /// Thumbnail type
        /// </summary>
        public string ThumnailType { get; set; }
        /// <summary>
        /// No need to use.
        /// </summary>
        public string CreateAt { get; set; }
        /// <summary>
        /// For created time & updated time.
        /// </summary>
        public string UpdateAt { get; set; }
        /// <summary>
        /// The child list of the folder.
        /// </summary>
        public List<FileObject> FileList { get; set; }

        private string selectCheckImage;
        public string SelectCheckImage
        {
            get
            {
                return selectCheckImage;
            }
            set
            {
                if (selectCheckImage != value)
                {
                    selectCheckImage = value;
                    NotifyPropertyChanged("SelectCheckImage");
                }
            }
        }


        public FileObject(string id, string name, string parentId, double size, string type, string typeDetail, string createAt, string updateAt)
        {
            this.Id = id;
            this.Name = name;
            this.ParentId = parentId;
            this.Size = size;

            //this.Type = id.Substring(0,id.IndexOf("."));
            this.Type = type;
            this.TypeDetail = typeDetail;
            if (this.Type.Equals(AppResources.Folder))
            {
                 this.ThumnailType = this.Type;
                 this.SelectCheckImage = TRANSPARENT_PATH;
            }
            else
            {
                this.ThumnailType = this.TypeDetail;
                this.SelectCheckImage = CHECK_NOT_IMAGE_PATH;
            }    
            this.CreateAt = createAt;
            this.UpdateAt = updateAt;
        }
        public void SetSelectCheckImage(bool isCheck)
        {
            if (isCheck)
                this.SelectCheckImage = CHECK_IMAGE_PATH;
            else
                this.SelectCheckImage = CHECK_NOT_IMAGE_PATH;
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

        /// <summary>
        /// Test Code for Printing FileObject easily.
        /// </summary>
        /// <param name="fo"></param>
        public static void PrintFileObject(FileObject fo)
        {
            if (fo != null)
            {
                MyDebug.WriteLine("id : " + fo.Id);
                MyDebug.WriteLine("Name : " + fo.Name);
                MyDebug.WriteLine("ParentId : " + fo.ParentId);
                MyDebug.WriteLine("Size : " + fo.Size);
                MyDebug.WriteLine("Type : " + fo.Type);
                MyDebug.WriteLine("TypeDetail : " + fo.TypeDetail);
                MyDebug.WriteLine("CreateAt : " + fo.CreateAt);
                MyDebug.WriteLine("UpdateAt : " + fo.UpdateAt);

                MyDebug.WriteLine("-----------------------------------------");
                if (fo.FileList != null)
                    FileObject.PrintFileObjectList(fo.FileList);
            }
            else
            {
                MyDebug.WriteLine("FileObject Null!");
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }

        }
        public static void PrintFileObjectList(List<FileObject> list)
        {
            foreach (FileObject fo in list)
                FileObject.PrintFileObject(fo);
        }
    }
}
