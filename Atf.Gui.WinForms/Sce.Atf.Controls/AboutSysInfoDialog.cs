using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class AboutSysInfoDialog : Form
{
	private class SubItemComparer : IComparer
	{
		private readonly ListView _listView;

		private int _sortColumn;

		public int SortColumn
		{
			get
			{
				return _sortColumn;
			}
			set
			{
				_sortColumn = value;
			}
		}

		public SubItemComparer(ListView listView)
		{
			_listView = listView;
		}

		public int Compare(object x, object y)
		{
			ListViewItem.ListViewSubItem listViewSubItem = ((ListViewItem)x).SubItems[_sortColumn];
			ListViewItem.ListViewSubItem listViewSubItem2 = ((ListViewItem)y).SubItems[_sortColumn];
			if (_listView.Sorting == SortOrder.Descending)
			{
				ListViewItem.ListViewSubItem listViewSubItem3 = listViewSubItem;
				listViewSubItem = listViewSubItem2;
				listViewSubItem2 = listViewSubItem3;
			}
			return CaseInsensitiveComparer.Default.Compare(listViewSubItem.Tag, listViewSubItem2.Tag);
		}
	}

	private ListView assemblyListView;

	private ColumnHeader assemblyColumnHeader;

	private ColumnHeader versionColumnHeader;

	private ColumnHeader dateColumnHeader;

	private Button okButton;

	private readonly Container components = null;

	private readonly SubItemComparer m_subItemComparer;

	public AboutSysInfoDialog()
	{
		InitializeComponent();
		m_subItemComparer = new SubItemComparer(assemblyListView);
		assemblyListView.ListViewItemSorter = m_subItemComparer;
		assemblyListView.ColumnClick += assemblyListView_ColumnClick;
		try
		{
			foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
			{
				FileVersionInfo fileVersionInfo = module.FileVersionInfo;
				if (!string.IsNullOrEmpty(fileVersionInfo.FileVersion))
				{
					ListViewItem listViewItem = new ListViewItem
					{
						Text = module.ModuleName
					};
					string version = $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}.{fileVersionInfo.FileBuildPart}.{fileVersionInfo.FilePrivatePart}";
					ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem(listViewItem, version)
					{
						Tag = new Version(version)
					};
					listViewItem.SubItems.Add(item);
					DateTime lastWriteTime = File.GetLastWriteTime(module.FileName);
					string text = lastWriteTime.ToString("g");
					item = new ListViewItem.ListViewSubItem(listViewItem, text)
					{
						Tag = lastWriteTime
					};
					listViewItem.SubItems.Add(item);
					assemblyListView.Items.Add(listViewItem);
				}
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
	}

	private void assemblyListView_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		m_subItemComparer.SortColumn = e.Column;
		assemblyListView.Sorting = ((assemblyListView.Sorting != SortOrder.Ascending) ? SortOrder.Ascending : SortOrder.Descending);
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.AboutSysInfoDialog));
		this.assemblyListView = new System.Windows.Forms.ListView();
		this.assemblyColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.versionColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.dateColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.okButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.assemblyListView.AllowColumnReorder = true;
		resources.ApplyResources(this.assemblyListView, "assemblyListView");
		this.assemblyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.assemblyColumnHeader, this.versionColumnHeader, this.dateColumnHeader });
		this.assemblyListView.FullRowSelect = true;
		this.assemblyListView.Name = "assemblyListView";
		this.assemblyListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
		this.assemblyListView.UseCompatibleStateImageBehavior = false;
		this.assemblyListView.View = System.Windows.Forms.View.Details;
		resources.ApplyResources(this.assemblyColumnHeader, "assemblyColumnHeader");
		resources.ApplyResources(this.versionColumnHeader, "versionColumnHeader");
		resources.ApplyResources(this.dateColumnHeader, "dateColumnHeader");
		resources.ApplyResources(this.okButton, "okButton");
		this.okButton.Name = "okButton";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += new System.EventHandler(okButton_Click);
		base.AcceptButton = this.okButton;
		resources.ApplyResources(this, "$this");
		this.BackColor = System.Drawing.SystemColors.Control;
		base.Controls.Add(this.okButton);
		base.Controls.Add(this.assemblyListView);
		base.Name = "AboutSysInfoDialog";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.Load += new System.EventHandler(AboutSysInfoDialog_Load);
		base.ResumeLayout(false);
	}

	private void AboutSysInfoDialog_Load(object sender, EventArgs e)
	{
	}

	private void okButton_Click(object sender, EventArgs e)
	{
		Close();
	}
}
