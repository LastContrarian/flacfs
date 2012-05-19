using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace flac.Io
{
	public class FlacStreamWriter
	{
		const int MAX_BITS_IN_CACHE = 8;

		static readonly byte[] MASK = new byte[] { 0x1, 0x3, 0x7, 0xf, 0x1f, 0x3f, 0x7f, 0xff };

		FileStream stream_;
		CRC crc_;

		byte cache_;
		int bitsInCache_;

		public long Debug_BytesWritten { get { return stream_.Position; } }
		public long Debug_BitsWritten { get { return stream_.Position * 8 + bitsInCache_; } }
		public CRC Crc { get { return crc_; } }

		public int BitsToByteBoundary { get { return Bit.ModBy8(Bit.BITS_IN_BYTE - Bit.ModBy8(bitsInCache_)); } }

		public FlacStreamWriter(FileStream stream)
		{
			stream_ = stream;
			crc_ = new CRC();
		}

		public void Truncate(long offset)
		{
			// seeks and clears everything
			//stream_.Seek(offset, SeekOrigin.Begin);
			stream_.SetLength(0);
			cache_ = 0;
			bitsInCache_ = 0;
		}

		void Flush()
		{
			stream_.WriteByte(cache_);
			if (crc_.Running)
			{
				crc_.AddByte(cache_);
			}
			bitsInCache_ = 0;
			cache_ = 0;
		}

		void putBit(byte bit)
		{
			cache_ = (byte)(cache_ << 1 | bit);
			bitsInCache_++;
			if (bitsInCache_ == MAX_BITS_IN_CACHE)
			{
				Flush();
			}
		}

		void putByte(byte b)
		{
			ushort cache = (ushort)(cache_ << Bit.BITS_IN_BYTE | b);
			cache_ = (byte)(cache >> bitsInCache_);

			int cached = bitsInCache_;
			int diff = Bit.BITS_IN_BYTE - bitsInCache_;
			cache = (byte)(((byte)(cache << diff)) >> diff);
			
			Flush();
			cache_ = (byte)cache;
			bitsInCache_ = Bit.ModBy8(cached);
		}

		// between a bit and a byte
		void putBits(byte bits, int bitCount)
		{
			Debug.Assert(bitCount >= 0 && bitCount < Bit.BITS_IN_BYTE);

			int rshift = bitCount - 1;
			while (rshift >= 0)
			{
				bits &= MASK[rshift];
				byte bit = (byte)(bits >> rshift);
				rshift--;
				putBit(bit);
			}
		}

		public void WriteBits(uint bits, int bitCount)
		{
			if (bitCount > Bit.BITS_IN_WORD)
			{
				int c = Bit.BITS_IN_DWORD - bitCount;
				uint b = bits << c;
				WriteUShort((ushort)(b >> Bit.BITS_IN_WORD));
				bitCount -= Bit.BITS_IN_WORD;
				bits = (b & 0xffff) >> c;
			}
			if (bitCount > Bit.BITS_IN_BYTE)
			{
				int c = Bit.BITS_IN_WORD - bitCount;
				ushort b = (ushort)(bits << c);
				WriteByte((byte)(b >> Bit.BITS_IN_BYTE));
				bitCount -= Bit.BITS_IN_BYTE;
				bits = (uint)((b & 0xff) >> c);
			}
			if (bitCount == Bit.BITS_IN_BYTE)
			{
				putByte((byte)bits);
				bitCount -= Bit.BITS_IN_BYTE;
			}
			putBits((byte)bits, bitCount);
		}

		private void WriteBitsAsULong(ulong bits, int bitCount)
		{
			if (bitCount > Bit.BITS_IN_DWORD)
			{
				int c = Bit.BITS_IN_LONG - bitCount;
				ulong b = bits << c;
				WriteUInt((uint)(b >> Bit.BITS_IN_DWORD));
				bitCount -= Bit.BITS_IN_DWORD;
				bits = (b & 0xffffffff) >> c;
			}
			WriteBits((uint)bits, bitCount);
		}

		public void WriteBits(int bits, int bitCount)
		{
			WriteBits((uint)bits, bitCount);
		}

		public void WriteBitsAsLong(long bits, int bitCount)
		{
			WriteBitsAsULong((ulong)bits, bitCount);
		}

		// full values
		public void WriteBit(sbyte bit)
		{
			putBit((byte)bit);
		}

		public void WriteByte(byte b)
		{
			putByte(b);
		}

		public void WriteUShort(ushort bits)
		{
			byte b;

			b = (byte)(bits >> Bit.BITS_IN_BYTE);
			putByte(b);

			b = (byte)(bits & 0xff);
			putByte(b);
		}

		public void WriteUInt(uint bits)
		{
			ushort s;

			s = (ushort)(bits >> Bit.BITS_IN_WORD);
			WriteUShort(s);

			s = (ushort)(bits & 0xffff);
			WriteUShort(s);
		}

		public void WriteString(string s, int bitCount)
		{
			int remainder;
			int bytes = Math.DivRem(bitCount, Bit.BITS_IN_BYTE, out remainder);
			Debug.Assert(remainder == 0);

			int i = 0;
			while (i < bytes)
			{
				putByte((byte)s[i]);
				i++;
			}
		}

		public void WriteUnaryBitStream(int bitCount)
		{
			while (bitCount >= Bit.BITS_IN_BYTE)
			{
				putByte(0);
				bitCount -= Bit.BITS_IN_BYTE;
			}
			while (bitCount > 0)
			{
				putBit(0);
				bitCount--;
			}
			putBit(1);
		}
	}
}
