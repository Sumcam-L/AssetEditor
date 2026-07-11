using System;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

[Shadow(typeof(ComStreamBaseShadow))]
[Guid("0c733a30-2a1c-11ce-ade5-00aa0044773d")]
public interface IStreamBase : ICallbackable, IDisposable
{
	int Read(IntPtr buffer, int numberOfBytesToRead);

	int Write(IntPtr buffer, int numberOfBytesToRead);
}
