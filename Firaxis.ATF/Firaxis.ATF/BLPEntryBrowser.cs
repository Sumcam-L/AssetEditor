using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class BLPEntryBrowser : Form
{
	public struct XLPData
	{
		public string xlpPath;

		public string blpPath;
	}

	private string[] m_cachedValidClassNames;

	private readonly ICivTechService m_civTechService;

	private string m_filter = string.Empty;

	private readonly IXLPRegistry m_xlpRegistry;

	private IContainer components;

	private Button btnOK;

	private Button btnCancel;

	private ListView lbBrowser;

	private ColumnHeader columnHeader1;

	private ColumnHeader columnHeader2;

	private Label _filterLabel;

	private TextBox _filterTextBox;

	private ColumnHeader columnHeader3;

	public bool multiSelect
	{
		get
		{
			return lbBrowser.MultiSelect;
		}
		set
		{
			lbBrowser.MultiSelect = value;
		}
	}

	private IEnumerable<string> CachedValidLibraryNames
	{
		get
		{
			return m_cachedValidClassNames;
		}
		set
		{
			m_cachedValidClassNames = value.ToArray();
		}
	}

	private string Filter
	{
		get
		{
			return m_filter;
		}
		set
		{
			if (!string.Equals(value, m_filter, StringComparison.CurrentCultureIgnoreCase))
			{
				m_filter = value;
			}
		}
	}

	private IXLPRegistry XLPRegistry
	{
		get
		{
			BugSubmitter.SilentAssert(m_xlpRegistry != null, "XLP Registry is not initialized!  This needs to be initialized for the BLP Entry Browser! @assign bwhitman");
			return m_xlpRegistry;
		}
	}

	public IEnumerable<string> selectedBlpEntries
	{
		get
		{
			ICollection<string> collection = new SortedSet<string>();
			foreach (ListViewItem item in lbBrowser.Items)
			{
				if (item.Selected)
				{
					collection.Add(item.Text);
				}
			}
			return collection;
		}
	}

	public IEnumerable<XLPData> selectedXLPEntries
	{
		get
		{
			ICollection<XLPData> collection = new List<XLPData>();
			foreach (ListViewItem item in lbBrowser.Items)
			{
				if (item.Selected)
				{
					collection.Add((XLPData)item.Tag);
				}
			}
			return collection.OrderBy((XLPData x) => x.xlpPath);
		}
	}

	public IEnumerable<string> selectedObjectEntries
	{
		get
		{
			ICollection<string> collection = new SortedSet<string>();
			foreach (ListViewItem item in lbBrowser.Items)
			{
				if (item.Selected)
				{
					collection.Add(item.SubItems[0].Text);
				}
			}
			return collection;
		}
	}

	public BLPEntryBrowser(IXLPRegistry xlpReg, ICivTechService civTechSvc)
	{
		InitializeComponent();
		AdjustColumnSizes();
		m_xlpRegistry = xlpReg;
		m_civTechService = civTechSvc;
	}

	public IDictionary<string, IEnumerable<IXLPCacheData>> GetXLPs(IEnumerable<string> validLibraryNames)
	{
		IXLPRegistry xLPRegistry = XLPRegistry;
		if (!validLibraryNames.Any())
		{
			return xLPRegistry.GetXLPCacheData();
		}
		ISet<string> packageNames = GetPackageNames(validLibraryNames);
		if (packageNames.Count > 0)
		{
			return xLPRegistry.GetXLPCacheData(packageNames);
		}
		return xLPRegistry.GetXLPCacheData();
	}

	public DialogResult OpenBLPEntry(ref string entryName, ref string xlpPath, ref string blpPath, IEnumerable<string> validTypes, bool allowMultiSelect = false)
	{
		RefreshEntries(validTypes, Filter);
		SelectEntry(entryName);
		lbBrowser.MultiSelect = allowMultiSelect;
		if (ShowDialog() == DialogResult.Cancel)
		{
			return DialogResult.Cancel;
		}
		if (!SetEntryName(ref entryName, ref xlpPath, ref blpPath))
		{
			return DialogResult.Cancel;
		}
		return DialogResult.OK;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		if (base.Visible)
		{
			_filterTextBox.Text = Filter;
			_filterTextBox.Focus();
			base.ActiveControl = _filterTextBox;
			_filterTextBox.SelectAll();
		}
		base.OnVisibleChanged(e);
	}

	private void AdjustColumnSizes()
	{
		int num = (int)(0.35 * (double)lbBrowser.Width);
		lbBrowser.Columns[0].Width = num - 2;
		lbBrowser.Columns[1].Width = lbBrowser.Width - num - 4;
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private ISet<string> GetPackageNames(IEnumerable<string> validLibraryNames)
	{
		IList<string> dependencies = m_civTechService.PrimaryProject.Dependencies;
		ISet<string> set = new SortedSet<string>();
		foreach (string item in dependencies)
		{
			ProjectEnvironment project = null;
			if (m_civTechService.ActiveProjectMap.GetProject(item, ref project))
			{
				AddValidLibrariesToSet(project.ArtSpecifications, validLibraryNames, set);
			}
		}
		AddValidLibrariesToSet(m_civTechService.PrimaryProject.ArtSpecifications, validLibraryNames, set);
		return set;
	}

	private void lbBrowser_SizeChanged(object sender, EventArgs e)
	{
		AdjustColumnSizes();
	}

	private void lbBrowser_DoubleClick(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void lbBrowser_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	private void _filterTextBox_TextChanged(object sender, EventArgs e)
	{
		RefreshEntries(CachedValidLibraryNames, _filterTextBox.Text);
	}

	private void AddValidLibrariesToSet(IEnumerable<IGameArtSpecification> artSpecifications, IEnumerable<string> validLibraryNames, ISet<string> validPackages)
	{
		foreach (IGameArtSpecification artSpecification in artSpecifications)
		{
			foreach (IGameLibrary item in artSpecification.GameLibraries.Where((IGameLibrary lib) => validLibraryNames.Contains(lib.LibraryName)))
			{
				foreach (string item2 in item.RelativePackagePaths.Where((string path) => !string.IsNullOrEmpty(path)))
				{
					validPackages.Add(item2);
				}
			}
		}
	}

	private void SelectEntry(string entry)
	{
		ListViewItem[] array = lbBrowser.Items.Find(entry, searchAllSubItems: false);
		if (array.Length != 0)
		{
			array[0].Selected = true;
		}
	}

	private bool SetEntryName(ref string name, ref string xlpPath, ref string blpPath)
	{
		foreach (ListViewItem item in lbBrowser.Items)
		{
			if (item.Selected)
			{
				name = item.Text;
				XLPData xLPData = (XLPData)item.Tag;
				blpPath = xLPData.blpPath;
				xlpPath = xLPData.xlpPath;
				return true;
			}
		}
		return false;
	}

	private void RefreshEntries(IEnumerable<string> validLibraryNames, string filter)
	{
		CachedValidLibraryNames = validLibraryNames;
		Filter = filter;
		lbBrowser.SuspendDrawing();
		lbBrowser.Items.Clear();
		Regex regex = RegexFilterHelper.BuildRegex(filter.ToLower());
		IDictionary<string, IEnumerable<IXLPCacheData>> xLPs = GetXLPs(CachedValidLibraryNames);
		foreach (string item in new SortedSet<string>(xLPs.Keys))
		{
			foreach (IXLPCacheData item2 in (IEnumerable<IXLPCacheData>)xLPs[item].OrderBy((IXLPCacheData x) => x.EntryName).ToArray())
			{
				if (regex == null || regex.Match(item2.EntryName).Success)
				{
					ListViewItem listViewItem = lbBrowser.Items.Add(item2.EntryName);
					listViewItem.SubItems.Add(item2.ObjectName);
					listViewItem.SubItems.Add(item2.BLPPackage);
					listViewItem.Name = item2.EntryName;
					XLPData xLPData = new XLPData
					{
						xlpPath = item2.XLPPath,
						blpPath = item2.BLPPackage
					};
					listViewItem.Tag = xLPData;
				}
			}
		}
		lbBrowser.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		lbBrowser.ResumeDrawing();
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
		this.lbBrowser = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
		this._filterLabel = new System.Windows.Forms.Label();
		this._filterTextBox = new System.Windows.Forms.TextBox();
		this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
		base.SuspendLayout();
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.Location = new System.Drawing.Point(336, 389);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(75, 23);
		this.btnOK.TabIndex = 0;
		this.btnOK.Text = "&OK";
		this.btnOK.UseVisualStyleBackColor = true;
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(417, 389);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(75, 23);
		this.btnCancel.TabIndex = 1;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.lbBrowser.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lbBrowser.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.columnHeader1, this.columnHeader2, this.columnHeader3 });
		this.lbBrowser.FullRowSelect = true;
		this.lbBrowser.HideSelection = false;
		this.lbBrowser.Location = new System.Drawing.Point(12, 39);
		this.lbBrowser.MultiSelect = false;
		this.lbBrowser.Name = "lbBrowser";
		this.lbBrowser.Size = new System.Drawing.Size(480, 333);
		this.lbBrowser.TabIndex = 2;
		this.lbBrowser.UseCompatibleStateImageBehavior = false;
		this.lbBrowser.View = System.Windows.Forms.View.Details;
		this.lbBrowser.SizeChanged += new System.EventHandler(lbBrowser_SizeChanged);
		this.lbBrowser.DoubleClick += new System.EventHandler(lbBrowser_DoubleClick);
		this.lbBrowser.KeyDown += new System.Windows.Forms.KeyEventHandler(lbBrowser_KeyDown);
		this.columnHeader1.Text = "Name";
		this.columnHeader1.Width = 98;
		this.columnHeader2.Text = "Asset Path";
		this.columnHeader2.Width = 103;
		this._filterLabel.AutoSize = true;
		this._filterLabel.Location = new System.Drawing.Point(13, 13);
		this._filterLabel.Name = "_filterLabel";
		this._filterLabel.Size = new System.Drawing.Size(29, 13);
		this._filterLabel.TabIndex = 3;
		this._filterLabel.Text = "Filter";
		this._filterTextBox.Location = new System.Drawing.Point(47, 9);
		this._filterTextBox.Name = "_filterTextBox";
		this._filterTextBox.Size = new System.Drawing.Size(445, 20);
		this._filterTextBox.TabIndex = 4;
		this._filterTextBox.TextChanged += new System.EventHandler(_filterTextBox_TextChanged);
		this.columnHeader3.Text = "BLP Package";
		this.columnHeader3.Width = 112;
		base.AcceptButton = this.btnOK;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(504, 424);
		base.Controls.Add(this._filterTextBox);
		base.Controls.Add(this._filterLabel);
		base.Controls.Add(this.lbBrowser);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnOK);
		base.Name = "BLPEntryBrowser";
		this.Text = "BLP Entry Browser";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
