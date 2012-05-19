using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using flac;
using VirtualFlac;
namespace VirtualFlacGen
{
	class Program
	{
		static List<AplFile> aplFiles_ = new List<AplFile>();
		static Stopwatch watch_ = new Stopwatch();

		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				throw new VfgException("Invalid argument.");
			}

			string arg = args[0];
			string file = args[1];

			if (arg == "-g")
			{
				GenerateVirtualFlac(file);
			}
			else if (arg == "-ct")
			{
				WriteTracks(file);
			}
		}

		private static void WriteTracks(string virtualFlacFile)
		{
			VirtualTrack [] tracks = VirtualTrack.GetTracks(virtualFlacFile);
			foreach(VirtualTrack vt in tracks)
			{
				string dir = Directory.GetParent(virtualFlacFile).FullName;
				string flacImage = Path.Combine(dir, vt.ImageFile);
				VirtualFlacTrack ft = new VirtualFlacTrack(flacImage, vt);

				FileStream fs = new FileStream(Path.Combine(dir, vt.FileNameWithExt), FileMode.Create);

				Init("Writing " + fs.Name + " ...");

				const int BUFFER_SIZE = 1024 * 128;
				byte[] data = new byte[BUFFER_SIZE];
				int offset = 0;
				while (offset < vt.Size)
				{
					int read = BUFFER_SIZE;// Math.Min(BUFFER_SIZE, (int)vt.Size - offset);
					ft.Read(data, offset, read);
					offset += read;
					fs.Write(data, 0, read);
					fs.Flush();
				}
				fs.Close();
				Done();
			}
		}

		private static void GenerateVirtualFlac(string file)
		{
			Init("Creating...");

			string fileName = new FileInfo(file).Name;

			string aplDirectory = Directory.GetParent(file).FullName;
			string[] files = Directory.GetFiles(aplDirectory, "*.apl");
			foreach (string f in files)
			{
				AplFile apl = new AplFile(f);
				if (apl.ImageFile.ToLower() != fileName.ToLower())
				{
					Console.WriteLine(apl.ImageFile);
					Console.WriteLine(fileName);

					throw new VfgException("Invalid apl/ image combination");
				}
				aplFiles_.Add(apl);
			}

			new VirtualFlacCreator(file, fileName, aplFiles_.ToArray());
			Done();
		}

		static void Init(string message)
		{
			watch_.Start();
			Console.Write(message);
		}

		static void Done()
		{
			TimeSpan t = watch_.Elapsed;
			watch_.Stop();
			Console.WriteLine("done [" + t.Minutes + "m " + t.Seconds + "s " + t.Milliseconds + "ms]");
		}
	}
}