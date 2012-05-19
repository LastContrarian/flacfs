using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

using flac;
using flac.Format;
namespace VirtualFlac
{
	public class VirtualFlacCreator
	{
		FlacStream inStream_;
		FlacStream outStream_;

		ITrackInfo[] tracksInfo_;
		int blockingStrategy_ = -1;
		List<VirtualTrack> virtualTracks_ = new List<VirtualTrack>();

		const long TRACK_HEADER_SIZE = 4 + 4 + 34; // at the least
		VirtualTrack currentTrack_ = null;
		
		long sampleCount_ = 0;

		public VirtualFlacCreator(string file, string flacImageFileName, ITrackInfo [] tracksInfo)
		{
			tracksInfo_ = tracksInfo;

			string xml = String.Format("<virtualFlac><flacImage uri=\"{0}\"><tracks></tracks><frames></frames></flacImage></virtualFlac>", flacImageFileName);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			inStream_ = new FlacStream(file, FlacStream.StreamMode.OpenExisting, FlacStream.StreamAccessMode.Read);
			inStream_.BeforeFrameDataRead += new FrameCallback(inStream__BeforeFrameDataRead);

			outStream_ = new FlacStream(Path.GetTempFileName(), FlacStream.StreamMode.CreateNew, FlacStream.StreamAccessMode.Write);
			inStream_.Decode();
			outStream_.Close();

			XmlNode tracks = doc.SelectSingleNode("/virtualFlac/flacImage/tracks");
			foreach (VirtualTrack vt in virtualTracks_)
			{
				XmlNode track = doc.CreateElement("track");
				vt.SaveToXml(track);

				tracks.AppendChild(track);
			}

			string virtualFlacFile = file.Remove(file.Length - ".flac".Length) + ".virtualflac";
			doc.Save(virtualFlacFile);
		}

		void inStream__BeforeFrameDataRead(FrameCallbackArgs args)
		{
			FlacStream inStream = args.Stream;
			long offset = args.Offset;

			Frame frame = args.Frame;
			frame.ReadData(inStream);
			frame.ReadFooter(inStream);

			ProcessFrame(inStream, frame, offset);
			args.HaveReadData = true;
		}

		private void ProcessFrame(FlacStream inStream, Frame frame, long offset)
		{
			long startSample = frame.Header.StartingSampleNumber;
			long endSample = startSample + frame.Header.BlockSize;
			int frameSize = (int)(inStream.Reader.Debug_BytesRead - offset);
			
			if (blockingStrategy_ == -1)
			{
				blockingStrategy_ = frame.Header.BlockingStrategy;
			}
			else
			{
				// whole stream must have the same blocking strategy
				Debug.Assert(frame.Header.BlockingStrategy == blockingStrategy_);
			}

			if (currentTrack_ != null)
			{
				long start = startSample;
				long end = endSample;

				if (start <= currentTrack_.EndSample && currentTrack_.EndSample <= end)
				{
					end = currentTrack_.EndSample;
					WriteFrame(frame, currentTrack_, start, end, frameSize, startSample, offset);
					currentTrack_.TotalSamples = sampleCount_;

					currentTrack_.Frames.Values[0].CalculateVirtualBlockSizeDifference(currentTrack_.StartSample, true);
					currentTrack_.Frames.Values[currentTrack_.Frames.Count - 1].CalculateVirtualBlockSizeDifference(currentTrack_.EndSample, false);

					RefreshFrame(inStream, ref frame, offset);
					currentTrack_ = null;
					goto loop;
				}
				else
				{
					WriteFrame(frame, currentTrack_, start, end, frameSize, startSample, offset);
					goto done;
				}
			}
			else
			{
				goto loop;
			}

		loop:
			foreach (ITrackInfo file in tracksInfo_)
			{
				long start = startSample;
				long end = endSample;
				if (start <= file.StartSample && file.StartSample <= end)
				{
					VirtualTrack lastTrack = null;
					if (virtualTracks_.Count > 0)
					{
						lastTrack = virtualTracks_[virtualTracks_.Count - 1];
					}
					if (lastTrack != null)
					{
						Debug.Assert(lastTrack.Frames.Values[lastTrack.Frames.Count - 1].imageEndSample == endSample);
					}

					currentTrack_ = new VirtualTrack();
					currentTrack_.StartSample =file.StartSample;
					currentTrack_.EndSample = file.EndSample;
					currentTrack_.FileName = file.Name;
					currentTrack_.Size = TRACK_HEADER_SIZE;
					virtualTracks_.Add(currentTrack_);

					sampleCount_ = 0;

					start = file.StartSample;

					bool next = false;
					if (start <= file.EndSample && file.EndSample <= end)
					{
						// track ends in the same frame
						end = file.EndSample;

						// write (probably) partial first frame of track
						WriteFrame(frame, currentTrack_, start, end, frameSize, startSample, offset);
						RefreshFrame(inStream, ref frame, offset);
						next = true;
					}
					else
					{
						// write (probably) partial first frame of track
						WriteFrame(frame, currentTrack_, start, end, frameSize, startSample, offset);
					}
					if (next)
					{
						continue;
					}
					break;
				}
			}
		done:
			return;
		}

