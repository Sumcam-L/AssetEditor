using System;

namespace Sce.Atf.Input;

public class KeyPressEventArgs : EventArgs
{
	public char KeyChar;

	public bool Handled;
}
