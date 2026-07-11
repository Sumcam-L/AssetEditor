using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls;

public class WindowLayoutManageDialog : Form
{
	private class LayoutTag
	{
		public string OldName { get; private set; }

		public LayoutTag(string oldName)
		{
			OldName = oldName;
		}
	}

	private DirectoryInfo m_screenshotDirectory;

	private Label m_selectLayoutLabel;

	private readonly List<string> m_names = new List<string>();

	private readonly List<string> m_deletedLayouts = new List<string>();

	private readonly BalloonToolTipHelper m_toolTip = new BalloonToolTipHelper();

	private readonly Dictionary<string, Image> m_screenshots = new Dictionary<string, Image>();

	private readonly Dictionary<string, string> m_renamedLayouts = new Dictionary<string, string>(StringComparer.CurrentCulture);

	private IContainer components = null;

	private SplitContainer m_split;

	private ListView m_layouts;

	private PictureBox m_screenshot;

	private ColumnHeader columnHeader1;

	private Button m_btnRename;

	private Button m_btnDelete;

	private Button m_btnClose;

	public DirectoryInfo ScreenshotDirectory
	{
		get
		{
			return m_screenshotDirectory;
		}
		set
		{
			if (value == null || value == m_screenshotDirectory)
			{
				return;
			}
			m_screenshotDirectory = value;
			try
			{
				m_screenshots.Clear();
				if (!Directory.Exists(m_screenshotDirectory.FullName))
				{
					return;
				}
				string[] files = Directory.GetFiles(m_screenshotDirectory.FullName, "*.jpg", SearchOption.TopDirectoryOnly);
				if (files != null && files.Length != 0)
				{
					string[] array = files;
					foreach (string text in array)
					{
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
						Image value2 = Image.FromFile(text);
						m_screenshots.Add(fileNameWithoutExtension, value2);
					}
				}
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Manage Layouts: Exception parsing screenshot directory: {0}", ex.Message);
			}
		}
	}

	public IEnumerable<string> LayoutNames
	{
		get
		{
			return m_names;
		}
		set
		{
			try
			{
				m_layouts.BeginUpdate();
				m_names.Clear();
				m_layouts.Items.Clear();
				foreach (string item in value)
				{
					AddLayout(item, m_layouts, m_names);
				}
			}
			finally
			{
				m_layouts.EndUpdate();
			}
		}
	}

	public IEnumerable<KeyValuePair<string, string>> RenamedLayouts => m_renamedLayouts;

	public IEnumerable<string> DeletedLayouts => m_deletedLayouts;

	public WindowLayoutManageDialog()
	{
		InitializeComponent();
		m_selectLayoutLabel = new Label
		{
			Text = "Select a layout".Localize(),
			AutoSize = false,
			Width = m_split.Panel2.Width,
			Height = m_split.Panel2.Height,
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleCenter
		};
		m_split.Panel2.Controls.Add(m_selectLayoutLabel);
		m_screenshot.Hide();
	}

	private void WindowLayoutManageDialogLoad(object sender, EventArgs e)
	{
		int num = m_layouts.Columns[0].Width;
		m_layouts.Columns[0].Width = -1;
		if (m_layouts.Columns[0].Width < num)
		{
			m_layouts.Columns[0].Width = num;
		}
	}

	private void LayoutsSelectedIndexChanged(object sender, EventArgs e)
	{
		TogglePanel2Control();
		IEnumerable<ListViewItem> source = m_layouts.SelectedItems.Cast<ListViewItem>();
		Image value;
		if (source.Count() != 1)
		{
			m_screenshot.Image = null;
		}
		else if (m_screenshots.TryGetValue(source.ElementAt(0).Text, out value))
		{
			m_screenshot.Image = value;
		}
	}