		private void RefreshFrame(FlacStream inStream, ref Frame frame, long offset)
		{
			inStream_.Reader.Seek(offset);

			frame = new Frame();
			frame.ReadHeader(inStream);
			frame.ReadData(inStream);
			frame.ReadFooter(inStream);
		}

		private void WriteFrame(Frame frame, VirtualTrack track, long start, long end, int frameSize, long imageStartSample, long offset)
		{
			TrackFrame trackFrame = new TrackFrame(offset, frameSize, imageStartSample);
			trackFrame.imageBlockSize = frame.Header.BlockSize;
			trackFrame.startSample = sampleCount_;
			trackFrame.virtualOffset = track.Size;

			int count = (int)(end - start);

			int virtualSize;
			int sizeDifference = 0;
			if (frame.Header.BlockSize == count)
			{
				sizeDifference = flac.Io.UTF8Encoder.GetUtf8ULong(sampleCount_).Length - frame.Header.SampleIdSize;
				virtualSize = frameSize + sizeDifference;
#if DEBUG
				frame.Header.StartingSampleNumber = sampleCount_;
				frame.Header.BlockingStrategy = FrameHeader.BLOCKING_STRATEGY_VARIABLE;
				
				outStream_.Writer.Truncate(0);
				frame.Write(outStream_);

				Debug.Assert(virtualSize == outStream_.Writer.Debug_BytesWritten);
#endif
			}
			else
			{
				frame.Header.StartingSampleNumber = sampleCount_;
				frame.Header.BlockingStrategy = FrameHeader.BLOCKING_STRATEGY_VARIABLE;
				frame.Header.BlockSize = count;

				outStream_.Writer.Truncate(0);
				frame.Write(outStream_, (int)(start - imageStartSample), count);
				virtualSize = Convert.ToInt32(outStream_.Writer.Debug_BytesWritten);
				sizeDifference = virtualSize - frameSize;
			}
			trackFrame.sizeDifference = sizeDifference;
			track.Frames.Add(trackFrame.virtualOffset, trackFrame);

			track.Size += virtualSize;

			track.MinBlockSize = Math.Min(track.MinBlockSize, frame.Header.BlockSize);
			track.MaxBlockSize = Math.Max(track.MaxBlockSize, frame.Header.BlockSize);
			track.MinFrameSize = (int)Math.Min(track.MinFrameSize, virtualSize);
			track.MaxFrameSize = (int)Math.Max(track.MaxFrameSize, virtualSize);

			sampleCount_ += frame.Header.BlockSize;
		}
	}
}
