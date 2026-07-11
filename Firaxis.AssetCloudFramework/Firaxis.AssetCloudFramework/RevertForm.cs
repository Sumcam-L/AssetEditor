using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetCloudFramework.Data;
using Firaxis.Collections;

namespace Firaxis.AssetCloudFramework;

public class RevertForm : Form
{
	private IContainer components = null;

	private Button btnOK;

	private Button btnCancel;

	private ListView lvFilesToRevert;

	private ColumnHeader NameColumn;

	private ColumnHeader PendingActionColumn;

	private Label lblFilesToRevert;

	private ColumnHeader TypeColumn;

	private CheckBox checkBox1;

	public SourceControlServicePathRequest FilesToRevert { get; private set; }

	public RevertForm()
	{
		InitializeComponent();
		FilesToRevert = new SourceControlServicePathRequest(new List<string>());
	}

	public RevertForm(SourceControlServiceFileResult addResponse, SourceControlServiceFileResult editResponse, SourceControlServiceFileResult deleteResponse)
		: this()
	{
		AddListViewItems(addResponse, "add");
		AddListViewItems(editResponse, "edit");
		AddListViewItems(deleteResponse, "delete");
	}

	private void AddListViewItems(SourceControlServiceFileResult response, string sPendingAction)
	{
		Func<string, string, string, ListViewItem> func = (string name, string type, string filePath) => new ListViewItem(new string[3] { name, type, sPendingAction })
		{
			Tag = filePath,
			Checked = true
		};
		response.Files.OrderBy((string a) => a).ForEach(delegate(string a)
		{
			string text = a.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(a);
			string extension = Path.GetExtension(a);
			FilesToRevert.FullPaths.Add(text);
			lvFilesToRevert.Items.Add(func(fileNameWithoutExtension, extension, text));
		});
	}

	private void lvFilesToRevert_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		int count = lvFilesToRevert.CheckedItems.Count;
		if (count <= 0)
		{
			if (btnOK.Enabled)
			{
				btnOK.Enabled = false;
			}
		}
		else if (!btnOK.Enabled)
		{
			btnOK.Enabled = true;
		}
		string item = (string)e.Item.Tag;
		if (e.Item.Checked)
		{
			FilesToRevert.FullPaths.Add(item);
		}
		else
		{
			FilesToRevert.FullPaths.Remove(item);
		}
	}

	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
		foreach (ListViewItem item in lvFilesToRevert.Items)
		{
			item.Checked = checkBox1.Checked;
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
		this.lvFilesToRevert = new System.Windows.Forms.ListView();
		this.NameColumn = new System.Windows.Forms.ColumnHeader();
		this.TypeColumn = new System.Windows.Forms.ColumnHeader();
		this.PendingActionColumn = new System.Windows.Forms.ColumnHeader();
		this.lblFilesToRevert = new System.Windows.Forms.Label();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Location = new System.Drawing.Point(344, 221);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 1;
		this.btnOK.Text = "OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(425, 221);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 2;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.lvFilesToRevert.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lvFilesToRevert.CheckBoxes = true;
		this.lvFilesToRevert.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.NameColumn, this.TypeColumn, this.PendingActionColumn });
		this.lvFilesToRevert.Location = new System.Drawing.Point(12, 25);
		this.lvFilesToRevert.MultiSelect = false;
		this.lvFilesToRevert.Name = "lvFilesToRevert";
		this.lvFilesToRevert.Size = new System.Drawing.Size(491, 190);
		this.lvFilesToRevert.TabIndex = 0;
		this.lvFilesToRevert.UseCompatibleStateImageBehavior = false;
		this.lvFilesToRevert.View = System.Windows.Forms.View.Details;
		this.lvFilesToRevert.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(lvFilesToRevert_ItemChecked);
		this.NameColumn.Text = "Name";
		this.NameColumn.Width = 255;
		this.TypeColumn.Text = "Type";
		this.TypeColumn.Width = 115;
		this.PendingActionColumn.Text = "Pending Action";
		this.PendingActionColumn.Width = 115;
		this.lblFilesToRevert.AutoSize = true;
		this.lblFilesToRevert.Location = new System.Drawing.Point(12, 9);
		this.lblFilesToRevert.Name = "lblFilesToRevert";
		this.lblFilesToRevert.Size = new System.Drawing.Size(121, 13);
		this.lblFilesToRevert.TabIndex = 6;
		this.lblFilesToRevert.Text = "Select the files to revert:";
		this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBox1.AutoSize = true;
		this.checkBox1.Checked = true;
		this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox1.Location = new System.Drawing.Point(18, 225);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(71, 17);
		this.checkBox1.TabIndex = 7;
		this.checkBox1.Text = "Check All";
		this.checkBox1.UseVisualStyleBackColor = true;
		this.checkBox1.CheckedChanged += new System.EventHandler(checkBox1_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(514, 256);
		base.Controls.Add(this.checkBox1);
		base.Controls.Add(this.lvFilesToRevert);
		base.Controls.Add(this.lblFilesToRevert);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnOK);
		this.MinimumSize = new System.Drawing.Size(530, 245);
		base.Name = "RevertForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Asset Cloud - Revert";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
