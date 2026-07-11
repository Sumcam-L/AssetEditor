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

public class SubmitForm : Form
{
	private IContainer components = null;

	private Button btnSubmit;

	private Button btnCancel;

	private Label lblChangelistDescription;

	private TextBox tbChangelistDescription;

	private Label lblFilesToSubmit;

	private ListView lvFilesToSubmit;

	private ColumnHeader NameColumn;

	private ColumnHeader PendingActionColumn;

	private Label lblNote;

	private ColumnHeader TypeColumn;

	private CheckBox checkBox1;

	public SourceControlServicePathRequest FilesToSubmit { get; private set; }

	public string Description => tbChangelistDescription.Text;

	public SubmitForm()
	{
		InitializeComponent();
		FilesToSubmit = new SourceControlServicePathRequest(new List<string>());
	}

	public SubmitForm(SourceControlServiceFileResult addResponse, SourceControlServiceFileResult editResponse, SourceControlServiceFileResult deleteResponse)
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
			string item = a.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string fileName = Path.GetFileName(a);
			string extension = Path.GetExtension(a);
			FilesToSubmit.FullPaths.Add(item);
			lvFilesToSubmit.Items.Add(func(fileName, "(" + extension + ")" + " Related Files", a));
		});
	}

	private void tbChangelistDescription_TextChanged(object sender, EventArgs e)
	{
		int count = lvFilesToSubmit.CheckedItems.Count;
		if (btnSubmit.Enabled)
		{
			if (string.IsNullOrEmpty(Description) || count <= 0)
			{
				btnSubmit.Enabled = false;
				lblChangelistDescription.ForeColor = Color.Red;
			}
		}
		else if (!string.IsNullOrEmpty(Description))
		{
			btnSubmit.Enabled = true;
			lblChangelistDescription.ForeColor = Color.Black;
		}
	}

	private void lvFilesToSubmit_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		int count = lvFilesToSubmit.CheckedItems.Count;
		if (string.IsNullOrEmpty(Description) || count <= 0)
		{
			if (btnSubmit.Enabled)
			{
				btnSubmit.Enabled = false;
			}
		}
		else if (!btnSubmit.Enabled)
		{
			btnSubmit.Enabled = true;
		}
		string item = (string)e.Item.Tag;
		if (e.Item.Checked)
		{
			FilesToSubmit.FullPaths.Add(item);
		}
		else
		{
			FilesToSubmit.FullPaths.Remove(item);
		}
	}

	private void checkBox1_CheckedChanged(object sender, EventArgs e)
	{
		foreach (ListViewItem item in lvFilesToSubmit.Items)
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
		this.btnSubmit = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.lblChangelistDescription = new System.Windows.Forms.Label();
		this.tbChangelistDescription = new System.Windows.Forms.TextBox();
		this.lblFilesToSubmit = new System.Windows.Forms.Label();
		this.lvFilesToSubmit = new System.Windows.Forms.ListView();
		this.NameColumn = new System.Windows.Forms.ColumnHeader();
		this.TypeColumn = new System.Windows.Forms.ColumnHeader();
		this.PendingActionColumn = new System.Windows.Forms.ColumnHeader();
		this.lblNote = new System.Windows.Forms.Label();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.btnSubmit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnSubmit.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnSubmit.Enabled = false;
		this.btnSubmit.Location = new System.Drawing.Point(351, 335);
		this.btnSubmit.Name = "btnSubmit";
		this.btnSubmit.Size = new System.Drawing.Size(75, 23);
		this.btnSubmit.TabIndex = 2;
		this.btnSubmit.Text = "Submit";
		this.btnSubmit.UseVisualStyleBackColor = true;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(432, 335);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.lblChangelistDescription.AutoSize = true;
		this.lblChangelistDescription.ForeColor = System.Drawing.Color.Red;
		this.lblChangelistDescription.Location = new System.Drawing.Point(13, 14);
		this.lblChangelistDescription.Name = "lblChangelistDescription";
		this.lblChangelistDescription.Size = new System.Drawing.Size(149, 13);
		this.lblChangelistDescription.TabIndex = 2;
		this.lblChangelistDescription.Text = "Write a changelist description:";
		this.tbChangelistDescription.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.tbChangelistDescription.Location = new System.Drawing.Point(16, 30);
		this.tbChangelistDescription.Multiline = true;
		this.tbChangelistDescription.Name = "tbChangelistDescription";
		this.tbChangelistDescription.Size = new System.Drawing.Size(491, 112);
		this.tbChangelistDescription.TabIndex = 0;
		this.tbChangelistDescription.TextChanged += new System.EventHandler(tbChangelistDescription_TextChanged);
		this.lblFilesToSubmit.AutoSize = true;
		this.lblFilesToSubmit.Location = new System.Drawing.Point(13, 149);
		this.lblFilesToSubmit.Name = "lblFilesToSubmit";
		this.lblFilesToSubmit.Size = new System.Drawing.Size(124, 13);
		this.lblFilesToSubmit.TabIndex = 4;
		this.lblFilesToSubmit.Text = "Select the files to submit:";
		this.lvFilesToSubmit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lvFilesToSubmit.CheckBoxes = true;
		this.lvFilesToSubmit.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.NameColumn, this.TypeColumn, this.PendingActionColumn });
		this.lvFilesToSubmit.Location = new System.Drawing.Point(16, 165);
		this.lvFilesToSubmit.MultiSelect = false;
		this.lvFilesToSubmit.Name = "lvFilesToSubmit";
		this.lvFilesToSubmit.Size = new System.Drawing.Size(491, 156);
		this.lvFilesToSubmit.TabIndex = 1;
		this.lvFilesToSubmit.UseCompatibleStateImageBehavior = false;
		this.lvFilesToSubmit.View = System.Windows.Forms.View.Details;
		this.lvFilesToSubmit.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(lvFilesToSubmit_ItemChecked);
		this.NameColumn.Text = "Name";
		this.NameColumn.Width = 255;
		this.TypeColumn.Text = "Type";
		this.TypeColumn.Width = 115;
		this.PendingActionColumn.Text = "Pending Action";
		this.PendingActionColumn.Width = 115;
		this.lblNote.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.lblNote.AutoSize = true;
		this.lblNote.Location = new System.Drawing.Point(13, 348);
		this.lblNote.Name = "lblNote";
		this.lblNote.Size = new System.Drawing.Size(241, 13);
		this.lblNote.TabIndex = 6;
		this.lblNote.Text = "Note: On submit, unchanged files will be reverted.";
		this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.checkBox1.AutoSize = true;
		this.checkBox1.Checked = true;
		this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox1.Location = new System.Drawing.Point(22, 327);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(71, 17);
		this.checkBox1.TabIndex = 8;
		this.checkBox1.Text = "Check All";
		this.checkBox1.UseVisualStyleBackColor = true;
		this.checkBox1.CheckedChanged += new System.EventHandler(checkBox1_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(519, 370);
		base.Controls.Add(this.checkBox1);
		base.Controls.Add(this.lblNote);
		base.Controls.Add(this.lvFilesToSubmit);
		base.Controls.Add(this.lblFilesToSubmit);
		base.Controls.Add(this.tbChangelistDescription);
		base.Controls.Add(this.lblChangelistDescription);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSubmit);
		this.MinimumSize = new System.Drawing.Size(535, 385);
		base.Name = "SubmitForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Asset Cloud - Submit";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
