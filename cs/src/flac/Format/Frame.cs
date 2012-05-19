using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class Frame
    {
        FrameHeader header_ = new FrameHeader();
        List<Subframe> subframes_ = new List<Subframe>();
        FrameFooter footer_ = new FrameFooter();

        public FrameHeader Header
        {
            get { return header_; }
        }

		public void ReadHeader(FlacStream stream)
		{
			header_.Read(stream);
		}

		public void ReadData(FlacStream stream)
		{
			Debug.Assert(subframes_.Count == 0);

			// one subframe for each channel
			int channels = stream.StreamInfo.Channels;
			Validation.IsValid(channels >= 1);

			while (channels > 0)
			{
				int bps = GetBps();

				Subframe subframe = Subframe.New(stream, header_, bps);
				subframes_.Add(subframe);
				channels--;
			}

			// zero padding
			stream.Reader.SkipToByteBoundary();
		}

		public void ReadFooter(FlacStream stream)
		{
			footer_.Read(stream);
		}

		public void Write(FlacStream stream)
		{
			header_.Write(stream);

			foreach (Subframe subframe in subframes_)
			{
				subframe.Write(stream);
			}

			stream.Writer.WriteBits(0, stream.Writer.BitsToByteBoundary);

			footer_.Write(stream);
		}

		/* the count functions "fold" the residual partitions into a single
		 * big partition. difference in no. of bytes can arise because of the same
		 * */
		public void Write(FlacStream stream, int startSample, int count)
		{
			header_.Write(stream);

			foreach (Subframe subframe in subframes_)
			{
				if (!(subframe  is SubframeConstant))
				{
					SubframeVerbatim v = subframe as SubframeVerbatim;
					if (v != null)
					{
						ShiftSamples(startSample, count, v);
					}
					SubframeFixed f = subframe as SubframeFixed;
					if (f != null)
					{
						f.DecodeSamples();
						ShiftSamples(startSample, count, f);
						f.EncodeSamples(count);
					}
					SubframeLpc lpc = subframe as SubframeLpc;
					if (lpc != null)
					{
						lpc.DecodeSamples();
#if DEBUG
						uint[] debug_DecodedSamples = new uint[lpc.Samples.Length];
						lpc.Samples.CopyTo(debug_DecodedSamples, 0);
#endif
						ShiftSamples(startSample, count, lpc);
						lpc.EncodeSamples(count);
#if DEBUG
						lpc.DecodeSamples();
						
						int i0 = startSample;
						int i1 = 0;
						int max = i0 + count;
						while (i0 < max)
						{
							Debug.Assert(debug_DecodedSamples[i0] == lpc.Samples[i1]);
							i0++;
							i1++;
						}
						lpc.EncodeSamples(count);
#endif
					}
				}
				subframe.Write(stream, count);
			}

			stream.Writer.WriteBits(0, stream.Writer.BitsToByteBoundary);

			footer_.Write(stream);
		}

		private static void ShiftSamples(int startSample, int count, Subframe sf)
		{
			// shift samples
			int i = 0;
			while (i < count)
			{
				sf.Samples[i] = sf.Samples[i + startSample];
				i++;
			}
		}

        private int GetBps()
        {
            int bps = header_.BitsPerSample;

            // first figure the correct bits-per-sample of the subframe
            switch (header_.ChannelAssignment)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                {
                    // independent channels
                    break;
                }
                case 8:
                case 10:
                {
                    // channel 1 is difference channel. Has additional bit.
                    if (subframes_.Count == 1) bps++;
                    break;
                }
                case 9:
                {
                    // channel 0 is difference channel. Has additional bit.
                    if (subframes_.Count == 0) bps++;
                    break;
                }
                default:
                {
                    throw new FlacFormatReservedException();
                }
            }
            return bps;
        }
    }
}
