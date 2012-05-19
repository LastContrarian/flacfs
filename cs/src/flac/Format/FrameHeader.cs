using System;
using System.Collections.Generic;
using System.Text;

using flac.Io;
namespace flac.Format
{
    public class FrameHeader
    {
        const ushort SYNC_CODE = 0x3ffe;  // 0x11111111111110b
		const int SYNC_CODE_BITS_COUNT = 14;
		const int BLOCK_SIZE_BITS_COUNT = 4;
		const int SAMPLE_RATE_BITS_COUNT = 4;
		const int CHANNELS_BITS_COUNT = 4;
		const int BPS_BITS_COUNT = 3;

        public const int BLOCKING_STRATEGY_FIXED = 0;
		public const int BLOCKING_STRATEGY_VARIABLE = 1;

        sbyte blockingStrategy_;
        sbyte blockSizeInInterChannelSamples_;
        sbyte sampleRate_;
        sbyte channelAssignment_;
        sbyte bitsPerSample_;
        ushort blockSizeHint_;
        ushort sampleRateHint_;
        byte crc_;

		//
        long decodedFrameOrSampleNumber_;
        MetadataBlockStreamInfo streamInfo_;
		int sampleIdSize_;

		public sbyte BlockingStrategy
		{
			get { return blockingStrategy_; }
			set { blockingStrategy_ = value; }
		}

		public int SampleIdSize
		{
			get { return sampleIdSize_; }
		}

        public long StartingSampleNumber
        {
            get
            {
                if (blockingStrategy_ == BLOCKING_STRATEGY_FIXED)
                {
                    return decodedFrameOrSampleNumber_ * streamInfo_.MinBlockSize;
                }
                else
                {
                    return decodedFrameOrSampleNumber_;
                }
            }
			set
			{
				decodedFrameOrSampleNumber_ = value;
			}
        }

        public int BitsPerSample
        {
            get
            {
                int bps = 0;
                switch (bitsPerSample_)
                {
                    case 0:
                    {
                        bps = streamInfo_.BitsPerSample;
                        break;
                    }
                    case 1:
                    {
                        bps = 8;
                        break;
                    }
                    case 2:
                    {
                        bps = 12;
                        break;
                    }
                    case 4:
                    {
                        bps = 16;
                        break;
                    }
                    case 5:
                    {
                        bps = 20;
                        break;
                    }
                    case 6:
                    {
                        bps = 24;
                        break;
                    }
                    case 3:
                    case 7:
                    {
                       throw new FlacFormatReservedException();
                    }
                    default:
                    {
                        throw new FlacDebugException();
                    }
                }
                return bps;
            }
        }

        public int BlockSize
        {
            get
            {
                int blockSize = 0;
                int b = blockSizeInInterChannelSamples_;
                if (b == 0)
                {
                    throw new FlacFormatReservedException();
                }
                else if (b == 1)
                {
                    blockSize = 192;
                }
                else if (b >= 2 && b <= 5)
                {
                    blockSize = 576 * (1 << (b - 2)); // 2 ^ (n - 2)
                }
                else if (b == 6 || b == 7)
                {
                    blockSize = blockSizeHint_ + 1;
                }
                else if (b >= 8 && b <= 15)
                {
                    blockSize = 256 * (1 << (b - 8)); // 2 ^ (n - 8)
                }
                else
                {
                    throw new FlacDebugException();
                }
                return blockSize;
            }

			set
			{
				int blockSize = value;
				Debug.Assert(blockSize != 0);

				blockSizeInInterChannelSamples_ = 0;

				if (blockSize == 192)
				{
					blockSizeInInterChannelSamples_ = 1;
				}
				else
				{
					int power = 0;
					while (power < 4)
					{
						if (blockSize == 576 * (1 << power))
						{
							blockSizeInInterChannelSamples_ = (sbyte)(power + 2);
							break;
						}
						power++;
					}

					if (blockSizeInInterChannelSamples_ == 0)
					{
						power = 0;
						while (power < 8)
						{
							if (blockSize == 256 * (1 << power))
							{
								blockSizeInInterChannelSamples_ = (sbyte)(power + 8);
								break;
							}
							power++;
						}

						if (blockSizeInInterChannelSamples_ == 0)
						{
							if (blockSize > byte.MaxValue)
							{
								blockSizeInInterChannelSamples_ = 7;
							}
							else
							{
								blockSizeInInterChannelSamples_ = 6;
							}
							blockSizeHint_ = (ushort)(blockSize - 1);
						}
					}
				}
			}
        }

        public int ChannelAssignment { get { return channelAssignment_; } }

        public int Channels
        {
            get
            {
                int ca = channelAssignment_;
                if (ca >= 0 && ca <= 7)
                {
                    return ca + 1;
                }
                else if (ca >= 8 && ca <= 10)
                {
                    return 2;
                }
                else
                {
                    throw new FlacFormatReservedException();
                }
            }
        }

