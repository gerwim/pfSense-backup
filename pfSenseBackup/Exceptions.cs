
using System;
using System.Collections.Generic;
using System.Text;

namespace pfSenseBackup
{
    public class UsernamePasswordInvalidException : Exception
    {
        public UsernamePasswordInvalidException(string message)
        : base(message)
        {
        }

        public UsernamePasswordInvalidException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
