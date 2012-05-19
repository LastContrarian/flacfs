using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class SubframeHeader
    {
		const int SUBFRAME_TYPE_BITS_COUNT = 6;
		const sbyte WASTED_BITS_EXIST_FLAG = 1;

        sbyte subframeType_;
        sbyte wastedBitsPerSampleFlag_; // wasted "bits-per-sample"

        int unaryWastedBitsPerSampleCount_;

        public int SubframeType
        {
            get { return subframeType_; }
        }

		public void Read(FlacStream stream)
		{
			sbyte reserved0 = stream.Reader.ReadBit();
			Validation.IsReserved(reserved0 == 0);

			subframeType_ = stream.Reader.ReadBitsAsSByte(SUBFRAME_TYPE_BITS_COUNT);
			wastedBitsPerSampleFlag_ = stream.Reader.ReadBit();

			unaryWastedBitsPerSampleCount_ = 0;
			if (wastedBitsPerSampleFlag_ == WASTED_BITS_EXIST_FLAG)
			{
				unaryWastedBitsPerSampleCount_ = stream.Reader.GetUnaryBitStreamLength();
			}
		}

		public void Write(FlacStream stream)
		{
			// reserved
			stream.Writer.WriteBit(0);

			stream.Writer.WriteBits(subframeType_, SUBFRAME_TYPE_BITS_COUNT);
			stream.Writer.WriteBit(wastedBitsPerSampleFlag_);

			if (wastedBitsPerSampleFlag_ == WASTED_BITS_EXIST_FLAG)
			{
				stream.Writer.WriteUnaryBitStream(unaryWastedBitsPerSampleCount_);
			}
		}
    }
}
