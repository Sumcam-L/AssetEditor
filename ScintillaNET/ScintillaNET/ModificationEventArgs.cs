using System;

namespace ScintillaNET;

public class ModificationEventArgs : BeforeModificationEventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private readonly int byteLength;

	private readonly IntPtr textPtr;

	public int LinesAdded { get; private set; }

	public override string Text
	{
		get
		{
			if (base.CachedText == null)
			{
				base.CachedText = Helpers.GetString(textPtr, byteLength, scintilla.Encoding);
			}
			return base.CachedText;
		}
	}

	public ModificationEventArgs(Scintilla scintilla, ModificationSource source, int bytePosition, int byteLength, IntPtr text, int linesAdded)
		: base(scintilla, source, bytePosition, byteLength, text)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		this.byteLength = byteLength;
		textPtr = text;
		LinesAdded = linesAdded;
	}
}
