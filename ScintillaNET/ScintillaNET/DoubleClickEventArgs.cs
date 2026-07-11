using System;
using System.Windows.Forms;

namespace ScintillaNET;

public class DoubleClickEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private int? position;

	public int Line { get; private set; }

	public Keys Modifiers { get; private set; }

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

	public DoubleClickEventArgs(Scintilla scintilla, Keys modifiers, int bytePosition, int line)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		Modifiers = modifiers;
		Line = line;
		if (bytePosition == -1)
		{
			position = -1;
		}
	}
}
