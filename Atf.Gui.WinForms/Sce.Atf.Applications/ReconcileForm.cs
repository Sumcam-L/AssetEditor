using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class ReconcileForm : Form
{
	private readonly ISourceControlService m_sourceControlService;

	private readonly List<Uri> m_modified;

	private readonly List<Uri> m_notInDepot;

	private IContainer components = null;

	private Label label1;

	private CheckedListBox localModifiedListBox;

	private Label label2;

	private CheckedListBox localfilesNotInDepotListBox;

	private Button reconcileBtn;

	private Button cancelBtn;

	public ReconcileForm(ISourceControlService sourceControlService, IEnumerable<Uri> modified, IEnumerable<Uri> notInDepot)
	{
		InitializeComponent();
		m_sourceControlService = sourceControlService;
		m_modified = new List<Uri>(modified);
		m_notInDepot = new List<Uri>(notInDepot);
		foreach (Uri item in modified)
		{
			localModifiedListBox.Items.Add(item.LocalPath, isChecked: true);
		}
		foreach (Uri item2 in notInDepot)
		{
			localfilesNotInDepotListBox.Items.Add(item2.LocalPath, isChecked: true);
		}
	}

	private void cancelBtn_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void reconcileBtn_Click(object sender, EventArgs e)
	{
		for (int i = 0; i < localModifiedListBox.Items.Count; i++)
		{
			CheckState itemCheckState = localModifiedListBox.GetItemCheckState(i);
			if (itemCheckState == CheckState.Checked)
			{
				m_sourceControlService.CheckOut(m_modified[i]);
			}
		}
		for (int j = 0; j < localfilesNotInDepotListBox.Items.Count; j++)
		{
			CheckState itemCheckState2 = localfilesNotInDepotListBox.GetItemCheckState(j);
			if (itemCheckState2 == CheckState.Checked)
			{
				m_sourceControlService.Add(m_notInDepot[j]);
			}
		}
		Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.ReconcileForm));
		this.label1 = new System.Windows.Forms.Label();
		this.localModifiedListBox = new System.Windows.Forms.CheckedListBox();
		this.label2 = new System.Windows.Forms.Label();
		this.localfilesNotInDepotListBox = new System.Windows.Forms.CheckedListBox();
		this.reconcileBtn = new System.Windows.Forms.Button();
		this.cancelBtn = new System.Windows.Forms.Button();
		base.SuspendLayout();
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.localModifiedListBox, "localModifiedListBox");
		this.localModifiedListBox.CheckOnClick = true;
		this.localModifiedListBox.FormattingEnabled = true;
		this.localModifiedListBox.Name = "localModifiedListBox";
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.localfilesNotInDepotListBox, "localfilesNotInDepotListBox");
		this.localfilesNotInDepotListBox.FormattingEnabled = true;
		this.localfilesNotInDepotListBox.Name = "localfilesNotInDepotListBox";
		this.reconcileBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
		resources.ApplyResources(this.reconcileBtn, "reconcileBtn");
		this.reconcileBtn.Name = "reconcileBtn";
		this.reconcileBtn.UseVisualStyleBackColor = true;
		this.reconcileBtn.Click += new System.EventHandler(reconcileBtn_Click);
		this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		resources.ApplyResources(this.cancelBtn, "cancelBtn");
		this.cancelBtn.Name = "cancelBtn";
		this.cancelBtn.UseVisualStyleBackColor = true;
		this.cancelBtn.Click += new System.EventHandler(cancelBtn_Click);
		base.AcceptButton = this.reconcileBtn;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.cancelBtn);
		base.Controls.Add(this.reconcileBtn);
		base.Controls.Add(this.localfilesNotInDepotListBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.localModifiedListBox);
		base.Controls.Add(this.label1);
		base.Name = "ReconcileForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
