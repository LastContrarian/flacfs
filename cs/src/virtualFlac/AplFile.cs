using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace VirtualFlac
{
	public class AplFile : ITrackInfo
	{
		const string aplPattern = @"\[Monkey's Audio Image Link File\]\r\nImage File=(.*)\r\nStart Block=(.*)\r\nFinish Block=(.*)";
		const string SIGNATURE = "[Monkey's Audio Image Link File]";

		string name_;
		string imageFile_;
		long startSample_;
		long endSample_;
		List<ITrackMetadata> metadata_ = new List<ITrackMetadata>();

		public string ImageFile { get { return imageFile_; } }
		public long StartSample { get { return startSample_; } }
		public long EndSample { get { return endSample_; } }
		public string Name { get { return name_; } }
		public ITrackMetadata [] Metadata { get { return metadata_.ToArray(); } }

		public AplFile(string file)
		{
			byte [] contents = File.ReadAllBytes(file);
			string data = Encoding.UTF8.GetString(contents);

			Match m = Regex.Match(data, aplPattern);

			imageFile_ = m.Groups[1].Value;
			startSample_ = Convert.ToInt64(m.Groups[2].Value);
			endSample_ = Convert.ToInt64(m.Groups[3].Value);
			name_ = new FileInfo(file).Name;
			name_ = name_.Remove(name_.Length - ".apl".Length);

			ParseMetadata(data);
		}

		class ApeFlags
		{
			public readonly bool IsString;

			public ApeFlags(BinaryReader reader)
			{
				uint flag = reader.ReadUInt32();

				// first byte 0x00001100b
				IsString = ((flag & 0x0c000000) >> 26) == 0;
			}
		}

		class ApeMetadataItem : ITrackMetadata
		{
			int itemValueLength_;
			ApeFlags flags_;

			string key_;
			string value_;

			public string Key { get { return key_; } }
			public string Value { get { return value_; } }

			public ApeMetadataItem(BinaryReader reader)
			{
				itemValueLength_ = reader.ReadInt32();
				flags_ = new ApeFlags(reader);

				while (reader.PeekChar() != 0)
				{
					key_ += reader.ReadChar();
				};
				reader.ReadByte();
				if (flags_.IsString)
				{
					value_ = Encoding.UTF8.GetString(reader.ReadBytes(itemValueLength_));
				}
				else
				{
					reader.ReadBytes(itemValueLength_);
				}
			}
		}

		void ParseMetadata(string data)
		{
			int tagStart = data.IndexOf("APETAGEX");
			if (tagStart == -1)
			{
				return;
			}
			data = data.Substring(tagStart);
			BinaryReader br = new BinaryReader(new MemoryStream(Encoding.ASCII.GetBytes(data)), Encoding.ASCII);
			br.ReadBytes(8 + 4);

			int tagSize = br.ReadInt32();
			int itemCount = br.ReadInt32();
			ApeFlags flags = new ApeFlags(br);
			ulong reserved = br.ReadUInt64();

			int i = 0;
			while (i < itemCount)
			{
				ApeMetadataItem metadataItem = new ApeMetadataItem(br);
				metadata_.Add(metadataItem);
				i++;
			}
		}
	}
}
