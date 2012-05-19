using System;
using System.Collections.Generic;
using System.Text;

using flac.Utils;
namespace flac.Format
{
    public class SubframeFixed : Subframe
    {        
        Residual residual_;

		int bitsPerSample_;
		int predictorOrder_;

		public int Order
		{
			get { return predictorOrder_; }
		}

		public void Read(FlacStream stream, FrameHeader frameHeader, int predictorOrder, int bitsPerSample)
		{
			bitsPerSample_ = bitsPerSample;
			predictorOrder_ = predictorOrder;

			uint [] warmupSamples = Warmup.ReadSamples(stream, predictorOrder, bitsPerSample_);

			int sampleCount = frameHeader.BlockSize;
			samples_ = new uint[sampleCount];
			
			int i = 0;
			foreach (uint w in warmupSamples)
			{
				samples_[i] = w;
				i++;
			}

			residual_ = new Residual();
			residual_.Read(stream, frameHeader.BlockSize, predictorOrder, ref samples_);
		}

		public override void Write(FlacStream stream)
		{
			Write_(stream);
			residual_.Write(stream, ref samples_);
		}

		public override void Write(FlacStream stream, int count)
		{
			Write_(stream);
			residual_.Write(stream, count, ref samples_);
		}

		private void Write_(FlacStream stream)
		{
			base.Write(stream);
			Warmup.WriteSamples(stream, samples_, bitsPerSample_, predictorOrder_);
		}

		public void DecodeSamples()
		{
			/* presently samples are residuals. we have to decode them if we
			* want to write a partial frame. convert them into "real" samples.
			* */
			int i = Order;
			while (i < samples_.Length)
			{
				uint residual = samples_[i];
				switch (Order)
				{
					case 0:
					{
						samples_[i] = residual;
						break;
					}
					case 1:
					{
						samples_[i] = samples_[i - 1] + residual;
						break;
					}
					case 2:
					{
						samples_[i] = (samples_[i - 1] * 2) - samples_[i - 2] + residual;
						break;
					}
					case 3:
					{
						samples_[i] = (samples_[i - 1] * 3) - (samples_[i - 2] * 3) + samples_[i - 3] + residual;
						break;
					}
					case 4:
					{
						samples_[i] = (samples_[i - 1] * 4) - (samples_[i - 2] * 6) + (4 * samples_[i - 3]) - samples_[i - 4] + residual;
						break;
					}
				}
				i++;
			}
		}

		public void EncodeSamples(int count)
		{
			int i = Order;
			int max = count;
			while (i < max)
			{
				uint residual;
				switch (Order)
				{
					case 0:
					{
						residual = samples_[i];
						break;
					}
					case 1:
					{
						residual = samples_[i] - samples_[i - 1];
						break;
					}
					case 2:
					{
						residual = samples_[i] - (2 * samples_[i - 1] - samples_[i - 2]);
						break;
					}
					case 3:
					{
						residual = samples_[i] - (3 * samples_[i - 1] - 3 * samples_[i - 2] + samples_[i - 3]);
						break;
					}
					case 4:
					{
						residual = samples_[i] - (4 * samples_[i - 1] - 6 * samples_[i - 2] + 4 * samples_[i - 3] - samples_[i - 4]);
						break;
					}
					default:
					{
						throw new FlacDebugException();
					}
				}
				samples_[i] = residual;
				i++;
			}
		}
    }
}
