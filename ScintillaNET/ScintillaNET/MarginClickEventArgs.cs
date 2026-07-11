using System;
using System.Windows.Forms;

namespace ScintillaNET;

public class MarginClickEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private int? position;

	public int Margin { get; private set; }

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

	public MarginClickEventArgs(Scintilla scintilla, Keys modifiers, int bytePosition, int margin)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		Modifiers = modifiers;
		Margin = margin;
	}
}
