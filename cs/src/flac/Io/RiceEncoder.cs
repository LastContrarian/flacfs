using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Io
{
	public static class RiceEncoder
	{
		public static void PutResidual(FlacStream stream, int residual, sbyte parameter)
		{
			// number : +48, param: 4
			// step 1: 48
			// step 2: 110000
			// step 3: 110000 0
			// step 4: 110 0000
			// step 5: 0000001 0000

			// number : -48, param: 4
			// step 1: 47
			// step 2: 101111
			// step 3: 101111 1
			// step 4: 101 1111
			// step 5: 000001 1111

			// number : 0, param: 4
			// step 1: 0
			// step 2: 0
			// step 3: 0 0
			// step 4:  0000
			// step 5: 1 0000

			if (residual < 0)
			{
				residual = (residual * -1) - 1;
				residual = (residual << 1) | 1;
			}
			else
			{
				residual <<= 1;
			}

			int msbValue = residual >> parameter;
			int lsbValue = residual - (msbValue << parameter);

			Debug.Assert(msbValue < short.MaxValue);
			stream.Writer.WriteUnaryBitStream(msbValue);

			stream.Writer.WriteBits(lsbValue, parameter);
		}
	}
}
