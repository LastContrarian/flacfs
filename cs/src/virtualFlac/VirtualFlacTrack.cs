using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using flac;
using flac.Format;
namespace VirtualFlac
{
	public class VirtualFlacTrack
	{
		//const long TRACK_HEADER_SIZE = 4 + 4 + 34;

		VirtualTrack track_;
		byte[] header_;

		//
		FlacStream inStream_;
		FlacStream outStream_;

		public VirtualTrack Track
		{
			get { return track_; }
		}

		public VirtualFlacTrack(string flacImage, VirtualTrack track)
		{
			track_ = track;

			inStream_ = new FlacStream(flacImage, FlacStream.StreamMode.OpenExisting, FlacStream.StreamAccessMode.Read);
			inStream_.BeforeFrameDataRead += new FrameCallback(inStream__BeforeFrameDataRead);
			inStream_.Decode();
			outStream_ = new FlacStream(Path.GetTempFileName(), FlacStream.StreamMode.CreateNew, FlacStream.StreamAccessMode.Both);

			ReadHeader();
		}

		public void Close()
		{
			inStream_.Close();
			outStream_.Close();
		}

		void inStream__BeforeFrameDataRead(FrameCallbackArgs args)
		{
			args.ContinueDecoding = false;
		}

		private void ReadHeader()
		{
			outStream_.Encode();

			inStream_.Reader.Seek(4);
			MetadataBlockHeader h = new MetadataBlockHeader();
			h.Read(inStream_);
			MetadataBlockStreamInfo si = new MetadataBlockStreamInfo(h);
			si.Read(inStream_);

			h.IsLastMetadataBlock = false;
			si.MinBlockSize = track_.MinBlockSize;
			si.MaxBlockSize = track_.MaxBlockSize;
			si.MinFrameSize = track_.MinFrameSize;
			si.MaxFrameSize = track_.MaxFrameSize;
			si.TotalSamples = track_.TotalSamples;
			si.Checksum[0] = 0;
			si.Checksum[1] = 0;
			si.Checksum[2] = 0;
			si.Checksum[3] = 0;
			si.Write(outStream_);

			long pos = outStream_.FileStream.Position;

			h = new MetadataBlockHeader();
			h.IsLastMetadataBlock = true;
			h.BlockLengthInBytes = track_.Metadata.block.Length;
			h.BlockType = MetadataBlockHeader.METADATA_BLOCK_VORBIS_COMMENT;
			MetadataBlockVorbisComment vc = new MetadataBlockVorbisComment(h, track_.Metadata.block);
			vc.Write(outStream_);

			Debug.Assert(outStream_.FileStream.Position == pos + h.BlockLengthInBytes + 4);

			outStream_.FileStream.Flush();
			header_ = ReadAllBytes();
		}

		byte[] ReadAllBytes()
		{
			byte [] data = new byte[outStream_.FileStream.Length];
			outStream_.FileStream.Seek(0, SeekOrigin.Begin);
			outStream_.FileStream.Read(data, 0, (int)outStream_.FileStream.Length);
			return data;
		}

		// read n bytes of virtual track starting at x offset into provided buffer
		public int Read(byte[] bytes, long offset, int count)
		{
			count = Math.Min((int)(track_.Size - offset), count);

			int bytesToRead = count;
			long currentOffset = offset;
			long endOffset = Math.Max(currentOffset, offset + bytesToRead - 1);

			// if header, copy it from memory
			long max = Math.Min(endOffset + 1, header_.Length);
			while (currentOffset < max)
			{
				bytes[currentOffset - offset] = header_[currentOffset];
				currentOffset++;
				bytesToRead--;
			}

			if (bytesToRead <= 0)
			{
				goto exit;
			}

			List<TrackFrame> pairs;

			int currentIndex = GetOffsetPairs(bytesToRead, currentOffset, out pairs);
			foreach (TrackFrame op in pairs)
			{
				inStream_.Reader.Seek(op.imageOffset);
				Frame f = new Frame();
				f.ReadHeader(inStream_);
				f.ReadData(inStream_);
				f.ReadFooter(inStream_);

				f.Header.BlockingStrategy = FrameHeader.BLOCKING_STRATEGY_VARIABLE;
				f.Header.StartingSampleNumber = op.startSample;
				f.Header.BlockSize = op.virtualBlockSize;

				outStream_.Writer.Truncate(0);

				bool countFunction = false;
				if ((currentIndex == 0 && op.imageStartSample != track_.StartSample) || (currentIndex == track_.Frames.Count -1 &&  op.imageEndSample != track_.EndSample))
				{
					// partial frame
					long start = Math.Max(track_.StartSample, op.imageStartSample);
					long end = Math.Min(track_.EndSample, op.imageEndSample);
					f.Write(outStream_, (int)(start - op.imageStartSample), (int)(end - start));
					countFunction = true;
				}
				else
				{
					f.Write(outStream_);	
				}

				byte[] data = ReadAllBytes();

				max = Math.Min(endOffset + 1, op.virtualOffset + op.virtualSize);
				if (!countFunction)
				{
					Debug.Assert(data.Length == op.virtualSize);
				}

				int i = Convert.ToInt32(currentOffset - op.virtualOffset);
				while (currentOffset < max)
				{
					bytes[currentOffset - offset] = data[i];
					i++;
					currentOffset++;
					bytesToRead--;
				}

				if (bytesToRead <= 0)
				{
					break;
				}
				currentIndex++;
			}

			exit:
			return count;
		}

		private int GetOffsetPairs(int bytesToRead, long currentOffset, out List<TrackFrame> pairs)
		{
			// locate pair in which offset is located
			int index = 0;
			while (index < track_.Frames.Count)
			{
				if (track_.Frames.Keys[index] > currentOffset)
				{
					break;
				}
				index++;
			}

			index--;
			Debug.Assert(index >= 0 && index < track_.Frames.Count);
			pairs = new List<TrackFrame>();
			int firstFrame = index;
			long co = currentOffset;
			while (bytesToRead > 0)
			{
				TrackFrame op = track_.Frames.Values[index];
				pairs.Add(op);
				bytesToRead -= (int)(op.virtualSize - (co - op.virtualOffset));
				index++;
				co = op.virtualOffset + op.virtualSize;
			}
			return firstFrame;
		}
	}
}
