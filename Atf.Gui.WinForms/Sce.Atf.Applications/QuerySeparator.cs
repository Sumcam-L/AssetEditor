using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QuerySeparator : QueryNode
{
	private ToolStripSeparator m_toolStripSeparator;

	private ToolStripSeparator ToolStripSeparator
	{
		get
		{
			if (m_toolStripSeparator == null)
			{
				m_toolStripSeparator = new ToolStripSeparator();
			}
			return m_toolStripSeparator;
		}
	}

	public override void GetToolStripItems(List<ToolStripItem> items)
	{
		items.Add(ToolStripSeparator);
		base.GetToolStripItems(items);
	}

	public override ToolStripItem GetToolStripItem()
	{
		return ToolStripSeparator;
	}
}
