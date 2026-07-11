using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QueryLabel : QueryNode
{
	private ToolStripLabel m_toolStripLabel;

	private readonly string m_text;

	private ToolStripLabel ToolStripLabel
	{
		get
		{
			if (m_toolStripLabel == null)
			{
				m_toolStripLabel = new ToolStripLabel(m_text);
			}
			return m_toolStripLabel;
		}
	}

	private QueryLabel()
	{
	}

	public QueryLabel(string text)
	{
		m_text = text;
	}

	public override void GetToolStripItems(List<ToolStripItem> items)
	{
		items.Add(ToolStripLabel);
		base.GetToolStripItems(items);
	}

	public override ToolStripItem GetToolStripItem()
	{
		return ToolStripLabel;
	}
}
