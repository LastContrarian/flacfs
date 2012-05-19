using System;
using System.Collections.Generic;
using System.Text;

using flac;
using flac.Format;
namespace flacTest
{
    class Program
    {
        static void Main(string[] args)
        {
			Work(args[0], args[1], args);
            return;
        }

		private static void Work(string arg, string file, string [] args)
		{
			if (arg == "-a")
			{
				new AnalyzeFile(file);
			}

			if (arg == "-e")
			{
				long start = Convert.ToInt64(args[2]);
				long end = Convert.ToInt64(args[3]);

				new ExtractTrack(file, start, end);
			}

			if (arg == "-c")
			{
				new CopyFile(file);
			}
		}
    }
}