	private void LayoutsAfterLabelEdit(object sender, LabelEditEventArgs e)
	{
		ListViewItem listViewItem = m_layouts.Items[e.Item];
		Point subItemTopRightPoint = GetSubItemTopRightPoint(listViewItem.SubItems[0]);
		string text = listViewItem.Text;
		string label = e.Label;
		if (string.IsNullOrEmpty(label))
		{
			e.CancelEdit = true;
			m_toolTip.Show(m_layouts, subItemTopRightPoint, null, ToolTipIcon.Warning, "Name can't be empty!");
			return;
		}
		if (m_names.Contains(label))
		{
			e.CancelEdit = true;
			if (string.Compare(label, text) != 0)
			{
				m_toolTip.Show(m_layouts, subItemTopRightPoint, null, ToolTipIcon.Warning, "A layout with this name already exists!");
			}
			return;
		}
		if (!WindowLayoutService.IsValidLayoutName(label))
		{
			e.CancelEdit = true;
			m_toolTip.Show(m_layouts, subItemTopRightPoint, null, ToolTipIcon.Warning, "Name contains illegal characters!");
			return;
		}
		LayoutTag layoutTag = (LayoutTag)listViewItem.Tag;
		m_renamedLayouts[layoutTag.OldName] = label;
		m_names.Remove(text);
		m_names.Add(label);
		try
		{
			m_screenshot.Image = null;
			if (m_screenshots.TryGetValue(text, out var value))
			{
				value.Dispose();
				m_screenshots.Remove(text);
			}
			string text2 = Path.Combine(m_screenshotDirectory.FullName + Path.DirectorySeparatorChar, text + ".jpg");
			if (File.Exists(text2))
			{
				string text3 = Path.Combine(m_screenshotDirectory.FullName + Path.DirectorySeparatorChar, label + ".jpg");
				File.Move(text2, text3);
				if (File.Exists(text3))
				{
					m_screenshots.Add(label, Image.FromFile(text3));
				}
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Manage Layouts: Exception renaming screenshot: {0}", ex.Message);
		}
		finally
		{
			if (m_screenshots.TryGetValue(label, out var value2))
			{
				m_screenshot.Image = value2;
			}
		}
	}

	private void BtnRenameClick(object sender, EventArgs e)
	{
		IEnumerable<ListViewItem> source = m_layouts.SelectedItems.Cast<ListViewItem>();
		if (source.Count() == 1)
		{
			source.ElementAt(0).BeginEdit();
		}
	}

	private void BtnDeleteClick(object sender, EventArgs e)
	{
		IEnumerable<ListViewItem> enumerable = m_layouts.SelectedItems.Cast<ListViewItem>();
		if (!enumerable.Any())
		{
			return;
		}
		try
		{
			m_layouts.BeginUpdate();
			foreach (ListViewItem item in enumerable)
			{
				string name = item.Text;
				RemoveLayout(item, m_layouts, m_names);
				RemoveScreenshot(name, m_screenshotDirectory, m_screenshots);
				LayoutTag layoutTag = (LayoutTag)item.Tag;
				m_renamedLayouts.Remove(layoutTag.OldName);
				m_deletedLayouts.Add(layoutTag.OldName);
			}
		}
		finally
		{
			m_layouts.EndUpdate();
		}
	}

	private void TogglePanel2Control()
	{
		if (m_layouts.SelectedItems.Count > 0)
		{
			m_screenshot.Show();
			m_selectLayoutLabel.Hide();
		}
		else
		{
			m_screenshot.Hide();
			m_selectLayoutLabel.Show();
		}
	}

	private static void AddLayout(string name, ListView listView, List<string> names)
	{
		try
		{
			listView.BeginUpdate();
			names.Add(name);
			ListViewItem value = new ListViewItem(name)
			{
				Tag = new LayoutTag(name)
			};
			listView.Items.Add(value);
		}
		finally
		{
			listView.EndUpdate();
		}
	}

	private static void RemoveLayout(ListViewItem lstItem, ListView listView, List<string> names)
	{
		try
		{
			listView.BeginUpdate();
			names.Remove(lstItem.Text);
			listView.Items.Remove(lstItem);
		}
		finally
		{
			listView.EndUpdate();
		}
	}

	private static void RemoveScreenshot(string name, DirectoryInfo screenshotDirectory, Dictionary<string, Image> screenshots)
	{
		try
		{
			if (screenshots.TryGetValue(name, out var value))
			{
				value.Dispose();
				screenshots.Remove(name);
			}
			string path = Path.Combine(screenshotDirectory.FullName + Path.DirectorySeparatorChar, name + ".jpg");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Manage layouts: Exception deleting screenshot: {0}", ex.Message);
		}
	}

	private static Point GetSubItemTopRightPoint(ListViewItem.ListViewSubItem subItem)
	{
		Point result = new Point(subItem.Bounds.X + subItem.Bounds.Right, subItem.Bounds.Y);
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (KeyValuePair<string, Image> screenshot in m_screenshots)
			{
				screenshot.Value.Dispose();
			}
			m_screenshots.Clear();
			m_selectLayoutLabel.Dispose();
			m_selectLayoutLabel = null;
		}
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.WindowLayoutManageDialog));
		this.m_split = new System.Windows.Forms.SplitContainer();
		this.m_layouts = new System.Windows.Forms.ListView();
		this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
		this.m_screenshot = new System.Windows.Forms.PictureBox();
		this.m_btnRename = new System.Windows.Forms.Button();
		this.m_btnDelete = new System.Windows.Forms.Button();
		this.m_btnClose = new System.Windows.Forms.Button();
		this.m_split.Panel1.SuspendLayout();
		this.m_split.Panel2.SuspendLayout();
		this.m_split.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_screenshot).BeginInit();
		base.SuspendLayout();
		resources.ApplyResources(this.m_split, "m_split");
		this.m_split.Name = "m_split";
		this.m_split.Panel1.Controls.Add(this.m_layouts);
		this.m_split.Panel2.Controls.Add(this.m_screenshot);
		resources.ApplyResources(this.m_layouts, "m_layouts");
		this.m_layouts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[1] { this.columnHeader1 });
		this.m_layouts.FullRowSelect = true;
		this.m_layouts.LabelEdit = true;
		this.m_layouts.Name = "m_layouts";
		this.m_layouts.UseCompatibleStateImageBehavior = false;
		this.m_layouts.View = System.Windows.Forms.View.Details;
		this.m_layouts.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(LayoutsAfterLabelEdit);
		this.m_layouts.SelectedIndexChanged += new System.EventHandler(LayoutsSelectedIndexChanged);
		resources.ApplyResources(this.columnHeader1, "columnHeader1");
		resources.ApplyResources(this.m_screenshot, "m_screenshot");
		this.m_screenshot.Name = "m_screenshot";
		this.m_screenshot.TabStop = false;
		resources.ApplyResources(this.m_btnRename, "m_btnRename");
		this.m_btnRename.Name = "m_btnRename";
		this.m_btnRename.UseVisualStyleBackColor = true;
		this.m_btnRename.Click += new System.EventHandler(BtnRenameClick);
		resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
		this.m_btnDelete.Name = "m_btnDelete";
		this.m_btnDelete.UseVisualStyleBackColor = true;
		this.m_btnDelete.Click += new System.EventHandler(BtnDeleteClick);
		resources.ApplyResources(this.m_btnClose, "m_btnClose");
		this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.m_btnClose.Name = "m_btnClose";
		this.m_btnClose.UseVisualStyleBackColor = true;
		base.AcceptButton = this.m_btnClose;
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_btnClose);
		base.Controls.Add(this.m_btnDelete);
		base.Controls.Add(this.m_btnRename);
		base.Controls.Add(this.m_split);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "WindowLayoutManageDialog";
		base.Load += new System.EventHandler(WindowLayoutManageDialogLoad);
		this.m_split.Panel1.ResumeLayout(false);
		this.m_split.Panel2.ResumeLayout(false);
		this.m_split.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_screenshot).EndInit();
		base.ResumeLayout(false);
	}
}
