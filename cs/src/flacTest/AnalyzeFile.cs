using System;
using System.Collections.Generic;
using System.Text;

using flac;
using flac.Format;
namespace flacTest
{
	class AnalyzeFile : ProcessFile
	{
		long currentFrame_ = 0;

		public AnalyzeFile(string file)
			: base(file, true)
		{
			Console.Write("Analyzing...");
			stream_.Decode();
			Done();
		}

		protected override void ProcessMetaInformation(MetadataBlock metadataBlock)
		{
		}

		protected override void ProcessFrame(FrameCallbackArgs args)
		{
			FrameHeader header = args.Frame.Header;
			string s = "Frame # {0}: S-{1} E-{2}";
			s = string.Format(s, currentFrame_, header.StartingSampleNumber, header.StartingSampleNumber + header.BlockSize);
			Console.WriteLine(s);
			currentFrame_++;

			// we are not reading file data
			args.HaveReadData = false;
		}
	}
}
