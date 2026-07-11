using System;

namespace Sce.Atf.Controls;

public class SpinDirectionEventArgs : EventArgs
{
	public readonly int Direction;

	public readonly int Value;

	public SpinDirectionEventArgs(int direction, int value)
	{
		Direction = direction;
		Value = value;
	}
}
