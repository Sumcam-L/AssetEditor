using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class FilteredFileDialogBase : Form
{
	internal class ListViewItemComparer : IComparer<ListViewItem>
	{
		private int m_columnToSort;

		private SortOrder m_orderOfSort;

		private readonly CaseInsensitiveComparer m_objectCompare;

		public int SortColumn
		{
			get
			{
				return m_columnToSort;
			}
			set
			{
				m_columnToSort = value;
			}
		}

		public SortOrder Order
		{
			get
			{
				return m_orderOfSort;
			}
			set
			{
				m_orderOfSort = value;
			}
		}

		public ListViewItemComparer()
		{
			m_columnToSort = 0;
			m_orderOfSort = SortOrder.None;
			m_objectCompare = new CaseInsensitiveComparer();
		}

		public int Compare(ListViewItem listviewX, ListViewItem listviewY)
		{
			int num = 0;
			if (listviewX.Tag is DirectoryInfo && listviewY.Tag is FileInfo)
			{
				num = -1;
			}
			else if (listviewX.Tag is FileInfo && listviewY.Tag is DirectoryInfo)
			{
				num = 1;
			}
			else if (m_columnToSort == 0)
			{
				num = m_objectCompare.Compare(listviewX.SubItems[m_columnToSort].Text, listviewY.SubItems[m_columnToSort].Text);
			}
			else if (m_columnToSort == 1)
			{
				if (listviewX.Tag is DirectoryInfo && listviewY.Tag is DirectoryInfo)
				{
					num = DateTime.Compare(((DirectoryInfo)listviewX.Tag).LastWriteTime, ((DirectoryInfo)listviewY.Tag).LastWriteTime);
				}
				else if (listviewX.Tag is FileInfo && listviewY.Tag is FileInfo)
				{
					num = DateTime.Compare(((FileInfo)listviewX.Tag).LastWriteTime, ((FileInfo)listviewY.Tag).LastWriteTime);
				}
			}
			else if (m_columnToSort == 2)
			{
				if (listviewX.Tag is DirectoryInfo && listviewY.Tag is DirectoryInfo)
				{
					num = m_objectCompare.Compare(listviewX.SubItems[m_columnToSort].Text, listviewY.SubItems[m_columnToSort].Text);
				}
				else if (listviewX.Tag is FileInfo && listviewY.Tag is FileInfo)
				{
					num = ((FileInfo)listviewX.Tag).Length.CompareTo(((FileInfo)listviewY.Tag).Length);
				}
			}
			if (m_orderOfSort == SortOrder.Ascending)
			{
				return num;
			}
			if (m_orderOfSort == SortOrder.Descending)
			{
				return -num;
			}
			return 0;
		}
	}

	public Predicate<string> CustomFileFilter;

	private List<string> m_patterns = new List<string>();

	private List<string> m_selectedFiles = new List<string>();

	private DirectoryInfo m_lastDir;

	private DirectoryInfo m_curDir;

	private string m_activeFileTypePattern;

	private ListViewItemComparer m_listViewItemComparer;

	private System.Windows.Forms.Timer m_timer;

	private const int SELECTION_DELAY = 100;

	private string m_filter = string.Empty;

	private int m_filterIndex = 1;

	private string m_initialDirectory;

	private bool m_selecting;

	private ListViewItem[] m_cache;

	private DirectoryInfo[] m_subdirInfos;

	private FileInfo[] m_fileInfos;

	private int m_numCachedItems;

	private BackgroundWorker m_backgroundWorker;

	private AutoResetEvent m_resetEvent = new AutoResetEvent(initialState: false);

	private static int[] s_columnWidths;

	private IContainer components = null;

	private ToolStrip toolStrip1;

	private ToolStripButton toolStripButton1;

	private ToolStripButton toolStripButton2;

	private ToolStripButton toolStripButton3;

	private ListView listView1;

	private Label label1;

	private Button button1;

	private Button cancelButton;

	private Label label2;

	private ComboBox fileTypeComboBox;

	private ToolStripDropDownButton viewOptionsToolStripDropDownButton;

	private ComboBox fileNameComboBox;

	private Label label3;

	private ComboBox lookInComboBox;

	private ToolStripButton toolStripFilterButton;

	public bool Multiselect
	{
		set
		{
			listView1.MultiSelect = value;
		}
	}

	public string InitialDirectory
	{
		get
		{
			return m_initialDirectory;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
			{
				m_initialDirectory = value;
			}
		}
	}

	public bool RestoreDirectory { get; set; }

	public string Filter
	{
		get
		{
			return m_filter;
		}
		set
		{
			if (value != null)
			{
				m_filter = value;
			}
			else
			{
				m_filter = string.Empty;
			}
		}
	}

	public int FilterIndex
	{
		get
		{
			return m_filterIndex;
		}
		set
		{
			m_filterIndex = value;
		}
	}

	public IEnumerable<string> SelectedFileNames => m_selectedFiles;

	internal int[] ColumnWidths
	{
		get
		{
			s_columnWidths = new int[listView1.Columns.Count];
			for (int i = 0; i < listView1.Columns.Count; i++)
			{
				s_columnWidths[i] = listView1.Columns[i].Width;
			}
			return s_columnWidths;
		}
		set
		{
			for (int i = 0; i < listView1.Columns.Count; i++)
			{
				if (i < value.Length)
				{
					listView1.Columns[i].Width = value[i];
				}
			}
		}
	}

	public FilteredFileDialogBase()
	{
		InitializeComponent();
		listView1.View = View.Details;
		listView1.VirtualMode = true;
		listView1.BeginUpdate();
		listView1.Columns.Add("Name", 250, HorizontalAlignment.Left);
		listView1.Columns.Add("Date Modified", 130, HorizontalAlignment.Left);
		listView1.Columns.Add("Size", listView1.Width - 250 - 130 - 20, HorizontalAlignment.Right);
		listView1.ColumnClick += listView1_ColumnClick;
		m_listViewItemComparer = new ListViewItemComparer();
		listView1.SmallImageList = new ImageList();
		listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.FolderIcon));
		listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.DocumentImage));
		listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.DiskDriveImage));
		listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.ComputerImage));
		listView1.LargeImageList = new ImageList();
		listView1.LargeImageList.Images.Add(ResourceUtil.GetImage32(Resources.FolderImage));
		listView1.LargeImageList.Images.Add(ResourceUtil.GetImage32(Resources.DocumentImage));
		listView1.LargeImageList.Images.Add(ResourceUtil.GetImage32(Resources.DiskDriveImage));
		listView1.DoubleClick += listView1_DoubleClick;
		listView1.VirtualItemsSelectionRangeChanged += listView1_VirtualItemsSelectionRangeChanged;
		listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
		listView1.AfterLabelEdit += listView1_AfterLabelEdit;
		listView1.RetrieveVirtualItem += listView1_RetrieveVirtualItem;
		listView1.CacheVirtualItems += listView1_CacheVirtualItems;
		listView1.EndUpdate();
		toolStripButton1.Click += backButton_Click;
		toolStripButton2.Click += upButton_Click;
		toolStripButton3.Click += newFolder_Click;
		fileTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
		fileTypeComboBox.SelectedIndexChanged += fileTypeComboBox_SelectedIndexChanged;
		fileNameComboBox.SelectedIndexChanged += fileNameComboBox_SelectedIndexChanged;
		fileNameComboBox.PreviewKeyDown += fileNameComboBox_PreviewKeyDown;
		fileNameComboBox.TextChanged += fileNameComboBox_TextChanged;
		lookInComboBox.SelectedIndexChanged += path_SelectedIndexChanged;
		lookInComboBox.DrawMode = DrawMode.OwnerDrawFixed;
		lookInComboBox.DrawItem += lookInComboBox_DrawItem;
		m_timer = new System.Windows.Forms.Timer
		{
			Interval = 100
		};
		m_timer.Tick += timer_Tick;
		base.Load += fileDialogBase_Load;
		m_backgroundWorker = new BackgroundWorker();
		m_backgroundWorker.WorkerSupportsCancellation = true;
		m_backgroundWorker.DoWork += backgroundWorker_DoWork;
	}

	private void listView1_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
	{
		UpdateSelectedFiles();
	}

	private void listView1_SelectedIndexChanged(object sender, EventArgs e)
	{
		UpdateSelectedFiles();
	}

	private void UpdateSelectedFiles()
	{
		m_selectedFiles.Clear();
		foreach (int selectedIndex in listView1.SelectedIndices)
		{
			ListViewItem listViewItem = listView1.Items[selectedIndex];
			if (listViewItem.Tag is FileInfo)
			{
				m_selectedFiles.Add(((FileInfo)listViewItem.Tag).FullName);
			}
		}
		if (listView1.SelectedIndices.Count > 0)
		{
			m_timer.Stop();
			m_timer.Start();
		}
	}

	private void fileNameComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Return)
		{
			if (StringUtil.IsNullOrEmptyOrWhitespace(fileNameComboBox.Text))
			{
				ResetActiveFileTypePattern();
				return;
			}
			m_activeFileTypePattern = fileNameComboBox.Text;
			UpdateFolderListView();
		}
	}

	private void fileNameComboBox_TextChanged(object sender, EventArgs e)
	{
		if (StringUtil.IsNullOrEmptyOrWhitespace(fileNameComboBox.Text))
		{
			ResetActiveFileTypePattern();
			UpdateFolderListView();
		}
	}

	private void ResetActiveFileTypePattern()
	{
		if (FilterIndex > 0 && FilterIndex <= fileTypeComboBox.Items.Count)
		{
			m_activeFileTypePattern = m_patterns[FilterIndex - 1];
		}
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		m_timer.Stop();
		int num = SelectedFileNames.Count();
		if (num == 1)
		{
			fileNameComboBox.Text = Path.GetFileName(SelectedFileNames.First());
		}
		else if (num > 1)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string selectedFileName in SelectedFileNames)
			{
				stringBuilder.Append("\"");
				stringBuilder.Append(Path.GetFileName(selectedFileName));
				stringBuilder.Append("\" ");
			}
			fileNameComboBox.Text = stringBuilder.ToString();
		}
		else
		{
			fileNameComboBox.Text = string.Empty;
		}
	}

	private void lookInComboBox_DrawItem(object sender, DrawItemEventArgs e)
	{
		e.DrawBackground();
		Brush brush = (((e.State & DrawItemState.Selected) != DrawItemState.Selected) ? Brushes.Black : Brushes.White);
		if (e.Index == 0)
		{
			e.Graphics.DrawImage(listView1.SmallImageList.Images[3], new Point(e.Bounds.X + e.Index * 16, e.Bounds.Y));
		}
		else if (e.Index == 1)
		{
			e.Graphics.DrawImage(listView1.SmallImageList.Images[2], new Point(e.Bounds.X + e.Index * 16, e.Bounds.Y));
		}
		else
		{
			e.Graphics.DrawImage(listView1.SmallImageList.Images[0], new Point(e.Bounds.X + e.Index * 16, e.Bounds.Y));
		}
		if (e.Index >= 0)
		{
			string s = ((ComboBox)sender).Items[e.Index].ToString();
			e.Graphics.DrawString(s, ((Control)sender).Font, brush, e.Bounds.X + (e.Index + 1) * 16, e.Bounds.Y);
		}
	}

	private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
	{
		if (e.Column == m_listViewItemComparer.SortColumn)
		{
			if (m_listViewItemComparer.Order == SortOrder.Ascending)
			{
				m_listViewItemComparer.Order = SortOrder.Descending;
			}
			else
			{
				m_listViewItemComparer.Order = SortOrder.Ascending;
			}
		}
		else
		{
			m_listViewItemComparer.SortColumn = e.Column;
			m_listViewItemComparer.Order = SortOrder.Ascending;
		}
		List<ListViewItem> list = new List<ListViewItem>(m_cache);
		list.Sort(m_listViewItemComparer);
		list.CopyTo(m_cache, 0);
		listView1.VirtualListSize = 0;
		listView1.VirtualListSize = m_cache.Length;
	}

	private void fileDialogBase_Load(object sender, EventArgs e)
	{
		if (!string.IsNullOrEmpty(Filter))
		{
			string[] array = Filter.Split('|');
			if (array.Length > 1)
			{
				for (int i = 0; i < array.Length; i += 2)
				{
					fileTypeComboBox.Items.Add(array[i]);
					m_patterns.Add(array[i + 1]);
				}
				if (FilterIndex > 0 && FilterIndex <= fileTypeComboBox.Items.Count)
				{
					fileTypeComboBox.Text = fileTypeComboBox.Items[FilterIndex - 1].ToString();
					m_activeFileTypePattern = m_patterns[FilterIndex - 1];
				}
			}
		}
		if (string.IsNullOrEmpty(m_initialDirectory) || !Directory.Exists(m_initialDirectory))
		{
			m_initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
		GoToDirectory(Directory.CreateDirectory(m_initialDirectory));
	}

	private bool IsFileTypeMatch(string fileName)
	{
		if (string.IsNullOrEmpty(m_activeFileTypePattern))
		{
			return true;
		}
		bool result = false;
		string[] array = m_activeFileTypePattern.Split(';');
		foreach (string text in array)
		{
			string text2 = text.Replace(".", "\\.");
			text2 = text2.Replace("*", ".*");
			text2 += "\\Z";
			if (Regex.Match(fileName, text2, RegexOptions.IgnoreCase).Success)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private void path_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_selecting)
		{
			return;
		}
		m_selecting = true;
		if (lookInComboBox.SelectedIndex == 0)
		{
			GoToComputer();
		}
		else if (lookInComboBox.SelectedIndex == 1)
		{
			string value = lookInComboBox.Items[lookInComboBox.SelectedIndex] as string;
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo driveInfo in drives)
			{
				if (driveInfo.Name.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
				{
					GotoDrive(driveInfo);
					break;
				}
			}
		}
		else if (lookInComboBox.SelectedIndex > 1)
		{
			StringBuilder stringBuilder = new StringBuilder(string.Concat(lookInComboBox.Items[1], "\\"));
			for (int j = 2; j <= lookInComboBox.SelectedIndex; j++)
			{
				stringBuilder.Append(string.Concat(lookInComboBox.Items[j], "\\"));
			}
			GoToDirectory(Directory.CreateDirectory(stringBuilder.ToString()));
		}
		m_selecting = false;
	}

	private void fileTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		m_activeFileTypePattern = m_patterns[fileTypeComboBox.SelectedIndex];
		UpdateFolderListView();
	}

	private void fileNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (listView1.Focused)
		{
			return;
		}
		listView1.Focus();
		foreach (object item in listView1.Items)
		{
			ListViewItem listViewItem = item as ListViewItem;
			if (string.Equals(listViewItem.Text, fileNameComboBox.Text, StringComparison.CurrentCultureIgnoreCase))
			{
				listViewItem.Selected = true;
			}
			else
			{
				listViewItem.Selected = false;
			}
		}
	}

	private void upButton_Click(object sender, EventArgs e)
	{
		if (m_curDir != null)
		{
			GoToDirectory(m_curDir.Parent);
		}
	}

	private void backButton_Click(object sender, EventArgs e)
	{
		if (m_lastDir != null)
		{
			GoToDirectory(m_lastDir);
		}
	}

	private void newFolder_Click(object sender, EventArgs e)
	{
		if (m_curDir == null)
		{
			return;
		}
		string text = "New folder".Localize();
		int num = 0;
		string text2 = text;
		bool flag;
		do
		{
			flag = true;
			DirectoryInfo[] directories = m_curDir.GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				if (string.Equals(directoryInfo.Name, text2, StringComparison.CurrentCultureIgnoreCase))
				{
					text2 = text + " (" + num + ")";
					num++;
					flag = false;
					break;
				}
			}
		}
		while (!flag);
		m_curDir.CreateSubdirectory(text2);
		UpdateFolderListView();
		foreach (object item in listView1.Items)
		{
			ListViewItem listViewItem = item as ListViewItem;
			if (listViewItem.Text == text2)
			{
				listViewItem.Selected = true;
				listView1.LabelEdit = true;
				listViewItem.BeginEdit();
			}
		}
	}

	private void listView1_DoubleClick(object sender, EventArgs e)
	{
		if (listView1.SelectedIndices.Count == 1)
		{
			int index = listView1.SelectedIndices[0];
			if (listView1.Items[index].Tag is DirectoryInfo)
			{
				GoToDirectory((DirectoryInfo)listView1.Items[index].Tag);
			}
			else if (listView1.Items[index].Tag is FileInfo)
			{
				base.DialogResult = DialogResult.OK;
				Close();
			}
			else if (listView1.Items[index].Tag is DriveInfo)
			{
				GotoDrive((DriveInfo)listView1.Items[index].Tag);
			}
		}
	}

	private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		listView1.LabelEdit = false;
		if (e.Label == null)
		{
			return;
		}
		ListViewItem listViewItem = listView1.Items[e.Item];
		if (!(listViewItem.Tag is DirectoryInfo))
		{
			return;
		}
		bool flag = false;
		DirectoryInfo[] directories = m_curDir.GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			if (string.Equals(directoryInfo.Name, e.Label, StringComparison.CurrentCultureIgnoreCase))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			string fullName = ((DirectoryInfo)listViewItem.Tag).FullName;
			string destDirName = m_curDir.FullName + "\\" + e.Label;
			Directory.Move(fullName, destDirName);
		}
		else
		{
			e.CancelEdit = true;
		}
	}

	private void GoToDirectory(DirectoryInfo dirInfo)
	{
		if (dirInfo != null)
		{
			m_lastDir = m_curDir;
			m_curDir = dirInfo;
			UpdateFolderListView();
			UpdateCurrentPath();
		}
	}

	private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
	{
		e.Item = GetListItem(e.ItemIndex);
	}

	private void listView1_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
	{
		for (int i = e.StartIndex; i <= e.EndIndex; i++)
		{
			GetListItem(i);
		}
	}

	private ListViewItem GetListItem(int i)
	{
		ListViewItem listViewItem = null;
		if (m_cache != null && i >= 0 && i < m_cache.Length && m_cache[i] != null)
		{
			return m_cache[i];
		}
		if (i < m_subdirInfos.Length)
		{
			DirectoryInfo directoryInfo = m_subdirInfos[i];
			listViewItem = new ListViewItem(directoryInfo.Name, 0);
			listViewItem.ImageIndex = 0;
			listViewItem.Tag = directoryInfo;
			ListViewItem.ListViewSubItem[] items = new ListViewItem.ListViewSubItem[2]
			{
				new ListViewItem.ListViewSubItem(listViewItem, directoryInfo.LastWriteTime.ToString()),
				new ListViewItem.ListViewSubItem(listViewItem, "")
			};
			listViewItem.SubItems.AddRange(items);
		}
		else if (i < m_subdirInfos.Length + m_fileInfos.Length)
		{
			FileInfo fileInfo = m_fileInfos[i - m_subdirInfos.Length];
			listViewItem = new ListViewItem(fileInfo.Name, 0);
			listViewItem.Tag = fileInfo;
			listViewItem.ImageIndex = 1;
			float num = (float)fileInfo.Length / 1024f;
			ListViewItem.ListViewSubItem[] items2 = new ListViewItem.ListViewSubItem[2]
			{
				new ListViewItem.ListViewSubItem(listViewItem, fileInfo.LastWriteTime.ToString()),
				new ListViewItem.ListViewSubItem(listViewItem, num.ToString("N0") + " KB")
			};
			listViewItem.SubItems.AddRange(items2);
		}
		m_cache[i] = listViewItem;
		m_numCachedItems++;
		return listViewItem;
	}

	private void UpdateFolderListView()
	{
		if (m_backgroundWorker.IsBusy)
		{
			m_backgroundWorker.CancelAsync();
			m_resetEvent.WaitOne();
		}
		listView1.VirtualListSize = 0;
		listView1.Items.Clear();
		if (m_curDir == null)
		{
			return;
		}
		m_cache = null;
		m_numCachedItems = 0;
		try
		{
			m_subdirInfos = m_curDir.GetDirectories();
		}
		catch (Exception)
		{
			m_subdirInfos = new DirectoryInfo[0];
		}
		try
		{
			fileNameComboBox.Items.Clear();
			List<FileInfo> list = new List<FileInfo>();
			bool flag = toolStripFilterButton.Checked;
			FileInfo[] files = m_curDir.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				bool flag2 = !flag;
				if (flag)
				{
					if (IsFileTypeMatch(fileInfo.Name))
					{
						flag2 = CustomFileFilter == null || CustomFileFilter(fileInfo.FullName);
					}
					if (!flag2)
					{
						continue;
					}
				}
				if (flag2)
				{
					list.Add(fileInfo);
				}
			}
			m_fileInfos = new FileInfo[list.Count];
			list.CopyTo(m_fileInfos);
			listView1.VirtualListSize = m_subdirInfos.Length + m_fileInfos.Length;
			m_cache = new ListViewItem[listView1.VirtualListSize];
		}
		catch (Exception)
		{
		}
		m_backgroundWorker.RunWorkerAsync();
	}

	private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!e.Cancel && m_numCachedItems < m_cache.Length)
		{
			for (int i = 0; i < m_cache.Length; i++)
			{
				if (m_cache[i] == null)
				{
					GetListItem(i);
				}
			}
		}
		m_resetEvent.Set();
	}

	private void UpdateCurrentPath()
	{
		lookInComboBox.Items.Clear();
		lookInComboBox.Items.Add("Computer".Localize());
		lookInComboBox.Items.AddRange(PathUtil.GetPathElements(m_curDir.FullName));
		m_selecting = true;
		lookInComboBox.Text = m_curDir.Name.TrimEnd('\\');
		m_selecting = false;
	}

	private void GoToComputer()
	{
		lookInComboBox.Text = "Computer".Localize();
		listView1.VirtualListSize = 0;
		listView1.Items.Clear();
		m_cache = new ListViewItem[DriveInfo.GetDrives().Length];
		int num = 0;
		DriveInfo[] drives = DriveInfo.GetDrives();
		foreach (DriveInfo driveInfo in drives)
		{
			ListViewItem listViewItem = new ListViewItem(driveInfo.Name, 0);
			listViewItem.ImageIndex = 2;
			listViewItem.Tag = driveInfo;
			ListViewItem.ListViewSubItem[] items = new ListViewItem.ListViewSubItem[2]
			{
				new ListViewItem.ListViewSubItem(listViewItem, "Drive"),
				new ListViewItem.ListViewSubItem(listViewItem, driveInfo.Name)
			};
			listViewItem.SubItems.AddRange(items);
			m_cache[num++] = listViewItem;
		}
		listView1.VirtualListSize = m_cache.Length;
		m_lastDir = m_curDir;
		m_curDir = null;
	}

	private void GotoDrive(DriveInfo drive)
	{
		GoToDirectory(drive.RootDirectory);
	}

	private void toolStripFilterButton_CheckedChanged(object sender, EventArgs e)
	{
		UpdateFolderListView();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.FilteredFileDialogBase));
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
		this.toolStripFilterButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
		this.viewOptionsToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
		this.listView1 = new System.Windows.Forms.ListView();
		this.label1 = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.cancelButton = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.fileTypeComboBox = new System.Windows.Forms.ComboBox();
		this.fileNameComboBox = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.lookInComboBox = new System.Windows.Forms.ComboBox();
		this.toolStrip1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.toolStripButton1, this.toolStripButton2, this.toolStripFilterButton, this.toolStripButton3, this.viewOptionsToolStripDropDownButton });
		resources.ApplyResources(this.toolStrip1, "toolStrip1");
		this.toolStrip1.Name = "toolStrip1";
		this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
		this.toolStripButton1.Name = "toolStripButton1";
		this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		resources.ApplyResources(this.toolStripButton2, "toolStripButton2");
		this.toolStripButton2.Name = "toolStripButton2";
		this.toolStripFilterButton.Checked = true;
		this.toolStripFilterButton.CheckOnClick = true;
		this.toolStripFilterButton.CheckState = System.Windows.Forms.CheckState.Checked;
		this.toolStripFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		resources.ApplyResources(this.toolStripFilterButton, "toolStripFilterButton");
		this.toolStripFilterButton.Name = "toolStripFilterButton";
		this.toolStripFilterButton.CheckedChanged += new System.EventHandler(toolStripFilterButton_CheckedChanged);
		this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		resources.ApplyResources(this.toolStripButton3, "toolStripButton3");
		this.toolStripButton3.Name = "toolStripButton3";
		this.viewOptionsToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		resources.ApplyResources(this.viewOptionsToolStripDropDownButton, "viewOptionsToolStripDropDownButton");
		this.viewOptionsToolStripDropDownButton.Name = "viewOptionsToolStripDropDownButton";
		resources.ApplyResources(this.listView1, "listView1");
		this.listView1.Name = "listView1";
		this.listView1.UseCompatibleStateImageBehavior = false;
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this.button1, "button1");
		this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.button1.Name = "button1";
		this.button1.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.cancelButton, "cancelButton");
		this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.label2, "label2");
		this.label2.Name = "label2";
		resources.ApplyResources(this.fileTypeComboBox, "fileTypeComboBox");
		this.fileTypeComboBox.FormattingEnabled = true;
		this.fileTypeComboBox.Name = "fileTypeComboBox";
		resources.ApplyResources(this.fileNameComboBox, "fileNameComboBox");
		this.fileNameComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
		this.fileNameComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
		this.fileNameComboBox.FormattingEnabled = true;
		this.fileNameComboBox.Name = "fileNameComboBox";
		resources.ApplyResources(this.label3, "label3");
		this.label3.Name = "label3";
		this.lookInComboBox.FormattingEnabled = true;
		resources.ApplyResources(this.lookInComboBox, "lookInComboBox");
		this.lookInComboBox.Name = "lookInComboBox";
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.lookInComboBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.fileNameComboBox);
		base.Controls.Add(this.fileTypeComboBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.cancelButton);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.listView1);
		base.Controls.Add(this.toolStrip1);
		base.Name = "FilteredFileDialogBase";
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
