using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using flac;
using flac.Format;
namespace flacTest
{
	public class ExtractTrack : ProcessFile
	{
		long startSample_;
		long endSample_;
		long trackFrameCount_;

		long sampleCount_;

		FlacStream outStream_;
		MetadataBlockStreamInfo streamInfo_;

		bool done_;
		bool writeFrames_;

		public ExtractTrack(string file, long startSample, long endSample)
			: base(file)
		{
			startSample_ = startSample;
			endSample_ = endSample;

			string destFile = file + "." + startSample + ".flac";

			Console.Write("Extracting...");
			outStream_ = new FlacStream(destFile, FlacStream.StreamMode.CreateNew, FlacStream.StreamAccessMode.Both);
			outStream_.Encode();

			stream_.Decode();

			stream_.Close();
			outStream_.Close();

			outStream_ = new FlacStream(destFile, FlacStream.StreamMode.OpenExisting, FlacStream.StreamAccessMode.Both);
			
			outStream_.Encode();

			streamInfo_.Header.IsLastMetadataBlock = true;
			streamInfo_.TotalSamples = sampleCount_;
			streamInfo_.Checksum[0] = 0;
			streamInfo_.Checksum[1] = 0;
			streamInfo_.Checksum[2] = 0;
			streamInfo_.Checksum[3] = 0;
			streamInfo_.Write(outStream_);

			outStream_.Close();

			Done();
		}

		protected override void ProcessMetaInformation(MetadataBlock metadataBlock)
		{
			MetadataBlockStreamInfo readStreamInfo = metadataBlock as MetadataBlockStreamInfo;
			if (readStreamInfo != null)
			{
				streamInfo_ = readStreamInfo;
				readStreamInfo.Write(outStream_);
			}
		}

		protected override void ProcessFrame(FrameCallbackArgs args)
		{
			if (done_)
			{
				args.ContinueDecoding = false;
				return;
			}

			Frame frame = args.Frame;
			FrameHeader header = args.Frame.Header;
			bool haveRead = false;

			long start = header.StartingSampleNumber;
			long end = start + header.BlockSize;

			if (!writeFrames_)
			{
				if (start <= startSample_ && startSample_ <= end)
				{
					if (start <= endSample_ && endSample_ <= end)
					{
						// tracks ends in the same frame
						writeFrames_ = false;
						end = endSample_;
					}
					else
					{
						writeFrames_ = true;
					}
					start = startSample_;

					// write (probably) partial first frame of track
					WriteFrame(frame, header, start, end);
					haveRead = true;
				}
				else
				{
					// nothing
				}
			}
			else
			{
				if (start <= endSample_ && endSample_ <= end)
				{
					// write (probably) partial last frame of track
					WriteFrame(frame, header, start, endSample_);
					haveRead = true;
					done_ = true;
					writeFrames_ = false;
				}
				else
				{
					// write full frame
					WriteFrame(frame, header, start, end);
					haveRead = true;
				}
			}

			args.HaveReadData = haveRead;
			args.ContinueDecoding = !done_;
		}

		private void WriteFrame(Frame frame, FrameHeader header, long start, long end)
		{
			frame.ReadData(stream_);
			frame.ReadFooter(stream_);

			long startSample = frame.Header.StartingSampleNumber;
			int count = (int)(end - start);

			header.StartingSampleNumber = sampleCount_;
			header.BlockingStrategy = FrameHeader.BLOCKING_STRATEGY_VARIABLE;

			if (count == header.BlockSize)
			{
				frame.Write(outStream_);
			}
			else
			{
				header.BlockSize = count;
				frame.Write(outStream_, (int)(start - startSample), count);
			}

			trackFrameCount_++;
			sampleCount_ += header.BlockSize;
		}
	}
}