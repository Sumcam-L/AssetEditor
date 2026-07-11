using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetCloudFramework;

public class DeleteForm : Form
{
	private IContainer components = null;

	private Button btnOK;

	private Button btnCancel;

	private ListView lvFilesToDelete;

	private ColumnHeader NameColumn;

	private ColumnHeader TypeColumn;

	private CheckBox checkBox1;

	private Label label1;

	private Label label3;

	private ListView lvReferringFilesToDelete;

	private ColumnHeader columnHeader3;

	private ColumnHeader columnHeader4;

	public IEnumerable<IInstanceEntity> SubmittedFiles => from ListViewItem lvi in lvFilesToDelete.Items
		where lvi.Checked
		select (IInstanceEntity)lvi.Tag;

	public IEnumerable<IInstanceEntity> RevertedFiles => from ListViewItem lvi in lvFilesToDelete.Items
		where !lvi.Checked
		select (IInstanceEntity)lvi.Tag;

	public DeleteForm()
	{
		InitializeComponent();
	}

	public DeleteForm(IEnumerable<IInstanceEntity> entitiesToDelete, IEnumerable<EntityFileInfo> referrersThatWillBreak, IEnumerable<EntityFileInfo> deleted)
		: this()
	{
		AddEntityItems(entitiesToDelete, deleted);
		AddReferrerItems(referrersThatWillBreak);
	}

	private void AddEntityItems(IEnumerable<IInstanceEntity> items, IEnumerable<EntityFileInfo> deleted)
	{
		foreach (IInstanceEntity item in from ent in items
			orderby ent.Name
			orderby ent.Type
			select ent)
		{
			ListViewItem listViewItem = lvFilesToDelete.Items.Add(new ListViewItem(new string[2]
			{
				item.Name,
				StaticMethods.ExtensionForInstanceType(item.Type)
			}));
			listViewItem.Tag = item;
			listViewItem.Checked = deleted.Contains(new EntityFileInfo(item.Type, item.Name));
		}
	}

	private void AddReferrerItems(IEnumerable<EntityFileInfo> items)
	{
		foreach (EntityFileInfo item in from item in items
			orderby item.name
			orderby item.instanceType
			select item)
		{
			lvReferringFilesToDelete.Items.Add(new ListViewItem(new string[2]
			{
				item.name,
				StaticMethods.ExtensionForInstanceType(item.instanceType)
			}));
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
		this.btnOK = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.lvFilesToDelete = new System.Windows.Forms.ListView();
		this.NameColumn = new System.Windows.Forms.ColumnHeader();
		this.TypeColumn = new System.Windows.Forms.ColumnHeader();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.lvReferringFilesToDelete = new System.Windows.Forms.ListView();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Location = new System.Drawing.Point(344, 454);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 1;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(425, 454);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.lvFilesToDelete.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lvFilesToDelete.CheckBoxes = true;
		this.lvFilesToDelete.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.NameColumn, this.TypeColumn });
		this.lvFilesToDelete.Location = new System.Drawing.Point(11, 25);
		this.lvFilesToDelete.MultiSelect = false;
		this.lvFilesToDelete.Name = "lvFilesToDelete";
		this.lvFilesToDelete.Size = new System.Drawing.Size(489, 185);
		this.lvFilesToDelete.TabIndex = 0;
		this.lvFilesToDelete.UseCompatibleStateImageBehavior = false;
		this.lvFilesToDelete.View = System.Windows.Forms.View.Details;
		this.NameColumn.Text = "Name";
		this.NameColumn.Width = 425;
		this.TypeColumn.Text = "Type";
		this.TypeColumn.Width = 50;
		this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBox1.AutoSize = true;
		this.checkBox1.Checked = true;
		this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox1.Location = new System.Drawing.Point(18, 458);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(71, 17);
		this.checkBox1.TabIndex = 7;
		this.checkBox1.Text = "Check All";
		this.checkBox1.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(8, 7);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(117, 13);
		this.label1.TabIndex = 8;
		this.label1.Text = "Selected files to delete:";
		this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(11, 217);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(203, 13);
		this.label3.TabIndex = 12;
		this.label3.Text = "Affected entities that will need to be fixed:";
		this.lvReferringFilesToDelete.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lvReferringFilesToDelete.Columns.AddRange(new System.Windows.Forms.ColumnHeader[2] { this.columnHeader3, this.columnHeader4 });
		this.lvReferringFilesToDelete.Location = new System.Drawing.Point(11, 234);
		this.lvReferringFilesToDelete.MultiSelect = false;
		this.lvReferringFilesToDelete.Name = "lvReferringFilesToDelete";
		this.lvReferringFilesToDelete.Size = new System.Drawing.Size(491, 203);
		this.lvReferringFilesToDelete.TabIndex = 11;
		this.lvReferringFilesToDelete.UseCompatibleStateImageBehavior = false;
		this.lvReferringFilesToDelete.View = System.Windows.Forms.View.Details;
		this.columnHeader3.Text = "Name";
		this.columnHeader3.Width = 425;
		this.columnHeader4.Text = "Type";
		this.columnHeader4.Width = 50;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(514, 489);
		base.Controls.Add(this.lvReferringFilesToDelete);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.checkBox1);
		base.Controls.Add(this.lvFilesToDelete);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnOK);
		this.MinimumSize = new System.Drawing.Size(530, 245);
		base.Name = "DeleteForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Asset Cloud - Delete";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
