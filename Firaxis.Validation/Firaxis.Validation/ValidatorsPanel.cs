using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Reflection;
using Firaxis.Utility;
using Firaxis.Validation.Properties;

namespace Firaxis.Validation;

public class ValidatorsPanel : UserControl
{
	private ValidatorProvider provider;

	private IContainer components = null;

	private ToolStrip toolStrip1;

	private ListView listView;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private ToolStripButton buttonSelectAll;

	private ToolStripButton buttonSelectNone;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ValidatorProvider ValidatorProvider
	{
		get
		{
			return provider;
		}
		set
		{
			if (provider != null)
			{
				provider.ValidatorsChanged -= Validators_RebuildList;
			}
			provider = value;
			if (provider != null)
			{
				provider.ValidatorsChanged += Validators_RebuildList;
			}
			RebuildList();
		}
	}

	public ValidatorsPanel()
	{
		InitializeComponent();
		if (Context.TryGet<ValidatorProvider>(out var service))
		{
			ValidatorProvider = service;
		}
		listView.ItemChecked += listView_ItemChecked;
	}

	private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		IValidator validator = (IValidator)e.Item.Tag;
		if (validator != null)
		{
			validator.Enabled = e.Item.Checked;
		}
	}

	private void Validators_RebuildList(object sender, EventArgs e)
	{
		RebuildList();
	}

	private void RebuildList()
	{
		listView.Items.Clear();
		if (provider == null)
		{
			return;
		}
		foreach (IValidator validator in provider.Validators)
		{
			ListViewItem listViewItem = listView.Items.Add(ReflectionHelper.GetDisplayName(validator));
			listViewItem.SubItems.Add(ReflectionHelper.GetDescription(validator));
			listViewItem.Checked = validator.Enabled;
			listViewItem.Tag = validator;
		}
	}

	private void CheckAllItems(bool asChecked)
	{
		listView.BeginUpdate();
		foreach (ListViewItem item in listView.Items)
		{
			item.Checked = asChecked;
		}
		listView.EndUpdate();
	}

	private void buttonSelectAll_Click(object sender, EventArgs e)
	{
		CheckAllItems(asChecked: true);
	}

	private void buttonSelectNone_Click(object sender, EventArgs e)
	{
		CheckAllItems(asChecked: false);
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
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.buttonSelectAll = new System.Windows.Forms.ToolStripButton();
		this.buttonSelectNone = new System.Windows.Forms.ToolStripButton();
		this.listView = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.buttonSelectAll, this.buttonSelectNone });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
		this.toolStrip1.Size = new System.Drawing.Size(438, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip1";
		this.buttonSelectAll.Image = Firaxis.Validation.Properties.Resources.Control_Checkbox;
		this.buttonSelectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectAll.Name = "buttonSelectAll";
		this.buttonSelectAll.Size = new System.Drawing.Size(79, 22);
		this.buttonSelectAll.Text = "Enable All";
		this.buttonSelectAll.ToolTipText = "Enable All Tests";
		this.buttonSelectAll.Click += new System.EventHandler(buttonSelectAll_Click);
		this.buttonSelectNone.Image = Firaxis.Validation.Properties.Resources.Control_Uncheckbox;
		this.buttonSelectNone.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonSelectNone.Name = "buttonSelectNone";
		this.buttonSelectNone.Size = new System.Drawing.Size(82, 22);
		this.buttonSelectNone.Text = "Disable All";
		this.buttonSelectNone.ToolTipText = "Disable All Tests";
		this.buttonSelectNone.Click += new System.EventHandler(buttonSelectNone_Click);
		this.listView.CheckBoxes = true;
		this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.columnHeader1, this.columnHeader2 });
		this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.listView.FullRowSelect = true;
		this.listView.Location = new System.Drawing.Point(0, 25);
		this.listView.Name = "listView";
		this.listView.ShowItemToolTips = true;
		this.listView.Size = new System.Drawing.Size(438, 198);
		this.listView.TabIndex = 1;
		this.listView.UseCompatibleStateImageBehavior = false;
		this.listView.View = System.Windows.Forms.View.Details;
		this.columnHeader1.Text = "Tests";
		this.columnHeader1.Width = 120;
		this.columnHeader2.Text = "Description";
		this.columnHeader2.Width = 250;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.listView);
		base.Controls.Add(this.toolStrip1);
		base.Name = "ValidatorsPanel";
		base.Size = new System.Drawing.Size(438, 223);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
