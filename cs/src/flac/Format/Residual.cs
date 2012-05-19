using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class Residual
    {
		const int CODING_METHOD_BITS_COUNT = 2;

        sbyte codingMethod_;
        ResidualCodingMethod method_;

		public void Read(FlacStream stream, int blockSize, int predictorOrder, ref uint [] samples)
		{
			codingMethod_ = stream.Reader.ReadBitsAsSByte(CODING_METHOD_BITS_COUNT);

			switch (codingMethod_)
			{
				case 0:
				{
					method_ = new ResidualCodingMethodPartitionedRice();
					method_.Read(stream, blockSize, predictorOrder, ref samples);
					break;
				}
				case 1:
				{
					throw new FlacNotImplementedException();
				}
				default:
				{
					throw new FlacFormatReservedException();
				}
			}
		}

		public void Write(FlacStream stream, ref uint[] samples)
		{
			Write_(stream);
			method_.Write(stream, ref samples);
		}

		public void Write(FlacStream stream, int count, ref uint[] samples)
		{
			Write_(stream);
			method_.Write(stream, count, ref samples);
		}

		private void Write_(FlacStream stream)
		{
			stream.Writer.WriteBits(codingMethod_, CODING_METHOD_BITS_COUNT);
		}
    }
}
