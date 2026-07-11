using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class DomNodeReplaceToolStrip : ReplaceToolStrip
{
	private readonly DomNodeQueryRoot m_rootNode;

	private readonly QueryTextInput m_replaceTextInput;

	internal DomNodeSearchToolStrip DomNodeSearchToolStrip { get; set; }

	public override event EventHandler UIChanged;

	public DomNodeReplaceToolStrip()
	{
		m_rootNode = new DomNodeQueryRoot();
		m_rootNode.AddLabel("Replace with");
		m_replaceTextInput = m_rootNode.AddReplaceTextInput(null, isNumericalText: false);
		m_rootNode.AddSeparator();
		m_rootNode.RegisterReplaceButtonPress(m_rootNode.AddButton("Replace"));
		m_rootNode.ReplaceTextEntered += replaceSubStrip_ReplaceTextEntered;
		SuspendLayout();
		List<ToolStripItem> list = new List<ToolStripItem>();
		m_rootNode.GetToolStripItems(list);
		Items.AddRange(list.ToArray());
		base.Location = new Point(0, 0);
		base.Name = "Event Sequence Document Replace";
		base.Size = new Size(292, 25);
		base.TabIndex = 0;
		Text = "Event Sequence Document Replace";
		base.GripStyle = ToolStripGripStyle.Hidden;
		ResumeLayout(performLayout: false);
		if (UIChanged == null)
		{
		}
	}

	private void replaceSubStrip_ReplaceTextEntered(object sender, EventArgs e)
	{
		DoReplace();
	}

	public override object GetReplaceInfo()
	{
		return m_replaceTextInput.InputText;
	}

	public override void DoReplace()
	{
		if (DomNodeSearchToolStrip == null || !DomNodeSearchToolStrip.QueryWithEmptyFields)
		{
			if (DomNodeSearchToolStrip != null && DomNodeSearchToolStrip.QueryDirty)
			{
				DomNodeSearchToolStrip.DoSearch();
			}
			base.DoReplace();
		}
	}
}
