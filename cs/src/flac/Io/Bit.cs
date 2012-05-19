using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Io
{
    public static class Bit
    {
        public const int BITS_IN_BYTE = 8;
		public const int BITS_IN_WORD = 16;
		public const int BITS_IN_DWORD = 32;
		public const int BITS_IN_LONG = 64;

        public static int DivideBy8(int val)
        {
            return val >> 3;
        }

        public static long DivideBy8L(long val)
        {
            return val >> 3;
        }

        public static long MultiplyBy8(long val)
        {
            return val << 3;
        }

		public static int MultiplyBy8I(int val)
		{
			return val << 3;
		}

        public static int ModBy8(long val)
        {
            return (int)(val & 7);
        }
    }
}
