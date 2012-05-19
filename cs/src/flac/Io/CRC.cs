using System;
using System.Collections.Generic;
using System.Text;

using flac.Utils;
namespace flac.Io
{
	// this class handles both crc codes
	public class CRC
	{
		byte crc8_;
		short crc16_;
		int crcData_;
		int bitsInCrc_;

		bool crc16Running_;
		bool crc8Running_;

		public bool Running { get { return crc8Running_ || crc16Running_; } }

		public void Start()
		{
			crc8_ = 0;
			crc16_ = 0;
			crcData_ = 0;
			bitsInCrc_ = 0;
			crc8Running_ = true;
			crc16Running_ = true;
		}

		static readonly byte[] byteMask = new byte[] { 0x1, 0x3, 0x7, 0xf, 0x1f, 0x3f, 0x7f, 0xff };

		void UpdateCRC()
		{
			if (bitsInCrc_ >= Bit.BITS_IN_BYTE)
			{
				int rshift = bitsInCrc_ - Bit.BITS_IN_BYTE;
				byte b = (byte)((crcData_ >> rshift) & 0xff);

				if (crc8Running_)
				{
					crc8_ = CRC8.update(b, crc8_);
				}
				if (crc16Running_)
				{
					crc16_ = CRC16.update(b, crc16_);
				}

				bitsInCrc_ -= Bit.BITS_IN_BYTE;

				if (bitsInCrc_ > 0)
				{
					crcData_ &= byteMask[rshift - 1];
				}
				else
				{
					crcData_ >>= Bit.BITS_IN_BYTE;
				}
			}
		}

		public void AddBit(sbyte b)
		{
			crcData_ = crcData_ << 1 | (byte)b;
			bitsInCrc_++;
			UpdateCRC();
		}

		public void AddByte(byte b)
		{
			crcData_ = crcData_ << Bit.BITS_IN_BYTE | b;
			bitsInCrc_ += Bit.BITS_IN_BYTE;
			UpdateCRC();
		}

		public byte GetCRC8()
		{
			Debug.Assert(bitsInCrc_ == 0);
			crc8Running_ = false;
			return crc8_;
		}

		public ushort GetCRC16()
		{
			Debug.Assert(bitsInCrc_ == 0);
			crc16Running_ = false;
			return (ushort)crc16_;
		}
	}
}
