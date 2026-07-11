using System;

namespace ScintillaNET;

public class DwellEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private int? position;

	public int Position
	{
		get
		{
			if (!position.HasValue)
			{
				position = scintilla.Lines.ByteToCharPosition(bytePosition);
			}
			return position.Value;
		}
	}

	public int X { get; private set; }

	public int Y { get; private set; }

	public DwellEventArgs(Scintilla scintilla, int bytePosition, int x, int y)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		X = x;
		Y = y;
		if (bytePosition < 0)
		{
			position = bytePosition;
		}
	}
}
