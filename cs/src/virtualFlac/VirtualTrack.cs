using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace VirtualFlac
{
	public class VirtualTrack
	{
		public VorbisComments Metadata;
		public string ImageFile;

		public string FileName;
		public string FileNameWithExt { get { return FileName + ".flac"; } }

		public long StartSample;
		public long EndSample;
		public long Size;

		public int MinBlockSize = int.MaxValue;
		public int MaxBlockSize;
		public int MinFrameSize = int.MaxValue;
		public int MaxFrameSize;
		public long TotalSamples;

		SortedList<long, TrackFrame> frames_ = new SortedList<long, TrackFrame>();

		public SortedList<long, TrackFrame> Frames { get { return frames_; } }

		void LoadFromXml(string dir, XmlNode track)
		{
			FileName = track.Attributes["name"].Value;
			StartSample = Convert.ToInt64(track.Attributes["startSample"].Value);
			EndSample = Convert.ToInt64(track.Attributes["endSample"].Value);

			Metadata = LoadMetadata(dir, FileName);
			long metadataBlockSize = Metadata.block.Length + 4; // 4 is size of metadatablockheader
			Size = Convert.ToInt64(track.Attributes["size"].Value) + metadataBlockSize;

			MinBlockSize  = Convert.ToInt32(track.Attributes["minBlockSize"].Value);
			MaxBlockSize = Convert.ToInt32(track.Attributes["maxBlockSize"].Value);
			MinFrameSize = Convert.ToInt32(track.Attributes["minFrameSize"].Value);
			MaxFrameSize = Convert.ToInt32(track.Attributes["maxFrameSize"].Value);
			TotalSamples = Convert.ToInt64(track.Attributes["totalSamples"].Value);

			XmlNode frames = track.SelectSingleNode("frames");
			MemoryStream ms = new MemoryStream(Convert.FromBase64String(frames.InnerText));
			BinaryReader br = new BinaryReader(ms);

			while (ms.Position < ms.Length)
			{
				TrackFrame trackFrame = new TrackFrame(br, metadataBlockSize);
				frames_.Add(trackFrame.virtualOffset, trackFrame);
			}
			frames_.Values[0].CalculateVirtualBlockSizeDifference(StartSample, true);
			frames_.Values[frames_.Count - 1].CalculateVirtualBlockSizeDifference(EndSample, false);
		}

		public void SaveToXml(XmlNode track)
		{
			XmlDocument doc = track.OwnerDocument;
			XmlAttribute a;

			a = doc.CreateAttribute("name");
			a.Value = FileName;
			track.Attributes.Append(a);

			a = doc.CreateAttribute("startSample");
			a.Value = StartSample.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("endSample");
			a.Value = EndSample.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("minBlockSize");
			a.Value = MinBlockSize.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("maxBlockSize");
			a.Value = MaxBlockSize.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("minFrameSize");
			a.Value = MinFrameSize.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("maxFrameSize");
			a.Value = MaxFrameSize.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("totalSamples");
			a.Value = TotalSamples.ToString();
			track.Attributes.Append(a);

			a = doc.CreateAttribute("size");
			a.Value = Size.ToString();
			track.Attributes.Append(a);

			XmlNode frames = doc.CreateElement("frames");

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);
			foreach (TrackFrame op in frames_.Values)
			{
				op.Save(bw);
			}
			frames.InnerText = Convert.ToBase64String(ms.ToArray());
			track.AppendChild(frames);
		}

		public static VirtualTrack[] GetTracks(string virtualFlacFile)
		{
			List<VirtualTrack> tracks_ = new List<VirtualTrack>();

			XmlDocument doc = new XmlDocument();
			doc.Load(virtualFlacFile);

			string imageName = doc.SelectSingleNode("/virtualFlac/flacImage").Attributes["uri"].Value;
			XmlNodeList list = doc.SelectNodes("/virtualFlac/flacImage/tracks/track");

			string dir = Directory.GetParent(virtualFlacFile).FullName;

			foreach (XmlNode n in list)
			{
				VirtualTrack vt = new VirtualTrack();
				vt.LoadFromXml(dir, n);
				vt.ImageFile = imageName;
				tracks_.Add(vt);
			}
			return tracks_.ToArray();
		}

		private static VorbisComments LoadMetadata(string dir, string fileName)
		{
			// load metadata
			AplFile metadataFile = new AplFile(Path.Combine(dir, fileName + ".apl"));
			return new VorbisComments(metadataFile.Metadata);
		}
	}
}
