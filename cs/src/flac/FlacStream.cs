using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using flac.Format;
using flac.Io;
namespace flac
{
	public class FlacStream
	{
		// enums
		public enum StreamAccessMode
		{
			Read = FileAccess.Read,
			Write = FileAccess.Write,
			Both = FileAccess.ReadWrite
		}

		public enum StreamMode
		{
			OpenExisting,
			CreateNew
		}

		// events
		public event MetadataCallback MetadataRead;
		public event FrameCallback BeforeFrameDataRead;

		// data
		Signature signature_;
		MetadataBlockStreamInfo streamInfo_;

		FileStream fileStream_;
		FlacStreamReader reader_;
		FlacStreamWriter writer_;

		public FlacStreamReader Reader { get { return reader_; } }
		public FlacStreamWriter Writer { get { return writer_; } }
		public FileStream FileStream { get { return fileStream_; } }

		public MetadataBlockStreamInfo StreamInfo { get { return streamInfo_; } }

		public void Decode()
		{
			signature_ = new Signature();
			signature_.Read(this);

			streamInfo_ = MetadataBlock.New(this) as MetadataBlockStreamInfo;
			Validation.IsValid(streamInfo_ != null);

			MetadataBlock block = streamInfo_;
			if (MetadataRead != null)
			{
				MetadataRead(streamInfo_);
			}

			while (!block.Header.IsLastMetadataBlock)
			{
				block = MetadataBlock.New(this);
				if (MetadataRead != null)
				{
					MetadataRead(block);
				}
			}

			while (!reader_.Done)
			{
				Frame frame = new Frame();
				long offset = reader_.Debug_BytesRead;
				frame.ReadHeader(this);
				bool haveRead = false;
				if (BeforeFrameDataRead != null)
				{
					FrameCallbackArgs a = new FrameCallbackArgs(frame, this);
					a.Offset = offset;
					BeforeFrameDataRead(a);
					if (!a.ContinueDecoding)
					{
						break;
					}
					haveRead = a.HaveReadData;	
				}
				if (!haveRead)
				{
					frame.ReadData(this);
					frame.ReadFooter(this);
				}
			}
		}

		public void Close()
		{
			fileStream_.Close();
		}

		public void Encode()
		{
			signature_ = new Signature();
			signature_.Write(this);
		}

		public FlacStream(string file, StreamMode mode, StreamAccessMode accessMode)
		{
			FileMode fileMode;
			FileAccess fileAccessMode;

			switch (mode)
			{
				case StreamMode.CreateNew:
				{
					fileMode = FileMode.Create;
					break;
				}
				case StreamMode.OpenExisting:
				{
					fileMode = FileMode.Open;
					break;
				}
				default:
				{
					throw new FlacDebugException();
				}
			}
			
			switch (accessMode)
			{
				case StreamAccessMode.Read:
				case StreamAccessMode.Write:
				case StreamAccessMode.Both:
				{
					fileAccessMode = (FileAccess)accessMode;
					break;
				}
				default:
				{
					throw new FlacDebugException();
				}
			}
			fileStream_ = new FileStream(file, fileMode, fileAccessMode, FileShare.Read);

			if ((accessMode & StreamAccessMode.Read) == StreamAccessMode.Read)
			{
				reader_ = new FlacStreamReader(fileStream_);
			}
			if ((accessMode & StreamAccessMode.Write) == StreamAccessMode.Write)
			{
				writer_ = new FlacStreamWriter(fileStream_);
			}
		}
	}
}
