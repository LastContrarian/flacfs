using System;
using System.Collections.Generic;
using System.Text;

namespace flac
{
	public static class Debug
	{
		public static void Assert(bool condition)
		{
#if DEBUG
			System.Diagnostics.Debug.Assert(condition);
#endif
		}
	}
}
