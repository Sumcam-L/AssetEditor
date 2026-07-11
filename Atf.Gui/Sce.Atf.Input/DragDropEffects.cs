using System;

namespace Sce.Atf.Input;

[Flags]
public enum DragDropEffects
{
	Scroll = int.MinValue,
	All = -2147483645,
	None = 0,
	Copy = 1,
	Move = 2,
	Link = 4
}
