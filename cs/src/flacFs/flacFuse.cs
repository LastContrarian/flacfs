using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mono.Fuse;
using Mono.Unix.Native;
namespace flacFs
{
	public class FlacFuse : FileSystem
	{
		FlacFs flacFs_;

		public FlacFuse(string srcDirectory)
		{
			flacFs_ = new FlacFs(srcDirectory);
		}

		protected override Errno OnOpenHandle(string file, Mono.Fuse.OpenedPathInfo info)
		{
			if (info.OpenAccess != OpenFlags.O_RDONLY)
			{
				return Errno.EACCES;
			}

			long handle = flacFs_.OpenFile(file);
			if (handle != FlacFs.INVALID_HANDLE)
			{
				info.Handle = new IntPtr(handle);
				return 0;
			}
			
			return Errno.EACCES;		
		}

		protected override Errno OnReadHandle(string file, Mono.Fuse.OpenedPathInfo info, byte[] buf, long offset, out int bytesWritten)
		{
			long handle = info.Handle.ToInt64();
			flacFs_.Read(handle, buf, offset, buf.Length, out bytesWritten);
			return 0;
		}

		protected override Errno OnReleaseHandle(string file, Mono.Fuse.OpenedPathInfo info)
		{
			flacFs_.CloseFile(info.Handle.ToInt64());
			return 0;
		}

		protected override Errno OnFlushHandle(string file, OpenedPathInfo info)
		{
 			 return OnReleaseHandle(file, info);
		}

		protected override Errno OnReadDirectory(string directory, OpenedPathInfo info, out IEnumerable<DirectoryEntry> paths)
		{
			List<DirectoryEntry> entries = new List<DirectoryEntry>();
			entries.Add(new DirectoryEntry("."));
			entries.Add(new DirectoryEntry(".."));

			FlacFsObjectInfo [] fsEntries = flacFs_.ListDirectoryContents(directory);
			foreach(FlacFsObjectInfo fsoi in fsEntries)
			{
				entries.Add(new DirectoryEntry(fsoi.Name));
			}
			paths = entries;
			return 0;
		}

		protected override Errno OnGetPathStatus(string path, out Stat stat)
		{
			stat = new Stat();

			FlacFsObjectInfo fsoi = flacFs_.GetFsObjectInfo(path);
			if (fsoi == null)
			{
				return Errno.ENOENT;
			}
			
			if ((fsoi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
			{
				stat.st_mode = FilePermissions.S_IFDIR | NativeConvert.FromOctalPermissionString("0755");
				stat.st_nlink = 2;
			}
			else
			{
				stat.st_mode = FilePermissions.S_IFREG | NativeConvert.FromOctalPermissionString("0444");
				stat.st_nlink = 1;
			}
			
			stat.st_ctime = Mono.Unix.Native.NativeConvert.FromDateTime(fsoi.LastWriteTime);
			stat.st_atime = Mono.Unix.Native.NativeConvert.FromDateTime(fsoi.LastAccessTime);
			stat.st_mtime = Mono.Unix.Native.NativeConvert.FromDateTime(fsoi.LastWriteTime);
			stat.st_size = fsoi.Length;
			return 0;
		}

		/*protected override Errno OnGetPathStatus (string path, ref Stat stbuf)
		{
			WriteLine("OnGetPathStatus", path);
			return 0;
		}
	
		protected override Errno OnReadDirectory (string directory, Mono.Fuse.OpenedPathInfo info, out System.Collections.Generic.IEnumerable<Mono.Fuse.DirectoryEntry> paths)
		{
			WriteLine("OnReadDirectory", directory);
		
			Console.WriteLine(directory);
			Console.WriteLine(root_);
	
		
		}
	
		protected override Errno OnListPathExtendedAttributes (string path, out string[] names)
		{
			Console.WriteLine(path);
			names = Directory.GetDirectories(path);
			return 0;
		}*/
	}
}