flacfs
======

A virtual file system that exposes a flac image and associated meta-data as multiple, independent flac files.

I developed this software as a proof-of-concept. It is functional, but neither complete nor robust. For details on the project, refer to cs/docs/doc.html. It's a modified version of a blogpost I wrote in mid-2009 about the idea behind the project and how I implemented it. The software is cross-platform, and uses dokan on Windows and fuse on Linux to do what it does.

The software is licensed under GPLv2 (or, at your option, any subsequent version). Do note that some code in the following files:
* flac\Utils\CRC8.cs
* flac\Utils\CRC16.cs
* flac\Io\UTF8Decoder.cs
* flac\Io\UTF8Encoder.cs

are based on/copied from the jflac project [http://jflac.sourceforge.net] which is in turn based on the original libflac project [http://flac.sourceforge.net] which is licenced under the GNU Library General Public License v2 or above.

Copyright for the code:

Copyright (c) 2009-2012 Last Contrarian (a.k.a Aristotle the Geek) <last.contrarian@gmail.com>. All rights reserved. Portions copyright (c) 2000-2003 Josh Coalson.

Copyright for the contents of the cs/docs folder (licensed under the Creative Commons Attribution-ShareAlike License 3.0):

Copyright (c) 2009-2012 Last Contrarian (a.k.a Aristotle the Geek) <last.contrarian@gmail.com>. All rights reserved.