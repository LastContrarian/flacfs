using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public abstract class MetadataBlock
    {
        MetadataBlockHeader header_;
        
        public MetadataBlockHeader Header
        {
            get { return header_; }
        }

        protected MetadataBlock(MetadataBlockHeader header)
        {
            header_ = header;
        }

		public abstract void Read(FlacStream stream);
		public abstract void Write(FlacStream stream);

		public static MetadataBlock New(FlacStream stream)
		{
			MetadataBlockHeader header = new MetadataBlockHeader();
			header.Read(stream);

			MetadataBlock block = GetBlock(header);
			block.Read(stream);
			return block;
		}

		private static MetadataBlock GetBlock(MetadataBlockHeader header)
		{
			MetadataBlock block;
			switch (header.BlockType)
			{
				case 0:
				{
					block = new MetadataBlockStreamInfo(header);
					break;
				}
				case 1:
				{
					block = new MetadataBlockPadding(header);
					break;
				}
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
				{
					// we won't read any block except streaminfo
					// skip over them all.
					// think of them as padding.
					block = new MetadataBlockPadding(header);
					break;
				}
				case 127:
				{
					throw new FlacFormatInvalidException();
				}
				default:
				{
					throw new FlacFormatReservedException();
				}
			}
			return block;
		}
    }
}
