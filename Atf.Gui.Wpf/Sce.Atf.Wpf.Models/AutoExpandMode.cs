using System;

namespace Sce.Atf.Wpf.Models;

[Flags]
public enum AutoExpandMode : byte
{
	Disabled = 0,
	ExpandSelected = 1,
	ExpandInserted = 2,
	ExpandInsertedIfParentSelected = 4,
	Default = 5
}
