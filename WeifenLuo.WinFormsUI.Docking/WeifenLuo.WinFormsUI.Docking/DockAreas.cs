using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace WeifenLuo.WinFormsUI.Docking;

[Serializable]
[Flags]
[Editor(typeof(DockAreasEditor), typeof(UITypeEditor))]
public enum DockAreas
{
	Float = 1,
	DockLeft = 2,
	DockRight = 4,
	DockTop = 8,
	DockBottom = 0x10,
	Document = 0x20
}
