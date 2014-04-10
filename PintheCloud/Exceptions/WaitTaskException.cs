using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PintheCloud.Exceptions
{
    public class WaitTaskException : Exception
    {
        public string Message { get; set; }

        public WaitTaskException()
        {
        }

        public WaitTaskException(string message)
        {
            this.Message = message;
        }

        public override string ToString()
        {
            return this.Message;
        }
    }
}
