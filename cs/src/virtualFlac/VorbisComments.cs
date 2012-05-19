using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VirtualFlac
{
	public class VorbisComments
	{
		public readonly byte[] block;

		public VorbisComments(ITrackMetadata[] metadata)
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(ms);

			string vendorString = "flacFS";
			writer.Write(vendorString.Length);
			writer.Write(Encoding.UTF8.GetBytes(vendorString));
			writer.Write(metadata.Length);

			foreach (ITrackMetadata m in metadata)
			{
				if (m.Key.Equals("Track", StringComparison.InvariantCultureIgnoreCase))
				{
					string [] track = m.Value.Split(new char[]{'/'});
					
					string comment;

					comment = "TrackNumber=" + Convert.ToInt32(track[0]).ToString();
					writer.Write(comment.Length);
					writer.Write(Encoding.UTF8.GetBytes(comment));

					comment = "TotalTracks=" + Convert.ToInt32(track[1]).ToString();
					writer.Write(comment.Length);
					writer.Write(Encoding.UTF8.GetBytes(comment));
				}
				else if (m.Key.Equals("Year", StringComparison.InvariantCultureIgnoreCase))
				{
					string comment = "Date=" +  m.Value;
					writer.Write(comment.Length);
					writer.Write(Encoding.UTF8.GetBytes(comment));
				}
				else
				{
					string[] v = m.Value.Split(new char[] { '\0' });
					foreach (string s in v)
					{
						string comment = m.Key + "=" + s;
						writer.Write(comment.Length);
						writer.Write(Encoding.UTF8.GetBytes(comment));
					}
				}
			}
			block = ms.GetBuffer();
			writer.Close();
		}
	}
}
