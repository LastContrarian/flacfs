using System;
using System.Collections.Generic;
using System.Text;

using Dokan;
namespace flacFs
{
    class Program
    {
        static void Main(string[] args)
        {
			if (args.Length != 2)
			{
				Console.WriteLine("flacFS - a virtual file system for flac images");
				Console.WriteLine("Usage: flacFs /src/directory /mount/point");
				Console.WriteLine("");
				Console.WriteLine("Notes for Windows:");
				Console.WriteLine("a. mount point is a drive letter.");
				Console.WriteLine("b. if an argument contains space characters, it should be enclosed in double quotes.");
				Console.WriteLine("c. the backspace at the end of a path in quotes should be escaped.");
				return;
			}

			string srcDirectory = System.IO.Path.GetFullPath(args[0]);
			string mountPoint = args[1];
			const string volumeLabel = "flacFs";
			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == (PlatformID)128) // 128 is defined by Mono as Unix
			{
				using (FlacFuse flacFuse = new FlacFuse(srcDirectory))
				{
					flacFuse.MountPoint = mountPoint;
					flacFuse.Name = volumeLabel;
					flacFuse.Start();
				}
			}
			else
			{
				DokanOptions opt = new DokanOptions();
                opt.MountPoint = mountPoint;
				//opt.DebugMode = true;
				//opt.UseStdErr = true;
				opt.VolumeLabel = volumeLabel;
				DokanNet.DokanMain(opt, new FlacDokan(srcDirectory));
			}
        }
    }
}
