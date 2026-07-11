using System;

namespace SharpDX.WIC;

[Flags]
public enum ProgressOperation
{
	CopyPixels = 1,
	WritePixels = 2,
	All = 0xFFFF
}
