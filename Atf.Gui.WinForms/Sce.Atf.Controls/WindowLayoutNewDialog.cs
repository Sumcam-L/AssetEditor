using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public class WindowLayoutNewDialog : Form
{
	private readonly List<string> m_existingItems = new List<string>();

	private readonly BalloonToolTipHelper m_toolTip = new BalloonToolTipHelper();

	private IContainer components = null;

	private GroupBox m_grpLayout;

	private Button m_btnCancel;

	private Button m_btnOk;

	private TextBox m_txtLayout;

	public string LayoutName
	{
		get
		{
			return m_txtLayout.Text.Trim();
		}
		set
		{
			m_txtLayout.Text = value;
		}
	}

	public IEnumerable<string> ExistingLayoutNames
	{
		set
		{
			m_existingItems.Clear();
			m_existingItems.AddRange(value);
		}
	}

	public WindowLayoutNewDialog()
	{
		InitializeComponent();
	}

	private void BtnOkClick(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.None;
		string layoutName = LayoutName;
		if (string.IsNullOrEmpty(layoutName))
		{
			m_toolTip.Show(m_txtLayout, null, ToolTipIcon.Warning, "Layout name cannot be empty!");
		}
		else if (!WindowLayoutService.IsValidLayoutName(layoutName))
		{
			m_toolTip.Show(m_txtLayout, null, ToolTipIcon.Warning, "Invalid layout name!");
		}
		else if (m_existingItems.Contains(layoutName))
		{
			m_toolTip.Show(m_txtLayout, null, ToolTipIcon.Warning, "A layout with this name already exists!");
		}
		else
		{
			base.DialogResult = DialogResult.OK;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.WindowLayoutNewDialog));
		this.m_grpLayout = new System.Windows.Forms.GroupBox();
		this.m_btnCancel = new System.Windows.Forms.Button();
		this.m_btnOk = new System.Windows.Forms.Button();
		this.m_txtLayout = new System.Windows.Forms.TextBox();
		this.m_grpLayout.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.m_grpLayout, "m_grpLayout");
		this.m_grpLayout.Controls.Add(this.m_btnCancel);
		this.m_grpLayout.Controls.Add(this.m_btnOk);
		this.m_grpLayout.Controls.Add(this.m_txtLayout);
		this.m_grpLayout.Name = "m_grpLayout";
		this.m_grpLayout.TabStop = false;
		resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.m_btnOk, "m_btnOk");
		this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnOk.Name = "m_btnOk";
		this.m_btnOk.UseVisualStyleBackColor = true;
		this.m_btnOk.Click += new System.EventHandler(BtnOkClick);
		resources.ApplyResources(this.m_txtLayout, "m_txtLayout");
		this.m_txtLayout.Name = "m_txtLayout";
		base.AcceptButton = this.m_btnOk;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_btnCancel;
		base.Controls.Add(this.m_grpLayout);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "LayoutNewDialog";
		this.m_grpLayout.ResumeLayout(false);
		this.m_grpLayout.PerformLayout();
		base.ResumeLayout(false);
	}
}
