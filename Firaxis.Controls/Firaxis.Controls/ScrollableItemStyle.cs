using System;

namespace Firaxis.Controls;

[Flags]
public enum ScrollableItemStyle
{
	Normal = 1,
	Collapsed = 2,
	Expanded = 4,
	Leaf = 8,
	Hot = 0x10
}
