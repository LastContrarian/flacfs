using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class MetadataBlockStreamInfo : MetadataBlock
    {
		const int MIN_BLOCK_SIZE = 16;
		const int MAX_BLOCK_SIZE = 65535;
		const int FRAME_SIZE_BITS_COUNT = 24;
		const int SAMPLE_RATE_BITS_COUNT = 20;
		const int CHANNELS_BITS_COUNT = 3;
		const int BPS_BITS_COUNT = 5;
		const int TOTAL_SAMPLES_BITS_COUNT = 36;

        ushort minBlockSizeInSamples_;
        ushort maxBlockSizeInSamples_;
        int minFrameSizeInBytes_;
        int maxFrameSizeInBytes_;
        int sampleRateInHz_;
        sbyte channelsMinusOne_;
        sbyte bitsPerSampleMinusOne_;
        long totalSamplesInStream_;
        uint [] md5Signature_ = new uint[4];

        public int BitsPerSample
        {
            get { return bitsPerSampleMinusOne_ + 1; }
        }

        public int MinBlockSize
        {
            get { return  minBlockSizeInSamples_; }
			set { minBlockSizeInSamples_ = (ushort)value; }
        }

        public int MaxBlockSize
        {
            get { return maxBlockSizeInSamples_; }
			set { maxBlockSizeInSamples_ = (ushort)value; }
        }

		public int MinFrameSize
		{
			set { minFrameSizeInBytes_ = value; }
		}

		public int MaxFrameSize
		{
			set { maxFrameSizeInBytes_ = value; }
		}

        public int Channels
        {
            get { return channelsMinusOne_ + 1; }
        }

		public long TotalSamples
		{
			set { totalSamplesInStream_ = value; }
		}

		public uint[] Checksum
		{
			get { return md5Signature_; }
		}

        public MetadataBlockStreamInfo(MetadataBlockHeader header)
            :base(header)
        {
        }

		public override void Read(FlacStream stream)
		{
			minBlockSizeInSamples_ = stream.Reader.ReadUShort();
			maxBlockSizeInSamples_ = stream.Reader.ReadUShort();

			Validation.IsValid(minBlockSizeInSamples_ >= MIN_BLOCK_SIZE && minBlockSizeInSamples_ <= MAX_BLOCK_SIZE);
			Validation.IsValid(maxBlockSizeInSamples_ >= MIN_BLOCK_SIZE && maxBlockSizeInSamples_ <= MAX_BLOCK_SIZE);

			minFrameSizeInBytes_ = stream.Reader.ReadBitsAsInt(FRAME_SIZE_BITS_COUNT);
			maxFrameSizeInBytes_ = stream.Reader.ReadBitsAsInt(FRAME_SIZE_BITS_COUNT);
			sampleRateInHz_ = stream.Reader.ReadBitsAsInt(SAMPLE_RATE_BITS_COUNT);
			channelsMinusOne_ = stream.Reader.ReadBitsAsSByte(CHANNELS_BITS_COUNT);
			bitsPerSampleMinusOne_ = stream.Reader.ReadBitsAsSByte(BPS_BITS_COUNT);
			totalSamplesInStream_ = stream.Reader.ReadBitsAsLong(TOTAL_SAMPLES_BITS_COUNT);

			md5Signature_[0] = stream.Reader.ReadUInt();
			md5Signature_[1] = stream.Reader.ReadUInt();
			md5Signature_[2] = stream.Reader.ReadUInt();
			md5Signature_[3] = stream.Reader.ReadUInt();
		}

		public override void Write(FlacStream stream)
		{
			Header.Write(stream);

			stream.Writer.WriteUShort(minBlockSizeInSamples_);
			stream.Writer.WriteUShort(maxBlockSizeInSamples_);

			stream.Writer.WriteBits(minFrameSizeInBytes_, FRAME_SIZE_BITS_COUNT);
			stream.Writer.WriteBits(maxFrameSizeInBytes_, FRAME_SIZE_BITS_COUNT);
			stream.Writer.WriteBits(sampleRateInHz_, SAMPLE_RATE_BITS_COUNT);
			stream.Writer.WriteBits(channelsMinusOne_, CHANNELS_BITS_COUNT);
			stream.Writer.WriteBits(bitsPerSampleMinusOne_, BPS_BITS_COUNT);
			stream.Writer.WriteBitsAsLong(totalSamplesInStream_,TOTAL_SAMPLES_BITS_COUNT);

			stream.Writer.WriteUInt(md5Signature_[0]);
			stream.Writer.WriteUInt(md5Signature_[1]);
			stream.Writer.WriteUInt(md5Signature_[2]);
			stream.Writer.WriteUInt(md5Signature_[3]);
		}
    }
}