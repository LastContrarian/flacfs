using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using Dokan;
namespace flacFs
{
    public class FlacDokan : DokanOperations
    {
		FlacFs flacFs_;

        public FlacDokan(string srcDirectory)
        {
			flacFs_ = new FlacFs(srcDirectory);
        }

        // file apis
        public int CreateFile(string filename, FileAccess access, FileShare share, FileMode mode, FileOptions options, DokanFileInfo info)
        {
			info.IsDirectory = flacFs_.IsDirectory(filename);
			if (info.IsDirectory)
			{
				return 0;
			}

			long handle = flacFs_.OpenFile(filename);
			if (handle != FlacFs.INVALID_HANDLE)
			{
				info.Context = handle;
				return 0;
			}
			
			return -DokanNet.ERROR_FILE_NOT_FOUND;
        }

        public int ReadFile(string filename, byte[] buffer, ref uint readBytes, long offset, DokanFileInfo info)
        {
			int bytesRead;
			flacFs_.Read(Convert.ToInt64(info.Context), buffer, offset, buffer.Length, out bytesRead);
			readBytes = Convert.ToUInt32(bytesRead);
			return 0;
        }

        public int WriteFile(string filename, byte[] buffer, ref uint writtenBytes, long offset, DokanFileInfo info)
        {
            return -1;
        }

		public int Cleanup(string filename, DokanFileInfo info)
		{
			return CloseFile(filename, info);
		}

        public int CloseFile(string filename, DokanFileInfo info)
        {
			flacFs_.CloseFile(Convert.ToInt64(info.Context));
			return 0;
        }

        /* for dokan v0.6 */
        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return -1;
        }

        public int DeleteFile(string filename, DokanFileInfo info)
        {
            return -1;
        }

        // directory apis
        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            return -1;
        }

        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            return -1;
        }

        public int OpenDirectory(string filename, DokanFileInfo info)
        {
			return flacFs_.IsDirectory(filename) ? 0 : -DokanNet.ERROR_PATH_NOT_FOUND;
        }

        // info apis
        public int SetFileAttributes(string filename, FileAttributes attr, DokanFileInfo info)
        {
            return -1;
        }

        public int SetFileTime(string filename, DateTime ctime, DateTime atime, DateTime mtime, DokanFileInfo info)
        {
            return -1;
        }

        public int GetFileInformation(string filename, FileInformation fileinfo, DokanFileInfo info)
        {
			FlacFsObjectInfo fsoi = flacFs_.GetFsObjectInfo(filename);
			if (fsoi == null)
			{
				return -1;
			}
			else
			{
				TranslateFileSystemInfo(fsoi, fileinfo);
				return 0;
			}
        }

        // find apis
        public int FindFiles(string filename, System.Collections.ArrayList files, DokanFileInfo info)
        {
			FlacFsObjectInfo[] entries = flacFs_.ListDirectoryContents(filename);
			if (entries == null)
			{
				return -1;
			}

            foreach(FlacFsObjectInfo fsoi in entries)
			{
				FileInformation fi = new FileInformation();
				TranslateFileSystemInfo(fsoi, fi);
				files.Add(fi);
			}
            return 0;
        }

		private static void TranslateFileSystemInfo(FlacFsObjectInfo fsoi, FileInformation fi)
		{
			fi.Attributes = fsoi.Attributes;
			fi.CreationTime = fsoi.CreationTime;
			fi.LastAccessTime = fsoi.LastAccessTime;
			fi.LastWriteTime = fsoi.LastWriteTime;
			fi.Length = fsoi.Length;
			fi.FileName = fsoi.Name;
		}

        // misc file apis
        public int MoveFile(string filename, string newname, bool replace, DokanFileInfo info)
        {
            return -1;
        }

        public int LockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return 0;
        }

        // misc
        public int FlushFileBuffers(string filename, DokanFileInfo info)
        {
            return -1;
        }

        public int Unmount(DokanFileInfo info)
        {
            return 0;
        }

        public int GetDiskFreeSpace(ref ulong freeBytesAvailable, ref ulong totalBytes, ref ulong totalFreeBytes, DokanFileInfo info)
        {
            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return 0;
        }
    }
}
