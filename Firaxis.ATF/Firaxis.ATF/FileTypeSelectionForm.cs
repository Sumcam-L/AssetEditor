using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public class FileTypeSelectionForm : Form
{
	public string SelectedEntityType = "";

	private IContainer components;

	private ListView entityTypesListBox;

	private Label label1;

	private Button okButton;

	private Button cancelButton;

	public FileTypeSelectionForm(IEnumerable<DocumentClientInfo> entityTypes)
	{
		InitializeComponent();
		base.ShowIcon = false;
		base.ControlBox = false;
		base.MinimizeBox = false;
		base.MaximizeBox = false;
		base.FormBorderStyle = FormBorderStyle.FixedDialog;
		okButton.Enabled = false;
		entityTypesListBox.Groups.Clear();
		entityTypesListBox.LargeImageList = new ImageList();
		entityTypesListBox.SmallImageList = new ImageList();
		entityTypesListBox.ShowGroups = true;
		entityTypesListBox.View = View.LargeIcon;
		IDictionary<string, ListViewGroup> dictionary = new Dictionary<string, ListViewGroup>();
		foreach (DocumentClientInfoEx item in from ob in entityTypes.OfType<DocumentClientInfoEx>()
			orderby ob.Category
			select ob)
		{
			if (!item.IsHidden)
			{
				if (!dictionary.ContainsKey(item.Category))
				{
					dictionary[item.Category] = entityTypesListBox.Groups.Add(item.Category, item.Category);
				}
				ListViewItem listViewItem = entityTypesListBox.Items.Add(item.FileType);
				string text = item.As<EntityDocumentClientInfo>()?.DocumentIconName ?? item.OpenIconName;
				if (!entityTypesListBox.LargeImageList.Images.ContainsKey(text))
				{
					entityTypesListBox.LargeImageList.Images.Add(text, ResourceUtil.GetImage32(text));
					entityTypesListBox.SmallImageList.Images.Add(text, ResourceUtil.GetImage24(text));
				}
				int imageIndex = entityTypesListBox.LargeImageList.Images.IndexOfKey(text);
				listViewItem.Group = dictionary[item.Category];
				listViewItem.ImageIndex = imageIndex;
			}
		}
		dictionary.Clear();
		base.Load += FileTypeSelectionForm_Load;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			base.Load -= FileTypeSelectionForm_Load;
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void FileTypeSelectionForm_Load(object sender, EventArgs e)
	{
		SkinService.ApplyActiveSkin(this);
	}

	private void AcceptAndClose()
	{
		SelectedEntityType = entityTypesListBox.SelectedItems[0].Text;
		Close();
	}

	private void cancelButton_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void entityTypesListBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (entityTypesListBox.SelectedItems.Count > 0)
		{
			AcceptAndClose();
		}
	}

	private void entityTypesListBox_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (entityTypesListBox.SelectedItems.Count > 0)
		{
			AcceptAndClose();
		}
	}

	private void entityTypesListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		okButton.Enabled = entityTypesListBox.SelectedItems.Count > 0;
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		AcceptAndClose();
	}

	private void InitializeComponent()
	{
		System.Windows.Forms.ListViewGroup listViewGroup = new System.Windows.Forms.ListViewGroup("Entities", System.Windows.Forms.HorizontalAlignment.Left);
		System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Packages", System.Windows.Forms.HorizontalAlignment.Left);
		this.entityTypesListBox = new System.Windows.Forms.ListView();
		this.label1 = new System.Windows.Forms.Label();
		this.okButton = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.entityTypesListBox.Alignment = System.Windows.Forms.ListViewAlignment.Default;
		this.entityTypesListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.entityTypesListBox.AutoArrange = false;
		this.entityTypesListBox.BackColor = System.Drawing.SystemColors.Window;
		this.entityTypesListBox.BackgroundImageTiled = true;
		this.entityTypesListBox.FullRowSelect = true;
		listViewGroup.Header = "Entities";
		listViewGroup.Name = "entitiesGroup";
		listViewGroup2.Header = "Packages";
		listViewGroup2.Name = "packagesGroup";
		this.entityTypesListBox.Groups.AddRange(new System.Windows.Forms.ListViewGroup[2] { listViewGroup, listViewGroup2 });
		this.entityTypesListBox.HideSelection = false;
		this.entityTypesListBox.Location = new System.Drawing.Point(12, 25);
		this.entityTypesListBox.MultiSelect = false;
		this.entityTypesListBox.Name = "entityTypesListBox";
		this.entityTypesListBox.Size = new System.Drawing.Size(411, 199);
		this.entityTypesListBox.TabIndex = 0;
		this.entityTypesListBox.TileSize = new System.Drawing.Size(100, 20);
		this.entityTypesListBox.UseCompatibleStateImageBehavior = false;
		this.entityTypesListBox.View = System.Windows.Forms.View.Tile;
		this.entityTypesListBox.SelectedIndexChanged += new System.EventHandler(entityTypesListBox_SelectedIndexChanged);
		this.entityTypesListBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(entityTypesListBox_KeyPress);
		this.entityTypesListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(entityTypesListBox_MouseDoubleClick);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(9, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(50, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "File Type";
		this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okButton.Location = new System.Drawing.Point(261, 249);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 1;
		this.okButton.Text = "OK";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += new System.EventHandler(okButton_Click);
		this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(348, 249);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(75, 23);
		this.cancelButton.TabIndex = 2;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;
		this.cancelButton.Click += new System.EventHandler(cancelButton_Click);
		base.AcceptButton = this.okButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.cancelButton;
		base.ClientSize = new System.Drawing.Size(435, 284);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.entityTypesListBox);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(420, 300);
		base.Name = "FileTypeSelectionForm";
		this.Text = "Pick the file type to create";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
