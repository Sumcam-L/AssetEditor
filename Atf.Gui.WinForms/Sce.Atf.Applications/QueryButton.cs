using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QueryButton : QueryNode
{
	private ToolStripButton m_toolStripButton;

	private readonly string m_text;

	private ToolStripButton ToolStripButton
	{
		get
		{
			if (m_toolStripButton == null)
			{
				m_toolStripButton = new ToolStripButton(m_text);
				m_toolStripButton.Click += ToolStripButton_Clicked;
			}
			return m_toolStripButton;
		}
	}

	public event EventHandler Clicked;

	private QueryButton()
	{
	}

	public QueryButton(string text)
	{
		m_text = text;
	}

	private void ToolStripButton_Clicked(object sender, EventArgs e)
	{
		this.Clicked.Raise(sender, e);
	}

	public override void GetToolStripItems(List<ToolStripItem> items)
	{
		items.Add(ToolStripButton);
		base.GetToolStripItems(items);
	}

	public override ToolStripItem GetToolStripItem()
	{
		return ToolStripButton;
	}
}
