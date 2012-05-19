using System;
using System.Collections.Generic;
using System.Text;

using flac.Io;
namespace flac.Format
{
	public class RicePartition
	{
		const int ENCODING_PARAMETER_BITS_COUNT = 4;
		const int BPS_UNENCODED_DATA_BITS_COUNT = 5;

		sbyte encodingParameter_;
		sbyte bpsForUnencodedData_;

		public sbyte EncodingParameter { set { encodingParameter_ = value; } }
		public sbyte BpsForUnencodedData { set { bpsForUnencodedData_ = value; } }

		//
		public void Read(FlacStream stream, int sampleCount, ref int currentSubframeSample, ref uint[] samples)
		{
			encodingParameter_ = stream.Reader.ReadBitsAsSByte(ENCODING_PARAMETER_BITS_COUNT);

			int i = currentSubframeSample;
			int max = i + sampleCount;
			if (ParamIsEscapeCode())
			{
				bpsForUnencodedData_ = stream.Reader.ReadBitsAsSByte(BPS_UNENCODED_DATA_BITS_COUNT);
				while (i < max)
				{
				 	samples[i] = stream.Reader.ReadBitsAsUInt(bpsForUnencodedData_);
					i++;
				}
			}
			else
			{
				while (i < max)
				{
					samples[i] = (uint)RiceDecoder.GetResidual(stream, encodingParameter_);
					i++;
				}
			}
			currentSubframeSample = i;
		}

		private bool ParamIsEscapeCode()
		{
			bool paramIsEscapeCode = encodingParameter_ > 14;
			return paramIsEscapeCode;
		}

		public void Write(FlacStream stream, int sampleCount, ref int currentSubframeSample, ref uint[] samples)
		{
			stream.Writer.WriteBits(encodingParameter_, ENCODING_PARAMETER_BITS_COUNT);

			int i = currentSubframeSample;
			int max = i + sampleCount;
			if (ParamIsEscapeCode())
			{
				stream.Writer.WriteBits(bpsForUnencodedData_, BPS_UNENCODED_DATA_BITS_COUNT);
				while (i < max)
				{
					stream.Writer.WriteBits(samples[i], bpsForUnencodedData_);
					i++;
				}
			}
			else
			{
				while (i < max)
				{
					RiceEncoder.PutResidual(stream, (int)samples[i],encodingParameter_);
					i++;
				}
			}
			currentSubframeSample = i;
		}
	}
}
