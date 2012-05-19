using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Utils
{
	class Warmup
	{
		public static uint[] ReadSamples(FlacStream stream, int order, int bitsPerSample)
		{
			uint[] warmupSamples = null;

			// read unencoded warmup bits
			warmupSamples = new uint[order];

			int i = 0;
			while (i < warmupSamples.Length)
			{
				uint val = stream.Reader.ReadBitsAsUInt(bitsPerSample);
				warmupSamples[i] = val;
				i++;
			}
			return warmupSamples;
		}

		public static void WriteSamples(FlacStream stream, uint[] warmupSamples, int bitsPerSample, int count)
		{
			Debug.Assert(warmupSamples != null);

			int i = 0;
			while (i < count)
			{
				stream.Writer.WriteBits(warmupSamples[i], bitsPerSample);
				i++;
			}
		}
	}
}
