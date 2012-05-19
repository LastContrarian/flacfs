using System;
using System.Collections.Generic;
using System.Text;

using flac;
using flac.Format;
namespace flacTest
{
	class CopyFile : ProcessFile
	{
		FlacStream outStream_;

		// dump whole file
		public CopyFile(string file)
			:base(file)
		{
			Console.Write("Copying...");
			outStream_ = new FlacStream(file + ".copy.flac", FlacStream.StreamMode.CreateNew, FlacStream.StreamAccessMode.Both);
			outStream_.Encode();

			stream_.Decode();

			Done();
		}

		protected override void ProcessMetaInformation(MetadataBlock metadataBlock)
		{
			MetadataBlockStreamInfo readStreamInfo = metadataBlock as MetadataBlockStreamInfo;
			if (readStreamInfo != null)
			{
				readStreamInfo.Write(outStream_);
			}
		}

		protected override void ProcessFrame(FrameCallbackArgs args)
		{
			Frame frame = args.Frame;
			frame.ReadData(stream_);
			frame.ReadFooter(stream_);
			frame.Write(outStream_);

			args.ContinueDecoding = true;
			args.HaveReadData = true;
		}
	}
}
