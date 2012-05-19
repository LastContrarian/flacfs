using System;
using System.Collections.Generic;
using System.Text;

namespace flac
{
    public class FlacException : Exception
    {
        public FlacException()
        {
        }

        public FlacException(string message)
            : base(message)
        {
        }
    }
}
