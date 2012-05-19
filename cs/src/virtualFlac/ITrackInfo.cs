using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualFlac
{
	public interface ITrackInfo
	{
		string Name { get;}
		string ImageFile { get;}
		long StartSample { get;}
		long EndSample { get;}

		ITrackMetadata[] Metadata { get;}
	}
}
