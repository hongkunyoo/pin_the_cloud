using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public class FileObjectDataContext : DataContext
    {
        //public static string DBConnectionString = "Data Source=isostore:/ToDo.sdf";

        public FileObjectDataContext(string constr)
            : base(constr)
        {

        }

        public Table<FileObjectSQL> FileItems;
    }

    


    [Table(Name = "FileObjectSQLs")]
    public class FileObjectSQL
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "NVarChar(50) NOT NULL", CanBeNull = false, AutoSync = AutoSync.Default)]
        public string Id { get; set; }
        [Column]
        public string Name { get; set; }
        [Column]
        public double Size { get; set; }
        [Column]
        public PintheCloud.Models.FileObject.FileObjectType Type { get; set; }
        [Column]
        public string Extension { get; set; }
        [Column]
        public string UpdateAt { get; set; }
        [Column]
        public string Thumbnail { get; set; }
        [Column]
        public string DownloadUrl { get; set; }
        [Column]
        public string MimeType { get; set; }
        [Column]
        public string ProfileId { get; set; }
        [Column]
        public string ProfileEmail { get; set; }
        [Column]
        public string ProfilePhoneNumber { get; set; }
        [Column]
        public string ProfileName { get; set; }
        [Column]
        public string SpotId { get; set; }
        [Column]
        public string ParentId { get; set; }
    }
}
