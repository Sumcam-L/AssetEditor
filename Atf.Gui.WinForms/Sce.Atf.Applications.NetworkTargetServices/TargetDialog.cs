using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Applications.NetworkTargetServices;

public class TargetDialog : Form
{
	private readonly bool m_canEditPortNumber = true;

	private readonly bool m_singleSelectionMode = true;

	private readonly int m_defaultPortNumber = -1;

	private bool m_Changed;

	private bool m_initing = true;

	private readonly Dictionary<string, Target> m_originalTargets;

	private readonly string[] m_protocols;

	private IContainer components = null;

	private GroupBox grpTargets;

	private Button m_btnDelete;

	private Button m_btnOK;

	private Button m_btnEdit;

	private Button m_btnAdd;

	private Button m_btnCancel;

	private ListView m_lstTargets;

	private ColumnHeader Targets;

	public TargetDialog(Dictionary<string, Target> targets, bool singleSelectionMode, int defaultPortNumber, bool canEditPortNumber, string[] protocols)
	{
		if (targets == null)
		{
			throw new ArgumentNullException();
		}
		m_protocols = protocols;
		m_originalTargets = targets;
		m_canEditPortNumber = canEditPortNumber;
		m_defaultPortNumber = defaultPortNumber;
		m_singleSelectionMode = singleSelectionMode;
		m_protocols = protocols;
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterParent;
		m_lstTargets.SuspendLayout();
		m_lstTargets.BeginUpdate();
		foreach (Target value2 in targets.Values)
		{
			Target target = (Target)value2.Clone();
			ListViewItem value = CreateListViewItem(target);
			m_lstTargets.Items.Add(value);
		}
		if (m_lstTargets.Items.Count > 0)
		{
			m_lstTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}
		else
		{
			m_lstTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}
		m_lstTargets.EndUpdate();
		m_lstTargets.ResumeLayout();
		m_initing = false;
	}

	private void m_btnOK_Click(object sender, EventArgs e)
	{
		if (m_Changed)
		{
			m_originalTargets.Clear();
			foreach (ListViewItem item in m_lstTargets.Items)
			{
				Target target = (Target)item.Tag;
				m_originalTargets.Add(target.Name, target);
			}
		}
		base.DialogResult = DialogResult.OK;
	}

	private void m_btnAdd_Click(object sender, EventArgs e)
	{
		TargetEditDialog targetEditDialog = new TargetEditDialog(m_defaultPortNumber, m_canEditPortNumber, m_protocols);
		while (targetEditDialog.ShowDialog(this) == DialogResult.OK)
		{
			Target target = targetEditDialog.GetTarget();
			if (m_lstTargets.Items.ContainsKey(target.Name))
			{
				MessageBox.Show(this, "Target name already exist".Localize());
				continue;
			}
			m_lstTargets.Items.Add(CreateListViewItem(target));
			m_Changed = true;
			break;
		}
	}

	private void m_btnEdit_Click(object sender, EventArgs e)
	{
		ListView.SelectedListViewItemCollection selectedItems = m_lstTargets.SelectedItems;
		Target target = ((selectedItems.Count > 0) ? ((Target)selectedItems[0].Tag) : null);
		if (target == null)
		{
			MessageBox.Show(this, "Target not selected".Localize());
			return;
		}
		TargetEditDialog targetEditDialog = new TargetEditDialog(target, m_defaultPortNumber, m_canEditPortNumber, m_protocols);
		while (targetEditDialog.ShowDialog(this) == DialogResult.OK && targetEditDialog.Changed)
		{
			if (target.Name != selectedItems[0].Name)
			{
				if (m_lstTargets.Items.ContainsKey(target.Name))
				{
					MessageBox.Show(this, "Target name already exist".Localize());
					continue;
				}
				selectedItems[0].Name = target.Name;
			}
			selectedItems[0].Text = target.ToString();
			m_lstTargets.Update();
			m_Changed = true;
			break;
		}
	}

