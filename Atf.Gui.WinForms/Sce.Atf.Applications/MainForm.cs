using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(Form))]
[Export(typeof(IMainWindow))]
[Export(typeof(IWin32Window))]
[Export(typeof(ISynchronizeInvoke))]
[Export(typeof(MainForm))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MainForm : Form, IMainWindow
{
	private class ElementSortComparer<T> : IComparer<XmlElement>
	{
		int IComparer<XmlElement>.Compare(XmlElement element1, XmlElement element2)
		{
			string[] array = element1.GetAttribute("Location").Split(',');
			string[] array2 = element2.GetAttribute("Location").Split(',');
			return int.Parse(array[0]) - int.Parse(array2[0]);
		}
	}

	private readonly IContainer components = null;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService;

	private readonly ToolStripContainer m_toolStripContainer;

	private Rectangle m_mainFormBounds;

	private FormWindowState m_previousWindoState;

	private bool m_maximizeWindow;

	private bool m_mainFormLoaded;

	public ToolStripContainer ToolStripContainer => m_toolStripContainer;

	public IWin32Window DialogOwner
	{
		get
		{
			if (!base.IsDisposed)
			{
				return this;
			}
			return null;
		}
	}

	public Rectangle MainFormBounds
	{
		get
		{
			return m_mainFormBounds;
		}
		set
		{
			if (WinFormsUtil.IsOnScreen(value))
			{
				base.Bounds = value;
			}
		}
	}

	public FormWindowState MainFormWindowState
	{
		get
		{
			FormWindowState formWindowState = base.WindowState;
			if (formWindowState == FormWindowState.Minimized)
			{
				formWindowState = FormWindowState.Normal;
			}
			return formWindowState;
		}
		set
		{
			if (value == FormWindowState.Maximized && !m_mainFormLoaded)
			{
				m_maximizeWindow = true;
				base.WindowState = FormWindowState.Normal;
			}
			else
			{
				base.WindowState = value;
			}
		}
	}

	[Browsable(false)]
	public string ToolStripContainerSettings
	{
		get
		{
			if (m_toolStripContainer == null)
			{
				return string.Empty;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("ToolStripContainerSettings");
			xmlDocument.AppendChild(xmlElement);
			XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("version");
			xmlAttribute.Value = "2";
			xmlElement.Attributes.Append(xmlAttribute);
			SavePanelState(m_toolStripContainer.TopToolStripPanel, "TopToolStripPanel", xmlDocument, xmlElement);
			SavePanelState(m_toolStripContainer.LeftToolStripPanel, "LeftToolStripPanel", xmlDocument, xmlElement);
			SavePanelState(m_toolStripContainer.BottomToolStripPanel, "BottomToolStripPanel", xmlDocument, xmlElement);
			SavePanelState(m_toolStripContainer.RightToolStripPanel, "RightToolStripPanel", xmlDocument, xmlElement);
			return xmlDocument.InnerXml;
		}
		set
		{
			if (!m_mainFormLoaded || m_toolStripContainer == null)
			{
				return;
			}
			ElementSortComparer<XmlElement> comparer = new ElementSortComparer<XmlElement>();
			Dictionary<string, ToolStrip> dictionary = new Dictionary<string, ToolStrip>();
			Dictionary<string, ToolStripItem> dictionary2 = new Dictionary<string, ToolStripItem>();
			try
			{
				PrepareLoadPanelState(m_toolStripContainer.TopToolStripPanel, dictionary, dictionary2);
				PrepareLoadPanelState(m_toolStripContainer.LeftToolStripPanel, dictionary, dictionary2);
				PrepareLoadPanelState(m_toolStripContainer.BottomToolStripPanel, dictionary, dictionary2);
				PrepareLoadPanelState(m_toolStripContainer.RightToolStripPanel, dictionary, dictionary2);
				SuspendLayout();
				m_toolStripContainer.SuspendLayout();
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				XmlElement documentElement = xmlDocument.DocumentElement;
				if (documentElement == null || documentElement.Name != "ToolStripContainerSettings")
				{
					throw new InvalidOperationException("Invalid Toolstrip settings");
				}
				if (!documentElement.HasAttribute("version") || documentElement.Attributes["version"].Value != "2")
				{
					throw new Exception("Unsupported settings format. Discarding existing toolbar layout setting information, after a save the new setting info should be correct. Unless the version setting code below is out of sync this this check");
				}
				XmlNodeList xmlNodeList = documentElement.SelectNodes("ToolStripPanel");
				foreach (XmlElement item2 in xmlNodeList)
				{
					ToolStripPanel toolStripPanel;
					switch (item2.GetAttribute("Name"))
					{
					case "TopToolStripPanel":
						toolStripPanel = m_toolStripContainer.TopToolStripPanel;
						break;
					case "LeftToolStripPanel":
						toolStripPanel = m_toolStripContainer.LeftToolStripPanel;
						break;
					case "BottomToolStripPanel":
						toolStripPanel = m_toolStripContainer.BottomToolStripPanel;
						break;
					case "RightToolStripPanel":
						toolStripPanel = m_toolStripContainer.RightToolStripPanel;
						break;
					default:
						continue;
					}
					List<XmlElement> list = new List<XmlElement>();
					foreach (XmlElement childNode in item2.ChildNodes)
					{
						list.Add(childNode);
					}
					SortedDictionary<int, List<XmlElement>> sortedDictionary = new SortedDictionary<int, List<XmlElement>>();
					foreach (XmlElement item3 in list)
					{
						string[] array = item3.GetAttribute("Location").Split(',');
						int key = int.Parse(array[1]);
						if (!sortedDictionary.TryGetValue(key, out var value2))
						{
							value2 = new List<XmlElement>();
							sortedDictionary.Add(key, value2);
						}
						value2.Add(item3);
					}
					foreach (List<XmlElement> value5 in sortedDictionary.Values)
					{
						value5.Sort(comparer);
					}
					int num = 0;
					int num2 = 0;
					int num3 = ((sortedDictionary.Count != 0) ? sortedDictionary.Keys.First() : 0);
					foreach (List<XmlElement> value6 in sortedDictionary.Values)
					{
						num3 += num2;
						foreach (XmlElement item4 in value6)
						{
							string attribute = item4.GetAttribute("Name");
							if (!dictionary.TryGetValue(attribute, out var value3))
							{
								continue;
							}
							value3.Parent.Controls.Remove(value3);
							toolStripPanel.Controls.Add(value3);
							toolStripPanel.Controls.SetChildIndex(value3, num++);
							if (value3.Height > num2)
							{
								num2 = value3.Height;
							}
							string[] array2 = item4.GetAttribute("Location").Split(',');
							int num4 = int.Parse(array2[0]);
							value3.Location = new Point(num4, num3);
							XmlNodeList childNodes = item4.ChildNodes;
							int num5 = 0;
							foreach (XmlElement item5 in childNodes)
							{
								string attribute2 = item5.GetAttribute("Name");
								if (!dictionary2.TryGetValue(attribute2, out var value4))
								{
									continue;
								}
								value4.Owner.Items.Remove(value4);
								value3.Items.Insert(num5, value4);
								string attribute3 = item5.GetAttribute("Visible");
								if (attribute3 == "false")
								{
									value4.Available = false;
								}
								else
								{
									value4.Available = true;
								}
								if (value4 is ToolStripButton)
								{
									ToolStripButton toolStripButton = value4 as ToolStripButton;
									string attribute4 = item5.GetAttribute("Checked");
									if (attribute4 == "true")
									{
										toolStripButton.Checked = true;
									}
								}
								num5++;
							}
						}
					}
				}
			}
			finally
			{
				foreach (ToolStrip value7 in dictionary.Values)
				{
					value7.ResumeLayout(performLayout: true);
				}
				m_toolStripContainer.TopToolStripPanel.ResumeLayout(performLayout: true);
				m_toolStripContainer.LeftToolStripPanel.ResumeLayout(performLayout: true);
				m_toolStripContainer.BottomToolStripPanel.ResumeLayout(performLayout: true);
				m_toolStripContainer.RightToolStripPanel.ResumeLayout(performLayout: true);
				m_toolStripContainer.ResumeLayout(performLayout: true);
				ResumeLayout(performLayout: false);
			}
		}
	}

	public event EventHandler Loading;

	public event EventHandler Loaded;

	public MainForm()
		: this(null)
	{
	}

	public MainForm(ToolStripContainer toolStripContainer)
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.Manual;
		m_mainFormBounds = base.Bounds;
		if (toolStripContainer != null)
		{
			m_toolStripContainer = toolStripContainer;
			m_toolStripContainer.Dock = DockStyle.Fill;
			base.Controls.Add(m_toolStripContainer);
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		bool flag = false;
		Control control = WinFormsUtil.GetFocusedControl();
		if ((!(control is TextBoxBase) && !(control is ComboBox)) || !KeysUtil.IsTextBoxInput(control, keyData))
		{
			if (m_commandService != null)
			{
				flag = m_commandService.ProcessKey(keyData);
			}
			if (!flag)
			{
				flag = base.ProcessCmdKey(ref msg, keyData);
			}
		}
		return flag;
	}

	protected override void OnLoad(EventArgs e)
	{
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => MainFormBounds, "MainFormBounds", null, null), new BoundPropertyDescriptor(this, () => MainFormWindowState, "MainFormWindowState", null, null));
			if (m_toolStripContainer != null)
			{
				m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => ToolStripContainerSettings, "ToolStripContainerSettings", null, null));
			}
		}
		base.OnLoad(e);
		m_mainFormLoaded = true;
		if (m_maximizeWindow)
		{
			base.WindowState = FormWindowState.Maximized;
		}
		this.Loading.Raise(this, e);
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		if (base.WindowState == FormWindowState.Minimized)
		{
			base.WindowState = ((m_previousWindoState != FormWindowState.Minimized) ? m_previousWindoState : FormWindowState.Normal);
		}
	}

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		this.Loaded.Raise(this, e);
	}

	private const int WM_SETREDRAW = 0x000B;

	[System.Runtime.InteropServices.DllImport("user32.dll")]
	internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

	protected override void OnSizeChanged(EventArgs e)
	{
		if (base.WindowState != FormWindowState.Minimized)
		{
			m_previousWindoState = base.WindowState;
		}
		ResizeTrace.Log("MainForm.OnSizeChanged");
		if (base.Visible)
		{
			SendMessage(base.Handle, WM_SETREDRAW, 0, 0);
		}
		base.OnSizeChanged(e);
		if (base.Visible)
		{
			SendMessage(base.Handle, WM_SETREDRAW, 1, 0);
			base.Invalidate(true);
		}
	}

	protected override void WndProc(ref Message m)
	{
		switch ((uint)m.Msg)
		{
		case 0x0231u:
			ResizeTrace.IsResizing = true;
			ResizeTrace.Log("WM_ENTERSIZEMOVE");
			break;
		case 0x0232u:
			ResizeTrace.IsResizing = false;
			ResizeTrace.Log("WM_EXITSIZEMOVE");
			break;
		case 0x0005u:
			if (ResizeTrace.IsResizing) ResizeTrace.Log("WM_SIZE");
			break;
		case 0x000Fu:
			if (ResizeTrace.IsResizing) ResizeTrace.Log("WM_PAINT");
			break;
		case 0x0085u:
			if (ResizeTrace.IsResizing) ResizeTrace.Log("WM_NCPAINT");
			break;
		case 0x0214u:
			if (ResizeTrace.IsResizing) ResizeTrace.Log("WM_SIZING");
			break;
		}
		base.WndProc(ref m);
	}

	protected override void OnLocationChanged(EventArgs e)
	{
		SetBounds();
		base.OnLocationChanged(e);
	}

	private void SetBounds()
	{
		if (base.WindowState == FormWindowState.Normal)
		{
			m_mainFormBounds = base.Bounds;
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

	private void PrepareLoadPanelState(ToolStripPanel panel, Dictionary<string, ToolStrip> toolStrips, Dictionary<string, ToolStripItem> toolStripItems)
	{
		panel.SuspendLayout();
		foreach (ToolStrip control in panel.Controls)
		{
			if (!control.Visible)
			{
				continue;
			}
			toolStrips.Add(control.Name, control);
			control.SuspendLayout();
			foreach (ToolStripItem item in control.Items)
			{
				toolStripItems.Add(item.Name, item);
			}
		}
	}

	private static void SavePanelState(ToolStripPanel panel, string panelName, XmlDocument xmlDoc, XmlElement root)
	{
		XmlElement xmlElement = xmlDoc.CreateElement("ToolStripPanel");
		root.AppendChild(xmlElement);
		xmlElement.SetAttribute("Name", panelName);
		foreach (ToolStrip control in panel.Controls)
		{
			if (!control.Visible || control.Name == null || control.Name.Trim().Length == 0)
			{
				continue;
			}
			XmlElement xmlElement2 = xmlDoc.CreateElement("ToolStrip");
			xmlElement.AppendChild(xmlElement2);
			xmlElement2.SetAttribute("Name", control.Name);
			xmlElement2.SetAttribute("Location", $"{control.Location.X},{control.Location.Y}");
			if (control is MenuStrip)
			{
				continue;
			}
			foreach (ToolStripItem item in control.Items)
			{
				if (item.Name != null && item.Name.Trim().Length != 0)
				{
					XmlElement xmlElement3 = xmlDoc.CreateElement("ToolStripItem");
					xmlElement2.AppendChild(xmlElement3);
					xmlElement3.SetAttribute("Name", item.Name);
					if (!(item is ToolStripDropDownButton) && item.Placement == ToolStripItemPlacement.None && !item.Available)
					{
						xmlElement3.SetAttribute("Visible", "false");
					}
					if (item is ToolStripButton && ((ToolStripButton)item).Checked)
					{
						xmlElement3.SetAttribute("Checked", "true");
					}
				}
			}
		}
	}

	private int ToolStripWidth(ToolStrip toolStrip)
	{
		int num = 0;
		foreach (ToolStripItem item in toolStrip.Items)
		{
			if (item.Visible)
			{
				num += item.Width;
			}
		}
		return num;
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Applications.MainForm));
		base.SuspendLayout();
		this.BackColor = System.Drawing.SystemColors.Control;
		resources.ApplyResources(this, "$this");
		base.KeyPreview = true;
		base.Name = "MainForm";
		base.ResumeLayout(false);
	}

	void IMainWindow.Close()
	{
		Close();
	}
}
