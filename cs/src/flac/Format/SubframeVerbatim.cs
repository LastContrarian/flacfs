using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
	public class SubframeVerbatim : Subframe
	{
		int bitsPerSampleInFrame_;
		int frameBlockSize_;

		public void Read(FlacStream stream, int bitsPerSampleInFrame, int frameBlockSize)
		{
			bitsPerSampleInFrame_ = bitsPerSampleInFrame;
			frameBlockSize_ = frameBlockSize;

			// read data
			samples_ = new uint[frameBlockSize_];

			int i = 0;
			while (i < frameBlockSize_)
			{
				samples_[i] = stream.Reader.ReadBitsAsUInt(bitsPerSampleInFrame_);
				i++;
			}
		}

		public override void Write(FlacStream stream)
		{
			Write_(stream, frameBlockSize_);
		}

		public override void Write(FlacStream stream, int count)
		{
			Write_(stream, count);
		}

		private void Write_(FlacStream stream, int count)
		{
			base.Write(stream); ;

			int i = 0;
			while (i < count)
			{
				stream.Writer.WriteBits(samples_[i], bitsPerSampleInFrame_);
				i++;
			}
		}
	}
}
