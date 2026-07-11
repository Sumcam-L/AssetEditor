using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class ToolStripAutoFitLabel : ToolStripLabel
{
	private int m_minimumWidth = 50;

	private int m_maximumWidth = 200;

	public int MinimumWidth
	{
		get
		{
			return m_minimumWidth;
		}
		set
		{
			m_minimumWidth = value;
		}
	}

	public int MaximumWidth
	{
		get
		{
			return m_maximumWidth;
		}
		set
		{
			m_maximumWidth = value;
		}
	}

	public override Size GetPreferredSize(Size constrainingSize)
	{
		int preferredWidth = GdiUtil.GetPreferredWidth<ToolStripAutoFitLabel>(base.Owner);
		preferredWidth = Math.Max(MinimumWidth, preferredWidth);
		preferredWidth = Math.Min(MaximumWidth, preferredWidth);
		return new Size(preferredWidth, base.GetPreferredSize(constrainingSize).Height);
	}
}
