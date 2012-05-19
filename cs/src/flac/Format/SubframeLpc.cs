using System;
using System.Collections.Generic;
using System.Text;

using flac.Utils;
namespace flac.Format
{
	public class SubframeLpc : Subframe
    {
		const int QLPC_PRECISION_BITS_COUNT = 4;
		const int QLPC_PRECISION_SHIFT_COUNT = 5;

        sbyte quantitizedLpcPrecisionInBitsMinusOne_;
        sbyte quantitizedLpcShiftInBits_;    // signed two's complement
		uint[] predictorCoefficients_; // signed two's complement
        Residual residual_;

		int bitsPerSample_;
		int lpcOrder_;

		static int GetSignedInt(uint value, int bits)
		{
			Debug.Assert(bits < Io.Bit.BITS_IN_DWORD);

			int shift = Io.Bit.BITS_IN_DWORD - bits;
			int val = (int)(value << shift);
			val >>= shift;
			return val;
		}

		public void Read(FlacStream stream, FrameHeader frameHeader, int order, int bitsPerSample)
		{
			bitsPerSample_ = bitsPerSample;
			lpcOrder_ = order;

			uint [] warmupSamples = Warmup.ReadSamples(stream, order, bitsPerSample_);

			quantitizedLpcPrecisionInBitsMinusOne_ = stream.Reader.ReadBitsAsSByte(QLPC_PRECISION_BITS_COUNT);
			quantitizedLpcShiftInBits_ = stream.Reader.ReadBitsAsSByte(QLPC_PRECISION_SHIFT_COUNT);

			predictorCoefficients_ = new uint[order];
			int i  = 0;
			while (i < predictorCoefficients_.Length)
			{
				predictorCoefficients_[i] = stream.Reader.ReadBits(quantitizedLpcPrecisionInBitsMinusOne_ + 1);
				i++;
			}

			samples_ = new uint[frameHeader.BlockSize];

			i = 0;
			foreach (uint w in warmupSamples)
			{
				samples_[i] = w;
				i++;
			}

			residual_ = new Residual();
			residual_.Read(stream, frameHeader.BlockSize, order, ref samples_);
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
			Warmup.WriteSamples(stream, samples_, bitsPerSample_, lpcOrder_);
			stream.Writer.WriteBits(quantitizedLpcPrecisionInBitsMinusOne_, QLPC_PRECISION_BITS_COUNT);
			stream.Writer.WriteBits(quantitizedLpcShiftInBits_, QLPC_PRECISION_SHIFT_COUNT);

			foreach (uint pc in predictorCoefficients_)
			{
				stream.Writer.WriteBits(pc, quantitizedLpcPrecisionInBitsMinusOne_ + 1);
			}
		}

		public void DecodeSamples()
		{
			/* presently samples are residuals. we have to decode them if we
			* want to write a partial frame. convert them into "real" samples.
			* */
			// don't use same array for both operations
			uint[] residuals = samples_;
			uint[] samples = new uint[samples_.Length];
			
			int i = 0;
			int max = residuals.Length;
			// copy warmup bits
			while (i < lpcOrder_)
			{
				samples[i] = residuals[i]; 
				i++;
			}
			while (i < max)
			{
				samples[i] = GetDecodedSample(ref residuals, ref samples, lpcOrder_, ref predictorCoefficients_, quantitizedLpcPrecisionInBitsMinusOne_ + 1, quantitizedLpcShiftInBits_, i);
				i++;
			}
			samples_ = samples;
		}

		private static uint GetDecodedSample(ref uint[] residuals, ref uint[] samples, int order, ref uint [] coeffs, int precision, int shift, int i)
		{
			int j = 0;
			int sum = 0;
			while (j < order)
			{
				//??? predictor coefficient is a signed integer. whatever its size in bits,
				// it should be treated like one.
				int coeff = GetSignedInt(coeffs[j], precision);
				sum += coeff * (int)samples[i - j - 1];
				j++;
			}
			int sshift = GetSignedInt((uint)shift, QLPC_PRECISION_SHIFT_COUNT);
			if (sshift < 0)
			{
				sum <<= shift;
			}
			else
			{
				sum >>= shift;
			}
			Debug.Assert(sum <= int.MaxValue && sum >= int.MinValue);
			return (uint)(sum + (int)residuals[i]);
		}

		public void EncodeSamples(int count)
		{
			uint[] samples = samples_;
			uint[] residuals = new uint[samples.Length];

			int i = 0;
			int max = count;
			// copy warmup bits
			while (i < lpcOrder_)
			{
				residuals[i] = samples[i];
				i++;
			}
			while (i < max)
			{
				residuals[i] = GetEncodedResidual(ref samples, lpcOrder_, ref predictorCoefficients_, quantitizedLpcPrecisionInBitsMinusOne_ + 1, quantitizedLpcShiftInBits_, i);
				i++;
			}
			samples_ = residuals;
		}

		private static uint GetEncodedResidual(ref uint[] samples, int order, ref uint [] coeffs, int precision, int shift, int i)
		{
			int j = 0;
			int sum = 0;
			while (j < order)
			{
				int coeff = GetSignedInt(coeffs[j], precision);
				sum += coeff * (int)samples[i - j - 1];
				j++;
			}
			int sshift = GetSignedInt((uint)shift, QLPC_PRECISION_SHIFT_COUNT);
			if (sshift < 0)
			{
				sum <<= shift;
			}
			else
			{
				sum >>= shift;
			}
			Debug.Assert(sum <= int.MaxValue && sum >= int.MinValue);
			return (uint)((int)samples[i] - sum);
		}
    }
}
