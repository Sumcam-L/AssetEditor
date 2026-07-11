using System;

namespace Sce.Atf.Input;

public class KeyEventArgs : EventArgs
{
	public virtual bool Alt => (KeyData & Keys.Alt) != 0;

	public virtual bool Control => (KeyData & Keys.Control) != 0;

	public bool Handled { get; set; }

	public Keys KeyCode => KeyData & Keys.KeyCode;

	public Keys KeyData { get; private set; }

	public int KeyValue => (int)(KeyData & Keys.KeyCode);

	public Keys Modifiers { get; set; }

	public virtual bool Shift => (KeyData & Keys.Shift) != 0;

	public bool SuppressKeyPress { get; set; }

	public KeyEventArgs(Keys keyData)
	{
		KeyData = keyData;
	}
}
