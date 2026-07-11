using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QueryOption : QueryNode
{
	private ToolStripDropDownButton m_toolStripDropDownButton;

	private QueryOptionItem m_selectedItem;

	private ToolStripDropDownButton ToolStripDropDownButton
	{
		get
		{
			if (m_toolStripDropDownButton == null)
			{
				m_toolStripDropDownButton = new ToolStripDropDownButton();
				m_toolStripDropDownButton.TextAlign = ContentAlignment.MiddleCenter;
			}
			return m_toolStripDropDownButton;
		}
	}

	protected QueryOptionItem SelectedItem => m_selectedItem;

	public event EventHandler OptionChanged;

	public void RegisterOptionItem(QueryOptionItem optionItem)
	{
		optionItem.ItemSelected += queryOptionItem_Selected;
	}

	private void queryOptionItem_Selected(object sender, EventArgs e)
	{
		if (sender is QueryOptionItem selectedItem)
		{
			m_selectedItem = selectedItem;
		}
		this.OptionChanged.Raise(this, EventArgs.Empty);
	}

	public override void GetToolStripItems(List<ToolStripItem> items)
	{
		ToolStripDropDownButton.DropDownItems.Clear();
		foreach (QueryOptionItem child in base.Children)
		{
			ToolStripDropDownButton.DropDownItems.Add(child.MenuItem);
		}
		items.Add(ToolStripDropDownButton);
		if (m_selectedItem == null && base.Children.Count > 0)
		{
			m_selectedItem = base.Children[0] as QueryOptionItem;
		}
		if (m_selectedItem != null)
		{
			ToolStripDropDownButton.Text = m_selectedItem.Text;
			m_selectedItem.GetToolStripItems(items);
		}
	}

	public override ToolStripItem GetToolStripItem()
	{
		return ToolStripDropDownButton;
	}

	public override void BuildPredicate(IQueryPredicate predicate)
	{
		if (m_selectedItem != null)
		{
			m_selectedItem.BuildPredicate(predicate);
		}
	}
}
