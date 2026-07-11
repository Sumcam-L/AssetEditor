using System;

namespace SharpDX.IO;

[Flags]
public enum NativeFileShare : uint
{
	None = 0u,
	Read = 1u,
	Write = 2u,
	ReadWrite = 3u,
	Delete = 4u
}
