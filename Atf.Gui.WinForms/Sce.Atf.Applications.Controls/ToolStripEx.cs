using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications.Controls;

public class ToolStripEx : ToolStrip
{
	private bool m_clickThrough = true;

	public bool ClickThrough
	{
		get
		{
			return m_clickThrough;
		}
		set
		{
			m_clickThrough = value;
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (ClickThrough && (long)m.Msg == 33 && m.Result == (IntPtr)2L)
		{
			m.Result = (IntPtr)1L;
		}
	}
}
