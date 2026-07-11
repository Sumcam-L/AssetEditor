using System;

namespace ScintillaNET;

public class BeforeModificationEventArgs : EventArgs
{
	private readonly Scintilla scintilla;

	private readonly int bytePosition;

	private readonly int byteLength;

	private readonly IntPtr textPtr;

	internal int? CachedPosition { get; set; }

	internal string CachedText { get; set; }

	public int Position
	{
		get
		{
			if (!CachedPosition.HasValue)
			{
				CachedPosition = scintilla.Lines.ByteToCharPosition(bytePosition);
			}
			return CachedPosition.Value;
		}
	}

	public ModificationSource Source { get; private set; }

	public unsafe virtual string Text
	{
		get
		{
			if (Source != ModificationSource.User)
			{
				return null;
			}
			if (CachedText == null)
			{
				if (textPtr == IntPtr.Zero)
				{
					IntPtr intPtr = scintilla.DirectMessage(2643, new IntPtr(bytePosition), new IntPtr(byteLength));
					CachedText = new string((sbyte*)(void*)intPtr, 0, byteLength, scintilla.Encoding);
				}
				else
				{
					CachedText = Helpers.GetString(textPtr, byteLength, scintilla.Encoding);
				}
			}
			return CachedText;
		}
	}

	public BeforeModificationEventArgs(Scintilla scintilla, ModificationSource source, int bytePosition, int byteLength, IntPtr text)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		this.byteLength = byteLength;
		textPtr = text;
		Source = source;
	}
}
