using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing;

public class GridControlShowHidePropertiesDialog : Form
{
	private GridView m_gridView;

	private IContainer components = null;

	private Button OK;

	private Button Cancel;

	private CheckedListBox PropertiesListBox;

	private Label Description;

	public GridControlShowHidePropertiesDialog(GridView gridView)
	{
		m_gridView = gridView;
		InitializeComponent();
	}

	private void GridControlShowHidePropertiesDialog_Load(object sender, EventArgs e)
	{
		Dictionary<string, bool> columnUserHiddenStates = m_gridView.GetColumnUserHiddenStates();
		int num = 0;
		foreach (KeyValuePair<string, bool> item in columnUserHiddenStates)
		{
			PropertiesListBox.Items.Add(item.Key);
			PropertiesListBox.SetItemChecked(num++, !item.Value);
		}
	}

	private void OK_Click(object sender, EventArgs e)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		int num = 0;
		foreach (string item in PropertiesListBox.Items)
		{
			dictionary.Add(item, !PropertiesListBox.GetItemChecked(num++));
		}
		m_gridView.SetColumnUserHiddenStates(dictionary);
		m_gridView.Invalidate();
		Close();
	}

	private void Cancel_Click(object sender, EventArgs e)
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.PropertyEditing.GridControlShowHidePropertiesDialog));
		this.OK = new System.Windows.Forms.Button();
		this.Cancel = new System.Windows.Forms.Button();
		this.PropertiesListBox = new System.Windows.Forms.CheckedListBox();
		this.Description = new System.Windows.Forms.Label();
		base.SuspendLayout();
		resources.ApplyResources(this.OK, "OK");
		this.OK.Name = "OK";
		this.OK.UseVisualStyleBackColor = true;
		this.OK.Click += new System.EventHandler(OK_Click);
		resources.ApplyResources(this.Cancel, "Cancel");
		this.Cancel.Name = "Cancel";
		this.Cancel.UseVisualStyleBackColor = true;
		this.Cancel.Click += new System.EventHandler(Cancel_Click);
		resources.ApplyResources(this.PropertiesListBox, "PropertiesListBox");
		this.PropertiesListBox.CheckOnClick = true;
		this.PropertiesListBox.FormattingEnabled = true;
		this.PropertiesListBox.Name = "PropertiesListBox";
		resources.ApplyResources(this.Description, "Description");
		this.Description.Name = "Description";
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.Description);
		base.Controls.Add(this.PropertiesListBox);
		base.Controls.Add(this.Cancel);
		base.Controls.Add(this.OK);
		base.Name = "GridControlShowHidePropertiesDialog";
		base.Load += new System.EventHandler(GridControlShowHidePropertiesDialog_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
