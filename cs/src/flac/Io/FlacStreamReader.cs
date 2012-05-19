using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace flac.Io
{
	public class FlacStreamReader
	{
		FileStream stream_;
		CRC crc_;

		byte cache_;
		int bitsInCache_;

		public long Debug_BytesRead { get { return stream_.Position; } }
		public long Debug_BitsRead { get { return stream_.Position * Bit.BITS_IN_BYTE - bitsInCache_; } }

		public long BytesRemaining { get { return stream_.Length - stream_.Position; } }
		public bool Done { get { return BytesRemaining == 0; } }
		public int BitsToByteBoundary { get { return Bit.ModBy8(bitsInCache_); } }

		public CRC Crc { get { return crc_; } }

		public FlacStreamReader(FileStream stream)
		{
			stream_ = stream;
			crc_ = new CRC();
		}

		public void Seek(long offset)
		{
			// seeks and clears everything
			stream_.Seek(offset, SeekOrigin.Begin);
			cache_ = 0;
			bitsInCache_ = 0;
		}

		void Update()
		{
			cache_ = (byte)stream_.ReadByte();
			bitsInCache_ = Bit.BITS_IN_BYTE;
			if (crc_.Running)
			{
				crc_.AddByte(cache_);
			}
		}

		byte getBit()
		{
			if (bitsInCache_ == 0)
			{
				Update();
			}
			byte retval = (byte)(cache_ >> Bit.BITS_IN_BYTE - 1);
			cache_ <<= 1;
			bitsInCache_ -= 1;
			return retval;
		}

		byte getByte()
		{
			// save current pos
			byte cachedByte = cache_;
			int cached = bitsInCache_;
			int diff = Bit.BITS_IN_BYTE - cached;

			// get new byte
			Update();

			byte retval = (byte)(cachedByte | cache_ >> cached);
			cache_ <<= diff;
			bitsInCache_ -= diff;
			return retval;
		}

		private byte getBits(int bitCount)
		{
			Debug.Assert(bitCount >= 0 && bitCount < Bit.BITS_IN_BYTE);
			byte retval = 0;
			while (bitCount > 0)
			{
				retval = (byte)(retval << 1 | getBit());
				bitCount--;
			}
			return retval;
		}

		public uint ReadBits(int bitCount)
		{
			uint retval = 0;
			if (bitCount >= Bit.BITS_IN_WORD)
			{
				retval = ReadUShort();
				bitCount -= Bit.BITS_IN_WORD;
			}
			if (bitCount >= Bit.BITS_IN_BYTE)
			{
				retval = retval << Bit.BITS_IN_BYTE | getByte();
				bitCount -= Bit.BITS_IN_BYTE;
			}
			return retval << bitCount | getBits(bitCount);
		}

		public ulong ReadBitsAsULong(int bitCount)
		{
			ulong retval = 0;
			if (bitCount >= Bit.BITS_IN_DWORD)
			{
				retval = ReadUInt();
				bitCount -= Bit.BITS_IN_DWORD;
			}
			return retval << bitCount | ReadBits(bitCount);
		}

		public sbyte ReadBitsAsSByte(int bitCount)
		{
			return (sbyte)getBits(bitCount);
		}

		public short ReadBitsAsShort(int bitCount)
		{
			return (short)ReadBits(bitCount);
		}

		public ushort ReadBitsAsUShort(int bitCount)
		{
			return (ushort)ReadBits(bitCount);
		}

		public int ReadBitsAsInt(int bitCount)
		{
			return (int)ReadBits(bitCount);
		}

		public uint ReadBitsAsUInt(int bitCount)
		{
			return ReadBits(bitCount);
		}

		public long ReadBitsAsLong(int bitCount)
		{
			return (long)ReadBitsAsULong(bitCount);
		}

		// full values
		public sbyte ReadBit()
		{
			return (sbyte)getBit();
		}

		public byte ReadByte()
		{
			return getByte();
		}

		public ushort ReadUShort()
		{
			return (ushort)(getByte() << Bit.BITS_IN_BYTE | getByte());
		}

		public uint ReadUInt()
		{
			uint retval = getByte();
			retval = retval << Bit.BITS_IN_BYTE | getByte();
			retval = retval << Bit.BITS_IN_BYTE | getByte();
			retval = retval << Bit.BITS_IN_BYTE | getByte();
			return retval;
		}

		public int ReadInt()
		{
			return (int)ReadUInt();
		}

		public string ReadString(int bitCount)
		{
			string s = string.Empty;
			int remainder;
			int bytes = Math.DivRem(bitCount, Bit.BITS_IN_BYTE, out remainder);
			Debug.Assert(remainder == 0);

			while (bytes > 0)
			{
				s += (char)getByte();
				bytes--;
			}
			return s;
		}

		public void SkipToByteBoundary()
		{
			ReadBits(BitsToByteBoundary);
		}

		public int GetUnaryBitStreamLength()
		{
			int count = 0;
			while (!(getBit() == 1))
			{
				count++;
			}
			return count;
		}

		public void SkipBits(int bitCount)
		{
			int bitsToByteBoundary = BitsToByteBoundary;
			if (bitCount - bitsToByteBoundary > 0)
			{
				getBits(bitsToByteBoundary);
				bitCount -= bitsToByteBoundary;

				cache_ = 0;
				bitsInCache_ = 0;

				int bytesToAdvance = Bit.DivideBy8(bitCount);
				stream_.Position += bytesToAdvance;
				bitCount = Bit.ModBy8(bitCount);
			}
			getBits(bitCount);
		}
	}
}
