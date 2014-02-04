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
        public string Name { get; set; }
        public string ParentId { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public string TypeDetail { get; set; }
        public string CreateAt { get; set; }
        public string UpdateAt { get; set; }
        public List<FileObject> FileList { get; set; }

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
                App.HDebug.WriteLine("id : " + fo.Id);
                App.HDebug.WriteLine("Name : " + fo.Name);
                App.HDebug.WriteLine("ParentId : " + fo.ParentId);
                App.HDebug.WriteLine("Size : " + fo.Size);
                App.HDebug.WriteLine("Type : " + fo.Type);
                App.HDebug.WriteLine("TypeDetail : " + fo.TypeDetail);
                App.HDebug.WriteLine("CreateAt : " + fo.CreateAt);
                App.HDebug.WriteLine("UpdateAt : " + fo.UpdateAt);

                App.HDebug.WriteLine("-----------------------------------------");
                if (fo.FileList != null)
                    FileObject.PrintFileObjectList(fo.FileList);
            }
            else
            {
                App.HDebug.WriteLine("FileObject Null!");
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
