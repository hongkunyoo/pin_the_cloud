using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Exceptions
{
    public class SignInException : Exception
    {
        public string FileId { get; set; }


        public SignInException(string fileId)
        {
            FileId = fileId;
        }


        public override string ToString()
        {
            return this.FileId;
        }
    }
}
