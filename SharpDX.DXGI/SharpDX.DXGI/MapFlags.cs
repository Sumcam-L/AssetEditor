using System;

namespace SharpDX.DXGI;

[Flags]
public enum MapFlags
{
	Read = 1,
	Write = 2,
	Discard = 4
}
