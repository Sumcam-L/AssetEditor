using System;

namespace SharpDX.Direct2D1;

[Flags]
public enum PresentOptions
{
	None = 0,
	RetainContents = 1,
	Immediately = 2
}
