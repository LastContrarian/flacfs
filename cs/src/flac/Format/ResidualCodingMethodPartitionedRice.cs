using System;
using System.Collections.Generic;
using System.Text;

namespace flac.Format
{
    public class ResidualCodingMethodPartitionedRice : ResidualCodingMethod
    {
		const int PARTITION_ORDER_BITS_COUNT = 4;

        sbyte partitionOrder_;
        List<RicePartition> partitions_ = new List<RicePartition>();

		//
		int predictorOrder_;
		int blockSize_;

		public override void Read(FlacStream stream, int frameBlockSize, int predictorOrder, ref uint[] samples)
		{
			partitionOrder_ = stream.Reader.ReadBitsAsSByte(PARTITION_ORDER_BITS_COUNT);

			predictorOrder_ = predictorOrder;
			blockSize_ = frameBlockSize;

			int partitionCount = 1 << partitionOrder_; // 2 ^ order
			Validation.IsValid(partitionCount >= 1);

			int currentSubframeSample = predictorOrder;

			int sampleCount0 = GetSampleCount(frameBlockSize, predictorOrder, partitionOrder_, true);
			int sampleCount1 = GetSampleCount(frameBlockSize, predictorOrder, partitionOrder_, false);

			while (partitions_.Count < partitionCount)
			{
				int count = partitions_.Count == 0 ? sampleCount0 : sampleCount1;
				RicePartition partition = new RicePartition();
				partition.Read(stream, count, ref currentSubframeSample, ref samples);
				partitions_.Add(partition);
			}
		}

		public override void Write(FlacStream stream, ref uint[] samples)
		{
			stream.Writer.WriteBits(partitionOrder_, PARTITION_ORDER_BITS_COUNT);

			int i = 0;
			int currentSubframeSample = predictorOrder_;
			foreach (RicePartition p in partitions_)
			{
				p.Write(stream, GetSampleCount(blockSize_, predictorOrder_, partitionOrder_, i == 0), ref currentSubframeSample, ref samples);
				i++;
			}
		}

		public override void Write(FlacStream stream, int blockSize, ref uint[] samples)
		{
			// only one partition
			partitionOrder_ = 0;
			blockSize_ = blockSize;

			stream.Writer.WriteBits(partitionOrder_, PARTITION_ORDER_BITS_COUNT);

			int currentSubframeSample = predictorOrder_;
			RicePartition p = new RicePartition();
			p.EncodingParameter = 0;
			p.Write(stream, GetSampleCount(blockSize_, predictorOrder_, partitionOrder_, true), ref currentSubframeSample, ref samples);
		}

		private static int GetSampleCount(int frameBlockSize, int predictorOrder, int partitionOrder, bool isFirstPartition)
		{
			int sampleCount;

			if (partitionOrder == 0)
			{
				/* predictorOrder is substracted from first partition because
				 * those many samples have been encoded as warmup samples.
				 * */
				sampleCount = frameBlockSize - predictorOrder;
			}
			else
			{
				if (!isFirstPartition)
				{
					/* frameBlockSize / (2 ^ partitionOrder) */
					sampleCount = frameBlockSize >> partitionOrder;
				}
				else
				{
					/* (frameBlockSize / (2 ^ partitionOrder)) - predictorOrder
					 * predictorOrder is substracted from first partition because those many samples
					 * have been encoded as warmup samples.
					 * */
					sampleCount = (frameBlockSize >> partitionOrder) - predictorOrder;
				}
			}
			return sampleCount;
		}
    }
}