	private void m_lstTargets_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		if (m_initing)
		{
			return;
		}
		m_initing = true;
		((Target)e.Item.Tag).Selected = e.Item.Checked;
		e.Item.Selected = true;
		if (m_singleSelectionMode)
		{
			foreach (ListViewItem item in m_lstTargets.Items)
			{
				if (item != e.Item)
				{
					item.Checked = false;
					((Target)item.Tag).Selected = false;
				}
			}
		}
		m_initing = false;
		m_Changed = true;
	}

	private void m_btnDelete_Click(object sender, EventArgs e)
	{
		ListView.SelectedListViewItemCollection selectedItems = m_lstTargets.SelectedItems;
		Target target = ((selectedItems.Count > 0) ? ((Target)selectedItems[0].Tag) : null);
		if (target == null)
		{
			MessageBox.Show(this, "Target not selected".Localize());
		}
		else if (MessageBox.Show(this, string.Format("Delete target {0}".Localize(), target), "", MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			m_lstTargets.Items.Remove(selectedItems[0]);
			m_Changed = true;
		}
	}

	private ListViewItem CreateListViewItem(Target target)
	{
		ListViewItem listViewItem = new ListViewItem(target.ToString());
		listViewItem.Name = target.Name;
		listViewItem.Tag = target;
		listViewItem.Checked = target.Selected;
		return listViewItem;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.NetworkTargetServices.TargetDialog));
		this.Targets = new System.Windows.Forms.ColumnHeader();
		this.grpTargets = new System.Windows.Forms.GroupBox();
		this.m_lstTargets = new System.Windows.Forms.ListView();
		this.m_btnDelete = new System.Windows.Forms.Button();
		this.m_btnOK = new System.Windows.Forms.Button();
		this.m_btnEdit = new System.Windows.Forms.Button();
		this.m_btnAdd = new System.Windows.Forms.Button();
		this.m_btnCancel = new System.Windows.Forms.Button();
		this.grpTargets.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.grpTargets, "grpTargets");
		this.grpTargets.Controls.Add(this.m_lstTargets);
		this.grpTargets.Name = "grpTargets";
		this.grpTargets.TabStop = false;
		this.m_lstTargets.CheckBoxes = true;
		this.m_lstTargets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.Targets });
		resources.ApplyResources(this.m_lstTargets, "m_lstTargets");
		this.m_lstTargets.FullRowSelect = true;
		this.m_lstTargets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
		this.m_lstTargets.HideSelection = false;
		this.m_lstTargets.MultiSelect = false;
		this.m_lstTargets.Name = "m_lstTargets";
		this.m_lstTargets.UseCompatibleStateImageBehavior = false;
		this.m_lstTargets.View = System.Windows.Forms.View.Details;
		this.m_lstTargets.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(m_lstTargets_ItemChecked);
		resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
		this.m_btnDelete.Name = "m_btnDelete";
		this.m_btnDelete.UseVisualStyleBackColor = true;
		this.m_btnDelete.Click += new System.EventHandler(m_btnDelete_Click);
		resources.ApplyResources(this.m_btnOK, "m_btnOK");
		this.m_btnOK.Name = "m_btnOK";
		this.m_btnOK.UseVisualStyleBackColor = true;
		this.m_btnOK.Click += new System.EventHandler(m_btnOK_Click);
		resources.ApplyResources(this.m_btnEdit, "m_btnEdit");
		this.m_btnEdit.Name = "m_btnEdit";
		this.m_btnEdit.UseVisualStyleBackColor = true;
		this.m_btnEdit.Click += new System.EventHandler(m_btnEdit_Click);
		resources.ApplyResources(this.m_btnAdd, "m_btnAdd");
		this.m_btnAdd.Name = "m_btnAdd";
		this.m_btnAdd.UseVisualStyleBackColor = true;
		this.m_btnAdd.Click += new System.EventHandler(m_btnAdd_Click);
		resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
		this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_btnCancel.Name = "m_btnCancel";
		this.m_btnCancel.UseVisualStyleBackColor = true;
		resources.ApplyResources(this, "$this");
		base.Controls.Add(this.m_btnCancel);
		base.Controls.Add(this.m_btnOK);
		base.Controls.Add(this.m_btnAdd);
		base.Controls.Add(this.m_btnEdit);
		base.Controls.Add(this.m_btnDelete);
		base.Controls.Add(this.grpTargets);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "TargetDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		this.grpTargets.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
