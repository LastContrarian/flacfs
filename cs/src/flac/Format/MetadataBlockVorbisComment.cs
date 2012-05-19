using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
	public class MetadataBlockVorbisComment : MetadataBlock
	{
		byte[] blockData_;

		public MetadataBlockVorbisComment(MetadataBlockHeader header, byte [] blockData)
            : base(header)
        {
			blockData_ = blockData;
        }

		public override void Read(FlacStream stream)
		{
			throw new FlacNotImplementedException();
		}

		public override void Write(FlacStream stream)
		{
			Header.Write(stream);

			foreach (byte b in blockData_)
			{
				stream.Writer.WriteByte(b);
			}
		}
	}
}
