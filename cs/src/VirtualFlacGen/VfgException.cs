using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualFlacGen
{
	class VfgException : Exception
	{
		public VfgException(string message)
			:base(message)
		{
		}
	}
}
