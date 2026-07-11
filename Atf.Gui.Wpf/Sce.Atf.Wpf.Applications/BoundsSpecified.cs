using System;

namespace Sce.Atf.Wpf.Applications;

[Flags]
public enum BoundsSpecified
{
	None = 0,
	X = 1,
	Y = 2,
	Location = 3,
	Width = 4,
	Height = 8,
	Size = 0xC,
	All = 0xF
}
