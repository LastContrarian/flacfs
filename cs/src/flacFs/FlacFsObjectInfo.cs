using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace flacFs
{
	public class FlacFsObjectInfo
	{
		public string Name;
		public FileAttributes Attributes;
		public DateTime CreationTime;
		public DateTime LastAccessTime;
		public DateTime LastWriteTime;
		public long Length;

		public FlacFsObjectInfo(DirectoryInfo directory)
		{
			Name = directory.Name;
			Attributes = directory.Attributes;
			CreationTime = directory.CreationTime;
			LastAccessTime = directory.LastAccessTime;
			LastWriteTime = directory.LastWriteTime;
			Length = 0;
		}

		public FlacFsObjectInfo(FileInfo file, VirtualFlac.VirtualTrack track)
		{
			Name = track.FileNameWithExt;
			Attributes = file.Attributes;
			CreationTime = file.CreationTime;
			LastAccessTime = file.LastAccessTime;
			LastWriteTime = file.LastWriteTime;
			Length = track.Size;
		}
	}
}