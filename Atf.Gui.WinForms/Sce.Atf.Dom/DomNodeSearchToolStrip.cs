using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class DomNodeSearchToolStrip : SearchToolStrip
{
	private readonly DomNodeQueryRoot m_rootNode;

	internal bool QueryDirty
	{
		get
		{
			return m_rootNode.QueryDirty;
		}
		set
		{
			m_rootNode.QueryDirty = value;
		}
	}

	internal bool QueryWithEmptyFields
	{
		get
		{
			foreach (object item in Items)
			{
				if (item is ToolStripTextBox toolStripTextBox && !string.IsNullOrWhiteSpace(toolStripTextBox.Text))
				{
					return false;
				}
			}
			return true;
		}
	}

	public override event EventHandler UIChanged;

	public DomNodeSearchToolStrip()
	{
		m_rootNode = new DomNodeQueryRoot();
		m_rootNode.AddLabel("Find node(s)");
		QueryOption parentNode = m_rootNode.AddOption();
		parentNode.AddOptionItem("whose name", 0uL).AddDomNodeNameQuery(isReplacePattern: true);
		parentNode.AddOptionItem("with a parameter", 0uL).AddDomNodePropertyQuery();
		m_rootNode.AddSeparator();
		m_rootNode.RegisterSearchButtonPress(m_rootNode.AddButton("Search"));
		m_rootNode.SearchTextEntered += searchSubStrip_SearchTextEntered;
		m_rootNode.OptionChanged += searchSubStrip_OptionsChanged;
		SuspendLayout();
		List<ToolStripItem> list = new List<ToolStripItem>();
		m_rootNode.GetToolStripItems(list);
		Items.AddRange(list.ToArray());
		base.Location = new Point(0, 0);
		base.Name = "Event Sequence Document Search";
		base.Size = new Size(292, 25);
		base.TabIndex = 0;
		Text = "Event Sequence Document Search";
		base.GripStyle = ToolStripGripStyle.Hidden;
		ResumeLayout(performLayout: false);
	}

	private void searchSubStrip_SearchTextEntered(object sender, EventArgs e)
	{
		DoSearch();
	}

	private void searchSubStrip_OptionsChanged(object sender, EventArgs e)
	{
		if (sender is QueryOption)
		{
			Items.Clear();
			List<ToolStripItem> list = new List<ToolStripItem>();
			m_rootNode.GetToolStripItems(list);
			Items.AddRange(list.ToArray());
			UIChanged.Raise(sender, EventArgs.Empty);
		}
	}

	public override IQueryPredicate GetPredicate()
	{
		return m_rootNode.GetPredicate();
	}
}
