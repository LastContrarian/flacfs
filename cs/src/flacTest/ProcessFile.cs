using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using flac;
using flac.Format;
namespace flacTest
{
	public abstract class ProcessFile
	{
		protected FlacStream stream_;
		protected Stopwatch watch_ = new Stopwatch();

		protected abstract void ProcessMetaInformation(MetadataBlock metadataBlock);
		protected abstract void ProcessFrame(FrameCallbackArgs args);

		long length_;

		protected ProcessFile(string file)
			:this(file, false)
		{
		}

		protected ProcessFile(string file, bool noCallback)
		{
			watch_.Start();
			stream_ = new FlacStream(file, FlacStream.StreamMode.OpenExisting, FlacStream.StreamAccessMode.Read);

			length_ = new System.IO.FileInfo(file).Length;

			if (noCallback)
			{
			}
			else
			{
				stream_.MetadataRead += new MetadataCallback(stream__MetadataRead);
				stream_.BeforeFrameDataRead += new FrameCallback(stream__BeforeFrameDataRead);
			}
		}

		void stream__BeforeFrameDataRead(FrameCallbackArgs args)
		{
			long rem = args.Stream.Reader.BytesRemaining;

			long percent = (length_ - rem) * 100 / length_;
			Console.Title = percent.ToString() + "% complete";
			ProcessFrame(args);
		}

		void stream__MetadataRead(MetadataBlock metadataBlock)
		{
			ProcessMetaInformation(metadataBlock);
		}

		protected virtual void Done()
		{
			TimeSpan t = watch_.Elapsed;
			watch_.Stop();
			Console.WriteLine("done in: " + t.Minutes + "m " + t.Seconds + "s " + t.Milliseconds + "ms");
		}
	}
}
