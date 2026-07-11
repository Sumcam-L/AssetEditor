using System;

namespace ScintillaNET;

[Flags]
public enum UpdateChange
{
	Content = 1,
	Selection = 2,
	VScroll = 4,
	HScroll = 8
}
