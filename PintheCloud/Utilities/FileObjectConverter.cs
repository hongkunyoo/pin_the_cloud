﻿using DropNet.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using PintheCloud.Managers;
using PintheCloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PintheCloud.Utilities
{
    public static class FileObjectConverter
    {
        /// <summary>
        /// Converting SkyDrive Model to FileObject.
        /// </summary>
        /// <param name="dic">SkyDrive Model</param>
        /// <returns>POJO FileObject</returns>
        public static FileObject ConvertToFileObject(IDictionary<string, object> dic)
        {

            string id = (string)(dic["id"] ?? "");
            string name = (string)(dic["name"] ?? "");
            //string parent_id = (string)(dic["parent_id"] ?? "/");
            double size = (double)dic["size"];
            FileObject.FileObjectType type = ("file".Equals(id.Split('.').First()) ? FileObject.FileObjectType.FILE : FileObject.FileObjectType.FOLDER);
            string extension = name.Split('.').Last();
            string updateAt = (string)dic["updated_time"] ?? DateTime.Now.ToString();

            return new FileObject(id, name, size, type, extension, updateAt);
            
        }
        /// <summary>
        /// Converting Windows Azure Blob Storage Model to FileObject.
        /// </summary>
        /// <param name="blob">Blob Storage Model</param>
        /// <returns>POJO FileObject</returns>
        public static FileObject ConvertToFileObject(CloudBlockBlob blob)
        {
            string id = blob.Name;
            string name = ParseHelper.ParseName(id);
            double size = (double)blob.Properties.Length;
            FileObject.FileObjectType type = FileObject.FileObjectType.FILE;
            string extension;
            if (name.Contains("."))
            {
                extension = ParseHelper.SplitNameAndExtension(name)[1];
            }
            else
            {
                extension = "";
            }
            string updateAt = blob.Properties.LastModified.ToString();

            return new FileObject(id, name, size, type, extension, updateAt);
        }
        /// <summary>
        /// Converting Windows Azure Blob Storage Directory Model to FileObject.
        /// </summary>
        /// <param name="directory">Blob Storage Directory</param>
        /// <returns>POJO FileObject</returns>
        public static FileObject ConvertToFileObject(CloudBlobDirectory directory)
        {
            string id = directory.Prefix;
            string name = ParseHelper.ParseName(id);
            double size = 0;
            FileObject.FileObjectType type = FileObject.FileObjectType.FOLDER;
            string extension = "";
            string updateAt = "";

            return new FileObject(id, name, size, type, extension, updateAt);
        }
        /// <summary>
        /// Converting DropBox Model to FileObject
        /// </summary>
        /// <param name="meta">DropBox Model</param>
        /// <returns>POJO FileObject</returns>
        public static FileObject ConvertToFileObject(MetaData meta)
        {
            string Name = meta.Name;
            string UpdateAt = meta.ModifiedDate.ToString(); //14/02/2014 15:48:13
            string Id = meta.Path; // Full path
            string ParentId = meta.Path;
            double Size = meta.Bytes * 1.0;
            FileObject.FileObjectType Type = (meta.Is_Dir ? FileObject.FileObjectType.FOLDER : FileObject.FileObjectType.FILE);
            string Extension = meta.Extension.Substring(1, meta.Extension.Length -1); // .png

            return new FileObject(Id,Name,Size,Type,Extension,UpdateAt);
        }
        /// <summary>
        /// Converting GoogleDrive Model to FileObject
        /// </summary>
        /// <param name="file">GoogleDrive Model</param>
        /// <returns>POJO FileObject</returns>
        public static FileObject ConvertToFileObject(Google.Apis.Drive.v2.Data.File file)
        {
            String Id = file.Id ?? "No Id";
            string Name = file.Title ?? "No Name";
            double Size = (file.FileSize == null ? 0.0 : file.FileSize.Value * 1.0);
            FileObject.FileObjectType Type = (file.MimeType.Contains("application/vnd.google-apps.folder")) ? FileObject.FileObjectType.FOLDER : (file.MimeType.Contains("application/vnd.google-apps") ? FileObject.FileObjectType.GOOGLE_DOC : FileObject.FileObjectType.FILE);
            string Extension = file.FileExtension ?? "No Extension";
            string UpdateAt = file.ModifiedDate.ToString();
            string Thumbnail = file.ThumbnailLink ?? "No Thumbnail";
            string DownloadUrl;
            if (file.ExportLinks != null && file.MimeType != null && file.MimeType.Contains("application/vnd.google-apps"))
            {
                DownloadUrl = file.ExportLinks[GoogleDriveManager.GoogleDocMapper[file.MimeType]];
            }
            else
            {
                DownloadUrl = file.DownloadUrl;
            }
            
            string MimeType = file.MimeType ?? "No MimeType";
            
            return new FileObject(Id,Name,Size,Type, Extension,UpdateAt,Thumbnail,DownloadUrl,MimeType);
        }
    }
}
