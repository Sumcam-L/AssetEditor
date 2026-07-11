using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(FontFileStreamShadow))]
[Guid("6d4865fe-0ab8-4d91-8f62-5dd6be34a3e0")]
public interface FontFileStream : ICallbackable, IDisposable
{
	void ReadFileFragment(out IntPtr fragmentStart, long fileOffset, long fragmentSize, out IntPtr fragmentContext);

	void ReleaseFileFragment(IntPtr fragmentContext);

	long GetFileSize();

	long GetLastWriteTime();
}
