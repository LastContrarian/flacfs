using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class Signature
    {
        const string SIGNATURE = "fLaC";
        const int SIGNATURE_BITS_COUNT = 32;

		public void Read(FlacStream stream)
		{
			Validation.IsValid(stream.Reader.ReadString(SIGNATURE_BITS_COUNT) == SIGNATURE);
		}

		public void Write(FlacStream stream)
		{
			stream.Writer.WriteString(SIGNATURE, SIGNATURE_BITS_COUNT);
		}
    }
}
