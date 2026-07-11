using System;

namespace Sce.Atf.Applications;

[Flags]
public enum StandardDockAreas
{
	Float = 1,
	DockLeft = 2,
	DockRight = 4,
	DockTop = 8,
	DockBottom = 0x10,
	Document = 0x20,
	Default = 0x3F
}
