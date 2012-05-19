using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Io
{
	public static class RiceDecoder
	{
		const int MAX_RICE_PARAMETER = 14;

		public static int GetResidual(FlacStream stream, sbyte parameter)
		{
			Debug.Assert(parameter <= MAX_RICE_PARAMETER);

			int msbValue;
			int lsbValue;

			msbValue = stream.Reader.GetUnaryBitStreamLength();

			if (parameter > 0)
			{
				lsbValue = (int)stream.Reader.ReadBits(parameter);
			}
			else
			{
				lsbValue = 0;
			}

			int residual = (msbValue << parameter) | lsbValue;
			if ((residual & 1) == 1)
			{
				residual = ((residual >> 1) * -1) - 1;
			}
			else
			{
				residual >>= 1;
			}

			return residual;
		}
	}
}
