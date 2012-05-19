using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class MetadataBlockHeader
    {
		public const int METADATA_BLOCK_VORBIS_COMMENT = 4;
        const sbyte LAST_METADATA_BLOC_FLAG = 1;
		const int BLOCK_TYPE_BITS_COUNT = 7;
		const int DATA_LENGTH_BITS_COUNT = 24;

        sbyte isLastMetadataBlock_;
        sbyte blockType_;
        int dataLength_;

        public bool IsLastMetadataBlock
        {
            get
			{ 
				return isLastMetadataBlock_ == LAST_METADATA_BLOC_FLAG;
			}
			set
			{ 
				isLastMetadataBlock_ = (value == true ? LAST_METADATA_BLOC_FLAG : (sbyte)0);
			}
        }

		public int BlockType
		{
			get { return blockType_; }
			set { blockType_ = Convert.ToSByte(value); }
        }

        public int BlockLengthInBytes
        {
            get { return dataLength_; }
			set { dataLength_ = value; }
        }

		public void Read(FlacStream stream)
		{
			isLastMetadataBlock_ = stream.Reader.ReadBit();
			blockType_ = stream.Reader.ReadBitsAsSByte(BLOCK_TYPE_BITS_COUNT);
			dataLength_ = stream.Reader.ReadBitsAsInt(DATA_LENGTH_BITS_COUNT);
		}

		public void Write(FlacStream stream)
		{
			stream.Writer.WriteBit(isLastMetadataBlock_);
			stream.Writer.WriteBits(blockType_, BLOCK_TYPE_BITS_COUNT);
			stream.Writer.WriteBits(dataLength_, DATA_LENGTH_BITS_COUNT);
		}
    }
}
