using PintheCloud.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Models
{
    public class FileObject
    {
        public string Id { get; set; }
        public string Name { get; set; }    // Name
        public string ParentId { get; set; }
        public int Size { get; set; }   // file size
        public string Type { get; set; }    // whethere it is file or folder
        public string TypeDetail { get; set; }  // file extension such as PDF, MP3
        public string CreateAt { get; set; }    // do not use this
        public string UpdateAt { get; set; }    // updated time & created time
        public List<FileObject> FileList { get; set; }  // child List

        public FileObject()
        {

        }
        public FileObject(string id, string name, string parentId, int size, string type, string typeDetail, string createAt, string updateAt)
        {
            this.Id = id;
            this.Name = name;
            this.ParentId = parentId;
            this.Size = size;
            //this.Type = id.Substring(0,id.IndexOf("."));
            this.Type = type;
            this.TypeDetail = typeDetail;
            this.CreateAt = createAt;
            this.UpdateAt = updateAt;
        }

        public static void PrintFileObject(FileObject fo){
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
                if(System.Diagnostics.Debugger.IsAttached)
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
