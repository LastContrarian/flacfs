using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    class FrameFooter
    {
		const int CRC16_BITS_COUNT = 16;

        ushort crc_;

		public void Read(FlacStream stream)
		{
			ushort crc = stream.Reader.Crc.GetCRC16();
			crc_ = stream.Reader.ReadUShort();

			Validation.IsValid(crc_ == crc);
		}

		public void Write(FlacStream stream)
		{
			crc_ = stream.Writer.Crc.GetCRC16();
			stream.Writer.WriteUShort(crc_);
		}
    }
}
