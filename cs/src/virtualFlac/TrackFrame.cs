using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VirtualFlac
{
	public class TrackFrame
	{
		int virtualBlockSizeDifference_; // positive value. to be reduced from imageBlockSize

		public long virtualSize { get { return imageSize_ + sizeDifference; } }
		public long imageOffset { get { return imageOffset_; } }
		public long imageStartSample { get { return imageStartSample_; } }
		public long imageEndSample { get { return imageStartSample_ + imageBlockSize; } }
		public int virtualBlockSize { get { return imageBlockSize - virtualBlockSizeDifference_; } }

		//
		public long virtualOffset;
		public int sizeDifference;
		public long startSample;
		long imageOffset_;
		int imageSize_;
		long imageStartSample_;
		public int imageBlockSize;

		public TrackFrame(BinaryReader br, long offsetDifference)
		{
			Load(br, offsetDifference);
		}

		public TrackFrame(long offset, int frameSize, long startSample)
		{
			imageOffset_ = offset;
			imageSize_ = frameSize;
			imageStartSample_ = startSample;
		}

		public void CalculateVirtualBlockSizeDifference(long sampleNumber, bool isFirstFrame)
		{
			if (isFirstFrame)
			{
				// sample number is track start sample
				virtualBlockSizeDifference_ = imageBlockSize - Convert.ToInt32(imageEndSample - sampleNumber);
			}
			else
			{
				// sample number is track end sample
				virtualBlockSizeDifference_ = imageBlockSize - Convert.ToInt32(sampleNumber - imageStartSample);
			}
		}

		public void Save(BinaryWriter bw)
		{
			bw.Write(virtualOffset);
			bw.Write(sizeDifference);
			bw.Write(startSample);
			bw.Write(imageOffset_);
			bw.Write(imageSize_);
			bw.Write(imageStartSample_);
			bw.Write(imageBlockSize);
		}

		void Load(BinaryReader br, long offsetDifference)
		{
			virtualOffset = br.ReadInt64() + offsetDifference;
			sizeDifference = br.ReadInt32();
			startSample = br.ReadInt64();
			imageOffset_ = br.ReadInt64();
			imageSize_ = br.ReadInt32();
			imageStartSample_ = br.ReadInt64();
			imageBlockSize = br.ReadInt32();
		}
	}
}
