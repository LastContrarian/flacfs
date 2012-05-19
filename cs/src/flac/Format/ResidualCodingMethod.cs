using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public abstract class ResidualCodingMethod
    {
		public abstract void Read(FlacStream stream, int frameBlockSize, int predictorOrder, ref uint[] samples);
		public abstract void Write(FlacStream stream, ref uint[] samples);
		public abstract void Write(FlacStream stream, int count, ref uint[] samples);
	}
}
