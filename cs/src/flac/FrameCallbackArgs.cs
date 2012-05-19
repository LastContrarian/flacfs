using System;
using System.Collections.Generic;
using System.Text;

using flac;
using flac.Format;
namespace flac
{
	public class FrameCallbackArgs
	{
		Frame frame_;
		FlacStream stream_;

		public Frame Frame { get { return frame_; } }
		public FlacStream Stream { get { return stream_; } }
		public bool HaveReadData = false;
		public bool ContinueDecoding = true;
		public long Offset;

		public FrameCallbackArgs(Frame frame, FlacStream stream)
		{
			frame_ = frame;
			stream_ = stream;
		}
	}
}
