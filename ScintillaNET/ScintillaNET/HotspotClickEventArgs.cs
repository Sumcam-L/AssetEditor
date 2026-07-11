using System;
using System.Windows.Forms;

namespace ScintillaNET;

public class HotspotClickEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private int? position;

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

	public HotspotClickEventArgs(Scintilla scintilla, Keys modifiers, int bytePosition)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		Modifiers = modifiers;
	}
}
