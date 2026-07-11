using System;

namespace ScintillaNET;

public class InsertCheckEventArgs : EventArgs
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

	public unsafe string Text
	{
		get
		{
			if (CachedText == null)
			{
				CachedText = Helpers.GetString(textPtr, byteLength, scintilla.Encoding);
			}
			return CachedText;
		}
		set
		{
			CachedText = value ?? string.Empty;
			byte[] bytes = Helpers.GetBytes(CachedText, scintilla.Encoding, zeroTerminated: false);
			fixed (byte* value2 = bytes)
			{
				scintilla.DirectMessage(2672, new IntPtr(bytes.Length), new IntPtr(value2));
			}
		}
	}

	public InsertCheckEventArgs(Scintilla scintilla, int bytePosition, int byteLength, IntPtr text)
	{
		this.scintilla = scintilla;
		this.bytePosition = bytePosition;
		this.byteLength = byteLength;
		textPtr = text;
	}
}
