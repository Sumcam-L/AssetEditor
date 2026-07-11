using System;

namespace SharpDX.DXGI;

[Flags]
public enum WindowAssociationFlags
{
	IgnoreAll = 1,
	IgnoreAltEnter = 2,
	IgnorePrintScreen = 4,
	Valid = 7,
	None = 0
}
