using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using VirtualFlac;
namespace flacFs
{
	class FlacFs
	{
		public const long INVALID_HANDLE = -1;

		class Handle
		{
			long handleId_ = 0;

			public long Next()
			{
				handleId_++;
				return handleId_;
			}
		}

		const string extVirtualFlac = ".virtualflac";

		Dictionary<string, VirtualTrack[]> directoryLookup_ = new Dictionary<string, VirtualTrack[]>();
		Dictionary<string, VirtualTrack> fileSystem_ = new Dictionary<string, VirtualTrack>();
		SortedDictionary<long, VirtualFlacTrack> openFiles_ = new SortedDictionary<long, VirtualFlacTrack>();

		string root_;
		Handle handle_;

		public FlacFs(string root)
		{
			root_ = root;
			handle_ = new Handle();
		}

		public bool IsDirectory(string relativePath)
		{
			return Directory.Exists(GetPath(relativePath));
		}

		public long OpenFile(string relativePath)
		{
			long handle = INVALID_HANDLE;
			VirtualFlacTrack vft = GetTrack(relativePath);
			if (vft != null)
			{
				lock (openFiles_)
				{
					lock(handle_)
					{
						handle = handle_.Next();
					}
					openFiles_.Add(handle, vft);
				}
			}
			return handle;
		}

		public void Read(long handle, byte[] buffer, long offset, int count, out int bytesRead)
		{
			VirtualFlacTrack vft = null;
			lock(openFiles_)
			{
				if (openFiles_.ContainsKey(handle))
				{
					vft = openFiles_[handle];
				}
			}

			bytesRead = 0;
			if (vft != null)
			{
				bytesRead = vft.Read(buffer, offset, count);
			}
		}

		public void CloseFile(long handle)
		{
			lock (openFiles_)
			{
				if (openFiles_.ContainsKey(handle))
				{
					openFiles_[handle].Close();
					openFiles_.Remove(handle);
				}
			}
		}

		public string KeyFromPath(string fullPath)
		{
			return fullPath.Trim(new char[] { '\\', '/' });
		}

		public FlacFsObjectInfo GetFsObjectInfo(string relativePath)
		{
			string fullPath = GetPath(relativePath);
			if (Directory.Exists(fullPath))
			{
				return new FlacFsObjectInfo(new DirectoryInfo(fullPath));
			}
			else
			{
				lock (fileSystem_)
				{
					string key = KeyFromPath(fullPath);
					if (fileSystem_.ContainsKey(key))
					{
						VirtualTrack vt = fileSystem_[key];
						string image = Path.Combine(Directory.GetParent(fullPath).FullName, vt.ImageFile);
						return new FlacFsObjectInfo(new FileInfo(image), vt);
					}
				}
				return null;
			}
		}

		public FlacFsObjectInfo[] ListDirectoryContents(string relativePath)
		{
			string fullPath = GetPath(relativePath);

			if (!Directory.Exists(fullPath))
			{
				return null;
			}

			List<FlacFsObjectInfo> entries = new List<FlacFsObjectInfo>();

			DirectoryInfo parent = new DirectoryInfo(fullPath);
			DirectoryInfo [] directories = parent.GetDirectories();
			foreach(DirectoryInfo di in directories)
			{
				FlacFsObjectInfo fsoi = new FlacFsObjectInfo(di);
				entries.Add(fsoi);
			}

			ListDirectoryFiles(parent);

			lock (directoryLookup_)
			{
				string key = KeyFromPath(parent.FullName);
				if (directoryLookup_.ContainsKey(key))
				{
					FileInfo fi = null;
					VirtualTrack[] tracks = directoryLookup_[key];
					foreach (VirtualTrack vt in tracks)
					{
						if (fi == null)
						{
							fi = new FileInfo(Path.Combine(parent.FullName, vt.ImageFile));
						}

						FlacFsObjectInfo fsoi = new FlacFsObjectInfo(fi, vt);
						entries.Add(fsoi);
					}
				}
			}
			return entries.ToArray();
		}

		string GetPath(string relativePath)
		{
			return Path.Combine(root_, relativePath.Trim(new char[] { '\\', '/' }));
		}

		VirtualFlacTrack GetTrack(string relativePath)
		{
			string fullPath = GetPath(relativePath);

			VirtualTrack vt = null;
			DirectoryInfo parent = Directory.GetParent(fullPath);
			
			ListDirectoryFiles(parent);
			lock (fileSystem_)
			{
				string key = KeyFromPath(fullPath);
				if (fileSystem_.ContainsKey(key)) vt = fileSystem_[key];
			}

			if (vt != null)
			{
				return new VirtualFlacTrack(Path.Combine(parent.FullName, vt.ImageFile), vt);
			}
			else
			{
				return null;
			}
		}

		private void ListDirectoryFiles(DirectoryInfo parent)
		{
			lock (directoryLookup_)
			{
				string dirKey = KeyFromPath(parent.FullName);
				if (!directoryLookup_.ContainsKey(dirKey))
				{
					FileInfo[] files = parent.GetFiles("*" + extVirtualFlac);
					if (files.Length == 1)
					{
						lock (fileSystem_)
						{
							string virtualFlacFile = files[0].FullName;
							VirtualTrack[] tracks = VirtualTrack.GetTracks(virtualFlacFile);
							foreach (VirtualTrack t in tracks)
							{
								string filePath = Path.Combine(parent.FullName, t.FileNameWithExt);
								string key = KeyFromPath(filePath);
								System.Diagnostics.Debug.Assert(!fileSystem_.ContainsKey(key));
								fileSystem_.Add(key, t);
							}
							// insert directory into the system
							directoryLookup_.Add(dirKey, tracks);
						}
					}
				}
			}
		}
	}
}