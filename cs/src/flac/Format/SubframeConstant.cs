using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class SubframeConstant : Subframe
    {
		int bitsPerSampleInFrame_;

		public void Read(FlacStream stream, int bitsPerSampleInFrame)
		{
			bitsPerSampleInFrame_ = bitsPerSampleInFrame;

			// read data
			samples_ = new uint[] { stream.Reader.ReadBitsAsUInt(bitsPerSampleInFrame_) };
		}

		public override void Write(FlacStream stream)
		{
			base.Write(stream);

			stream.Writer.WriteBits(samples_[0], bitsPerSampleInFrame_);
		}

		public override void Write(FlacStream stream, int count)
		{
			Write(stream);
		}
    }
}
