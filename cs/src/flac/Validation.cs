using System;
using System.Collections.Generic;
using System.Text;

namespace flac
{
	public static class Validation
	{
		public static void IsReserved(bool condition)
		{
			if (!condition)
			{
				throw new FlacFormatReservedException();
			}
		}

		public static void IsValid(bool condition)
		{
			if (!condition)
			{
				throw new FlacFormatInvalidException();
			}
		}
	}
}
