using System;

namespace ScintillaNET;

[Flags]
public enum VirtualSpace
{
	None = 0,
	RectangularSelection = 1,
	UserAccessible = 2,
	NoWrapLineStart = 4
}
