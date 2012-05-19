using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Io
{
	public class UTF8Encoder
	{
		public static byte [] GetUtf8UInt(int val)
		{
			List<byte> encodedValue = new List<byte>();

			if (val < 0x80)
			{
				encodedValue.Add((byte)val);
			}
			else if (val < 0x800)
			{
				encodedValue.Add((byte)(0xC0 | val >> 6));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x10000)
			{
				encodedValue.Add((byte)(0xE0 | val >> 12));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x200000)
			{
				encodedValue.Add((byte)(0xF0 | val >> 18));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x4000000)
			{
				encodedValue.Add((byte)(0xF8 | val >> 24));
				encodedValue.Add((byte)(0x80 | val >> 18 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else
			{
				encodedValue.Add((byte)(0xFC | val >> 30));
				encodedValue.Add((byte)(0x80 | val >> 24 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 18 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			return encodedValue.ToArray();
		}

		public static byte [] GetUtf8ULong(long val)
		{
			List<byte> encodedValue = new List<byte>();
			if (val < 0x80)
			{
				encodedValue.Add((byte)val);
			}
			else if (val < 0x800)
			{
				encodedValue.Add((byte)(0xC0 | val >> 6));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x10000)
			{
				encodedValue.Add((byte)(0xE0 | val >> 12));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x200000)
			{
				encodedValue.Add((byte)(0xF0 | val >> 18));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x4000000)
			{
				encodedValue.Add((byte)(0xF8 | val >> 24));
				encodedValue.Add((byte)(0x80 | val >> 18 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else if (val < 0x80000000)
			{
				encodedValue.Add((byte)(0xFC | val >> 30));
				encodedValue.Add((byte)(0x80 | val >> 24 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 18 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			else
			{
				encodedValue.Add(0xFE);
				encodedValue.Add((byte)(0x80 | val >> 30 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 24 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 18 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 12 & 0x3F));
				encodedValue.Add((byte)(0x80 | val >> 6 & 0x3F));
				encodedValue.Add((byte)(0x80 | val & 0x3F));
			}
			return encodedValue.ToArray();
		}
	}
}
