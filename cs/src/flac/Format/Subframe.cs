using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public abstract class Subframe
    {
        SubframeHeader header_;
		protected uint [] samples_;

		public uint[] Samples
		{
			get { return samples_; }
		}

        public SubframeHeader Header
        {
            get { return header_; }
        }

		public virtual void Write(FlacStream stream)
		{
			header_.Write(stream);
		}

		public abstract void Write(FlacStream stream, int count);

		public static Subframe New(FlacStream stream, FrameHeader frameHeader, int bitsPerSampleInFrame)
		{
			Subframe sf;
			SubframeHeader header = new SubframeHeader();
			header.Read(stream);

			int subframeType = header.SubframeType;
			if (subframeType == 0)
			{
				// constant
				SubframeConstant subframe = new SubframeConstant();
				subframe.header_ = header;
				subframe.Read(stream, bitsPerSampleInFrame);
				sf = subframe;
			}
			else if (subframeType == 1)
			{
				// verbatim
				SubframeVerbatim subframe = new SubframeVerbatim();
				subframe.header_ = header;
				subframe.Read(stream, bitsPerSampleInFrame, frameHeader.BlockSize);
				sf = subframe;
			}
			else if (subframeType >= 2 && subframeType <= 7)
			{
				throw new FlacFormatReservedException();
			}
			else if (subframeType >= 8 && subframeType <= 15)
			{
				// fixed
				int predictorOrder = subframeType & 7;
				Validation.IsReserved(predictorOrder <= 4);

				SubframeFixed subframe = new SubframeFixed();
				subframe.header_ = header;
				subframe.Read(stream, frameHeader, predictorOrder, bitsPerSampleInFrame);
				sf = subframe;
			}
			else if (subframeType == 16)
			{
				throw new FlacFormatReservedException();
			}
			else
			{
				int lpcOrder = (subframeType & 0x1f) + 1;

				SubframeLpc subframe = new SubframeLpc();
				subframe.header_ = header;
				subframe.Read(stream, frameHeader, lpcOrder, bitsPerSampleInFrame);
				sf = subframe;
			}
			return sf;
		}
    }
}
