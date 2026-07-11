using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Firaxis.Utility;

public static class KeyHelper
{
	private const int VK_SHIFT = 16;

	private const int VK_CONTROL = 17;

	private const int VK_MENU = 18;

	public static bool CtrlPressed => NativeMethods.GetAsyncKeyState(17) < 0;

	public static bool ShiftPressed => NativeMethods.GetAsyncKeyState(16) < 0;

	public static bool AltPressed => NativeMethods.GetAsyncKeyState(18) < 0;

	public static bool ProcessCmdKeys(object sender, EventArgs e, Keys key, IEnumerable<AccelKey> keys)
	{
		Control control = Control.FromHandle(NativeMethods.GetFocus());
		if (control != null && (control is TextBox || control is RichTextBox || control is ComboBox))
		{
			return false;
		}
		foreach (AccelKey key2 in keys)
		{
			if (key == key2.Keys)
			{
				key2.Handler(sender, e);
				return true;
			}
		}
		return false;
	}
}
