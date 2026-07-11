using System;

namespace ScintillaNET;

public class CharAddedEventArgs : EventArgs
{
	public int Char { get; private set; }

	public CharAddedEventArgs(int ch)
	{
		Char = ch;
	}
}
