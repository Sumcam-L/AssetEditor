using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Firaxis.ATF;

public class HotloadTargetsControl : UserControl
{
	private ICookableRegistry m_cookableRegistry;

	private IContainer components;

	private ListView lvAutoHotload;

	private ColumnHeader columnHeader1;

	public HotloadTargetsControl(ICookableRegistry cookableReg)
	{
		m_cookableRegistry = cookableReg;
		InitializeComponent();
		lvAutoHotload.ItemChecked += Cookables_ItemChecked;
	}

	public void PopulateItems()
	{
		lvAutoHotload.Items.Clear();
		foreach (Uri cookable in m_cookableRegistry.Cookables)
		{
			ListViewItem listViewItem = new ListViewItem(Path.GetFileName(cookable.LocalPath));
			listViewItem.Tag = cookable;
			listViewItem.Checked = m_cookableRegistry.IsCookingEnabled(cookable);
			lvAutoHotload.Items.Add(listViewItem);
		}
		lvAutoHotload.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
	}

	private void Cookables_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		m_cookableRegistry.EnableCooking((Uri)e.Item.Tag, e.Item.Checked);
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
		this.lvAutoHotload = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.lvAutoHotload.CheckBoxes = true;
		this.lvAutoHotload.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader1 });
		this.lvAutoHotload.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lvAutoHotload.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
		this.lvAutoHotload.Location = new System.Drawing.Point(0, 0);
		this.lvAutoHotload.Name = "lvAutoHotload";
		this.lvAutoHotload.Size = new System.Drawing.Size(150, 150);
		this.lvAutoHotload.TabIndex = 0;
		this.lvAutoHotload.UseCompatibleStateImageBehavior = false;
		this.lvAutoHotload.View = System.Windows.Forms.View.Details;
		this.columnHeader1.Text = "Hotload Targets";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.lvAutoHotload);
		base.Name = "AutoHotloadControl";
		base.ResumeLayout(false);
	}
}
