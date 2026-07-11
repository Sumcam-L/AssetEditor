using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QueryOptionItem : QueryNode
{
	private readonly string m_text;

	private readonly ulong m_tag;

	public string Text => m_text;

	public ulong Tag => m_tag;

	public ToolStripMenuItem MenuItem
	{
		get
		{
			ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(m_text, null, queryOptionItem_Clicked);
			toolStripMenuItem.CheckOnClick = false;
			toolStripMenuItem.Checked = false;
			toolStripMenuItem.Tag = this;
			return toolStripMenuItem;
		}
	}

	public event EventHandler ItemSelected;

	private QueryOptionItem()
	{
	}

	public QueryOptionItem(string text, ulong tag)
	{
		m_text = text;
		m_tag = tag;
	}

	private void queryOptionItem_Clicked(object sender, EventArgs e)
	{
		this.ItemSelected.Raise(this, EventArgs.Empty);
	}
}
