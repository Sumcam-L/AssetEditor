using System;
using System.Runtime.InteropServices;

namespace SharpDX;

[Guid("00000000-0000-0000-C000-000000000046")]
public interface IUnknown
{
	Result QueryInterface(ref Guid guid, out IntPtr comObject);

	int AddReference();

	int Release();
}
