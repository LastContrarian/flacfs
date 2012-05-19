using System;
using System.Collections.Generic;
using System.Text;

namespace flac
{
    public class FlacFormatException : FlacException
    {
        public FlacFormatException()
        {
        }

        public FlacFormatException(string message)
            : base(message)
        {
        }
    }
}
