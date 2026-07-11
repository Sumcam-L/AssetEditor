using System;

namespace Sce.Atf.Input;

public class PreviewKeyDownEventArgs : EventArgs
{
	public readonly Keys KeyData;

	public bool IsInputKey;

	public bool Alt => (KeyData & Keys.Alt) == Keys.Alt;

	public bool Control => (KeyData & Keys.Control) == Keys.Control;

	public bool Shift => (KeyData & Keys.Shift) == Keys.Shift;

	public Keys KeyCode
	{
		get
		{
			Keys keys = KeyData & Keys.KeyCode;
			return Enum.IsDefined(typeof(Keys), (int)keys) ? keys : Keys.None;
		}
	}

	public int KeyValue => (int)(KeyData & Keys.KeyCode);

	public Keys Modifiers => KeyData & Keys.Modifiers;

	public PreviewKeyDownEventArgs(Keys keyData)
	{
		KeyData = keyData;
	}
}
