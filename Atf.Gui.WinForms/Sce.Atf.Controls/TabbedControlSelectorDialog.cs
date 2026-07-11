using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public class TabbedControlSelectorDialog : Form
{
	private enum eListBoxType
	{
		FocusedPaneListBox,
		UnfocusedPaneListBox
	}

	private readonly IControlHostService m_controlHostService;

	private readonly List<ControlInfo> m_focusedPaneControlList;

	private readonly List<ControlInfo> m_unfocusedPaneControlList;

	private ListBox m_activeListBox;

	private List<ControlInfo> m_activeList;

	private IContainer components = null;

	private StatusStrip statusStrip1;

	private ToolStripStatusLabel toolStripStatusLabel;

	private ListBox unfocusedPaneListBox;

	private ListBox focusedPaneListBox;

	private int SelectedControlIndex => m_activeListBox.SelectedIndices[0];

	public TabbedControlSelectorDialog(IControlHostService controlHostService, bool incrementForward)
	{
		m_controlHostService = controlHostService;
		m_focusedPaneControlList = new List<ControlInfo>();
		m_unfocusedPaneControlList = new List<ControlInfo>();
		foreach (ControlInfo control in m_controlHostService.Controls)
		{
			if (control.InActiveGroup)
			{
				m_focusedPaneControlList.Add(control);
			}
			else
			{
				m_unfocusedPaneControlList.Add(control);
			}
		}
		m_focusedPaneControlList.Reverse();
		m_unfocusedPaneControlList.Reverse();
		InitializeComponent();
		focusedPaneListBox.DataSource = m_focusedPaneControlList;
		focusedPaneListBox.DisplayMember = "Name";
		if (m_focusedPaneControlList.Count > 0)
		{
			focusedPaneListBox.SetSelected(0, value: true);
		}
		unfocusedPaneListBox.DataSource = m_unfocusedPaneControlList;
		unfocusedPaneListBox.DisplayMember = "Name";
		SetActiveListBox(eListBoxType.FocusedPaneListBox);
		IncrementSelection(incrementForward);
	}

	private void SetActiveListBox(eListBoxType listBox)
	{
		switch (listBox)
		{
		case eListBoxType.FocusedPaneListBox:
			m_activeListBox = focusedPaneListBox;
			m_activeList = m_focusedPaneControlList;
			unfocusedPaneListBox.Enabled = false;
			break;
		case eListBoxType.UnfocusedPaneListBox:
			m_activeListBox = unfocusedPaneListBox;
			m_activeList = m_unfocusedPaneControlList;
			focusedPaneListBox.Enabled = false;
			break;
		default:
			throw new ApplicationException("SetActiveListBox() - unhandled list box type specified");
		}
		m_activeListBox.Enabled = true;
		UpdateToolStripStatus();
	}

	private void TabbedControlSelectorDialog_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Control)
		{
			e.Handled = true;
			switch (e.KeyCode)
			{
			case Keys.Tab:
				IncrementSelection(!e.Shift);
				break;
			case Keys.Up:
				IncrementSelection(forward: false);
				break;
			case Keys.Down:
				IncrementSelection(forward: true);
				break;
			case Keys.Left:
			case Keys.Right:
				SetActiveListBox((m_activeListBox == focusedPaneListBox) ? eListBoxType.UnfocusedPaneListBox : eListBoxType.FocusedPaneListBox);
				break;
			default:
				e.Handled = false;
				break;
			}
		}
	}

	private void IncrementSelection(bool forward)
	{
		if (m_activeListBox.SelectedIndices.Count > 0)
		{
			int num = m_activeListBox.SelectedIndices[0];
			num = ((!forward) ? ((num > 0) ? (num - 1) : (m_activeListBox.Items.Count - 1)) : ((num + 1) % m_activeListBox.Items.Count));
			if (num != m_activeListBox.SelectedIndices[0])
			{
				m_activeListBox.SetSelected(num, value: true);
				UpdateToolStripStatus();
			}
		}
	}

	private void UpdateToolStripStatus()
	{
		toolStripStatusLabel.Text = ((m_activeList.Count > 0) ? m_activeList[SelectedControlIndex].Description : "");
	}

	private void SelectionMade()
	{
		if (m_activeList.Count > 0)
		{
			m_controlHostService.Show(m_activeList[SelectedControlIndex].Control);
		}
		Close();
	}

	private void TabbedControlSelectorDialog_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			SelectionMade();
		}
	}

	private void focusedPaneListBox_SelectionChanged(object sender, EventArgs e)
	{
		SetActiveListBox(eListBoxType.FocusedPaneListBox);
		if (focusedPaneListBox.Focused)
		{
			SelectionMade();
		}
	}

	private void unfocusedPaneListBox_SelectionChanged(object sender, EventArgs e)
	{
		SetActiveListBox(eListBoxType.UnfocusedPaneListBox);
		if (unfocusedPaneListBox.Focused)
		{
			SelectionMade();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.TabbedControlSelectorDialog));
		this.statusStrip1 = new System.Windows.Forms.StatusStrip();
		this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
		this.unfocusedPaneListBox = new System.Windows.Forms.ListBox();
		this.focusedPaneListBox = new System.Windows.Forms.ListBox();
		System.Windows.Forms.Label label = new System.Windows.Forms.Label();
		System.Windows.Forms.Label label2 = new System.Windows.Forms.Label();
		this.statusStrip1.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(label, "label1");
		label.Name = "label1";
		resources.ApplyResources(label2, "label2");
		label2.Name = "label2";
		this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.toolStripStatusLabel });
		resources.ApplyResources(this.statusStrip1, "statusStrip1");
		this.statusStrip1.Name = "statusStrip1";
		this.statusStrip1.SizingGrip = false;
		this.toolStripStatusLabel.Name = "toolStripStatusLabel";
		resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
		this.unfocusedPaneListBox.BackColor = System.Drawing.SystemColors.Window;
		this.unfocusedPaneListBox.FormattingEnabled = true;
		resources.ApplyResources(this.unfocusedPaneListBox, "unfocusedPaneListBox");
		this.unfocusedPaneListBox.Name = "unfocusedPaneListBox";
		this.unfocusedPaneListBox.TabStop = false;
		this.unfocusedPaneListBox.UseTabStops = false;
		this.unfocusedPaneListBox.SelectedIndexChanged += new System.EventHandler(unfocusedPaneListBox_SelectionChanged);
		this.unfocusedPaneListBox.SelectedValueChanged += new System.EventHandler(unfocusedPaneListBox_SelectionChanged);
		this.focusedPaneListBox.BackColor = System.Drawing.SystemColors.Window;
		this.focusedPaneListBox.FormattingEnabled = true;
		resources.ApplyResources(this.focusedPaneListBox, "focusedPaneListBox");
		this.focusedPaneListBox.Name = "focusedPaneListBox";
		this.focusedPaneListBox.TabStop = false;
		this.focusedPaneListBox.UseTabStops = false;
		this.focusedPaneListBox.SelectedIndexChanged += new System.EventHandler(focusedPaneListBox_SelectionChanged);
		this.focusedPaneListBox.SelectedValueChanged += new System.EventHandler(focusedPaneListBox_SelectionChanged);
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Control;
		base.ControlBox = false;
		base.Controls.Add(label2);
		base.Controls.Add(label);
		base.Controls.Add(this.statusStrip1);
		base.Controls.Add(this.focusedPaneListBox);
		base.Controls.Add(this.unfocusedPaneListBox);
		this.DoubleBuffered = true;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "TabbedControlSelectorDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.TopMost = true;
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(TabbedControlSelectorDialog_KeyDown);
		base.KeyUp += new System.Windows.Forms.KeyEventHandler(TabbedControlSelectorDialog_KeyUp);
		this.statusStrip1.ResumeLayout(false);
		this.statusStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
