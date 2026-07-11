using System;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class NoFocusTrackBar : TrackBar
{
	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		User32.SendMessage(base.Handle, 296, MakeParam(1, 1), 0);
	}

	private static int MakeParam(int loWord, int hiWord)
	{
		return (hiWord << 16) | (loWord & 0xFFFF);
	}
}
