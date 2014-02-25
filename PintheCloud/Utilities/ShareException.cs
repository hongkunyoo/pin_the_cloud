using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Utilities
{
    public class ShareException : Exception
    {
        public enum ShareType { UPLOAD, DOWNLOAD };
        public string FileId { get; set; }
        public ShareType Type { get; set; }

        public ShareException(string fileId)
        {
            FileId = fileId;
        }
        public ShareException(string fileId, ShareType type)
        {
            this.FileId = fileId;
            this.Type = type;
        }
        public override string ToString()
        {
            return this.FileId + " : " + this.Type.ToString();
        }
    }
}
