using System;

namespace Sce.Atf.Wpf.Docking;

public class BooleanArgs : EventArgs
{
	public bool Value { get; private set; }

	public BooleanArgs(bool value)
	{
		Value = value;
	}
}
