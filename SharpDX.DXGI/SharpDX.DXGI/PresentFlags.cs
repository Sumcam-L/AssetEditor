using System;

namespace SharpDX.DXGI;

[Flags]
public enum PresentFlags
{
	Test = 1,
	DoNotSequence = 2,
	Restart = 4,
	None = 0
}