		public void Read(FlacStream stream)
		{
			stream.Reader.Crc.Start();
			streamInfo_ = stream.StreamInfo;
			Validation.IsValid(stream.Reader.ReadBitsAsShort(SYNC_CODE_BITS_COUNT) == SYNC_CODE);

			// reserved
			sbyte reserved0 = stream.Reader.ReadBit();
			Validation.IsReserved(reserved0 == 0);

			blockingStrategy_ = stream.Reader.ReadBit();
			blockSizeInInterChannelSamples_ = stream.Reader.ReadBitsAsSByte(BLOCK_SIZE_BITS_COUNT);
			sampleRate_ = stream.Reader.ReadBitsAsSByte(SAMPLE_RATE_BITS_COUNT);
			channelAssignment_ = stream.Reader.ReadBitsAsSByte(CHANNELS_BITS_COUNT);
			bitsPerSample_ = stream.Reader.ReadBitsAsSByte(BPS_BITS_COUNT);

			// reserved
			sbyte reserved1 = stream.Reader.ReadBit();
			Validation.IsReserved(reserved1 == 0);

			long pos = stream.Reader.Debug_BytesRead;
			DecodeSampleNumber(stream);
			sampleIdSize_ = (int)(stream.Reader.Debug_BytesRead - pos);

			if (blockSizeInInterChannelSamples_ == 6)
			{
				blockSizeHint_ = stream.Reader.ReadByte();
			}
			else if (blockSizeInInterChannelSamples_ == 7)
			{
				blockSizeHint_ = stream.Reader.ReadUShort();
			}
			else
			{
				// is zero
			}

			if (sampleRate_ == 0xc)
			{
				sampleRateHint_ = stream.Reader.ReadByte();
			}
			else if (sampleRate_ == 0xd || sampleRate_ == 0xe)
			{
				sampleRateHint_ = stream.Reader.ReadUShort();
			}
			else
			{
				// is zero
			}

			byte crc = stream.Reader.Crc.GetCRC8();
			crc_ = stream.Reader.ReadByte();

			Validation.IsValid(crc_ == crc);
		}

		public void Write(FlacStream stream)
		{
			stream.Writer.Crc.Start();

			// sync
			stream.Writer.WriteBits(SYNC_CODE, SYNC_CODE_BITS_COUNT);

			// reserved
			stream.Writer.WriteBit(0);

			stream.Writer.WriteBit(blockingStrategy_);

			stream.Writer.WriteBits(blockSizeInInterChannelSamples_, BLOCK_SIZE_BITS_COUNT);
			stream.Writer.WriteBits(sampleRate_, SAMPLE_RATE_BITS_COUNT);
			stream.Writer.WriteBits(channelAssignment_, CHANNELS_BITS_COUNT);
			stream.Writer.WriteBits(bitsPerSample_, BPS_BITS_COUNT);

			// reserved
			stream.Writer.WriteBit(0);

			byte [] encodedValue;
			switch (blockingStrategy_)
			{
				case BLOCKING_STRATEGY_FIXED:
				{
					encodedValue = UTF8Encoder.GetUtf8UInt((int)decodedFrameOrSampleNumber_);
					break;
				}
				case BLOCKING_STRATEGY_VARIABLE:
				{
					encodedValue = UTF8Encoder.GetUtf8ULong(decodedFrameOrSampleNumber_);
					break;
				}
				default:
				{
					throw new FlacDebugException();
				}
			}
			foreach (byte b in encodedValue)
			{
				stream.Writer.WriteByte(b);
			}

			if (blockSizeInInterChannelSamples_ == 6)
			{
				stream.Writer.WriteByte((byte)blockSizeHint_);
			}
			else if (blockSizeInInterChannelSamples_ == 7)
			{
				stream.Writer.WriteUShort(blockSizeHint_);
			}
			else
			{
				// is zero
			}

			if (sampleRate_ == 0xc)
			{
				stream.Writer.WriteByte((byte)sampleRateHint_);
			}
			else if (sampleRate_ == 0xd || sampleRate_ == 0xe)
			{
				stream.Writer.WriteUShort(sampleRateHint_);
			}
			else
			{
				// is zero
			}

			crc_ = stream.Writer.Crc.GetCRC8();
			stream.Writer.WriteByte(crc_);
		}

		private void DecodeSampleNumber(FlacStream stream)
		{
			switch (blockingStrategy_)
			{
				case BLOCKING_STRATEGY_FIXED:
				{
					Validation.IsValid(streamInfo_.MinBlockSize == streamInfo_.MaxBlockSize);

					// fixed block size stream
					UInt32 x;
					UTF8Decoder.ReadUtf8UInt(stream.Reader, out x);

					Validation.IsValid(x != 0xffffffff);

					decodedFrameOrSampleNumber_ = x;
					break;
				}
				case BLOCKING_STRATEGY_VARIABLE:
				{
					// variable block size stream
					UInt64 xx;
					UTF8Decoder.ReadUtf8ULong(stream.Reader, out xx);
					
					Validation.IsValid(xx != 0xffffffffffffffff);

					decodedFrameOrSampleNumber_ = Convert.ToInt64(xx);
					break;
				}
				default:
				{
					throw new FlacDebugException();
				}
			}
		}
    }
}