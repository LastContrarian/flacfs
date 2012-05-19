using System;
using System.Collections.Generic;
using System.Text;

using flac.Io;
namespace flac.Format
{
    public class MetadataBlockPadding : MetadataBlock
    {
        public MetadataBlockPadding(MetadataBlockHeader header)
            : base(header)
        {
        }

		public override void Read(FlacStream stream)
		{
			stream.Reader.SkipBits(Bit.MultiplyBy8I(Header.BlockLengthInBytes));
		}

		public override void Write(FlacStream stream)
		{
			Header.Write(stream);

			stream.Writer.WriteBits(0, Bit.MultiplyBy8I(Header.BlockLengthInBytes));
		}
    }
}
