using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Export(typeof(IInitializable))]
[Export(typeof(ITargetConsumer))]
[Export(typeof(TargetEnumerationService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TargetEnumerationService : IControlHostClient, ITargetConsumer, IInitializable
{
	private class DummmyTargetInfo : TargetInfo
	{
	}

	[ImportMany]
	private IEnumerable<ITargetProvider> m_targetProviders = null;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[ImportMany]
	private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

	private List<IContextMenuCommandProvider> m_actualContextMenuCommandProviders;

	private const string AddString = "Add Target";

	private const string AddNewString = "Add New ";

	private IControlHostService m_controlHostService;

	private SortableBindingList<TargetInfo> m_targets = new SortableBindingList<TargetInfo>();

	private List<TargetInfo> m_targetsToSelect = new List<TargetInfo>();

	private List<TargetInfo> m_targetsPicked = new List<TargetInfo>();

	private List<TargetInfo> m_targetsLastChecked = new List<TargetInfo>();

	private Multimap<ITargetProvider, TargetInfo> m_providerTargets = new Multimap<ITargetProvider, TargetInfo>();

	private DataBoundListView m_listView;

	private SplitButton m_addTargetButton;

	private Button m_okButton;

	private UserControl m_userControl;

	private bool m_enabled = true;

	private static bool s_showAsDialog = false;

	public static bool ShowAsDialog
	{
		get
		{
			return s_showAsDialog;
		}
		set
		{
			s_showAsDialog = value;
		}
	}

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
			m_userControl.Enabled = m_enabled;
		}
	}

	public IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
	{
		get
		{
			if (m_actualContextMenuCommandProviders != null)
			{
				return m_actualContextMenuCommandProviders;
			}
			if (m_contextMenuCommandProviders != null)
			{
				m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(m_contextMenuCommandProviders.GetValues());
				return m_actualContextMenuCommandProviders;
			}
			return EmptyEnumerable<IContextMenuCommandProvider>.Instance;
		}
		set
		{
			m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(value);
		}
	}

	public string PersistedUISettings
	{
		get
		{
			return m_listView.Settings;
		}
		set
		{
			m_listView.Settings = value;
		}
	}

	public string PersistedSelectedTargets
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("SelectedTargets");
			xmlDocument.AppendChild(xmlElement);
			for (int i = 0; i < m_listView.Items.Count; i++)
			{
				if (m_listView.Items[i].Checked)
				{
					TargetInfo targetInfo = m_targets[i];
					XmlElement xmlElement2 = xmlDocument.CreateElement("Target");
					xmlElement2.SetAttribute("name", targetInfo.Name);
					xmlElement2.SetAttribute("platform", targetInfo.Platform);
					xmlElement2.SetAttribute("endpoint", targetInfo.Endpoint);
					xmlElement2.SetAttribute("protocol", targetInfo.Protocol);
					xmlElement2.SetAttribute("scope", targetInfo.Scope.ToString());
					xmlElement.AppendChild(xmlElement2);
				}
			}
			if (xmlDocument.DocumentElement.ChildNodes.Count == 0)
			{
				xmlDocument.RemoveAll();
			}
			return xmlDocument.InnerXml;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("Target");
			if (xmlNodeList == null || xmlNodeList.Count == 0)
			{
				return;
			}
			foreach (XmlElement item2 in xmlNodeList)
			{
				DummmyTargetInfo dummmyTargetInfo = new DummmyTargetInfo();
				dummmyTargetInfo.Name = item2.GetAttribute("name");
				dummmyTargetInfo.Endpoint = item2.GetAttribute("endpoint");
				dummmyTargetInfo.Protocol = item2.GetAttribute("protocol");
				dummmyTargetInfo.Platform = item2.GetAttribute("platform");
				dummmyTargetInfo.Scope = (TargetScope)Enum.Parse(typeof(TargetScope), item2.GetAttribute("scope"));
				DummmyTargetInfo item = dummmyTargetInfo;
				m_targetsToSelect.Add(item);
				SelectTargets(m_targetsToSelect, valueEqual: true);
			}
		}
	}

	public IEnumerable<TargetInfo> AllTargets
	{
		get
		{
			foreach (TargetInfo target in m_targets)
			{
				yield return target;
			}
		}
	}

	public IEnumerable<TargetInfo> SelectedTargets
	{
		get
		{
			int index = 0;
			while (index < m_listView.Items.Count)
			{
				if (m_listView.Items[index] != null && m_listView.Items[index].Checked)
				{
					yield return m_targets[index];
				}
				int num = index + 1;
				index = num;
			}
		}
		set
		{
			SelectTargets(value, valueEqual: false);
		}
	}

	public IEnumerable<ITargetProvider> TargetProviders
	{
		get
		{
			return m_targetProviders;
		}
		set
		{
			m_targetProviders = value;
		}
	}

	[Import(AllowDefault = true)]
	protected Form MainForm { get; set; }

	[Import(AllowDefault = true)]
	public ICommandService CommandService { get; set; }

	public event EventHandler<SelectedTargetsChangedArgs> SelectedTargetsChanged;

	[ImportingConstructor]
	public TargetEnumerationService(IControlHostService controlHostService)
	{
		m_controlHostService = controlHostService;
	}

	public TargetEnumerationService()
	{
	}

	void IInitializable.Initialize()
	{
		IntPtr handle = MainForm.Handle;
		Control control = SetUpTargetsView();
		if (!ShowAsDialog)
		{
			m_controlHostService.RegisterControl(control, new ControlInfo("Targets".Localize(), "Controls for managing targets.".Localize(), StandardControlGroup.Bottom), this);
		}
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => PersistedUISettings, "UI Settings".Localize(), null, null));
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => PersistedSelectedTargets, "Selected Targets".Localize(), null, null));
		}
	}

	public void ShowDialog(string title)
	{
		if (Enabled)
		{
			if (title == null)
			{
				title = "Targets".Localize();
			}
			Form form = new Form();
			form.ShowIcon = false;
			form.ShowInTaskbar = false;
			form.MaximizeBox = false;
			form.MinimizeBox = false;
			form.Controls.Add(m_userControl);
			form.Text = title;
			form.ShowDialog();
		}
	}

	public void TargetsChanged(ITargetProvider targetProvider, IEnumerable<TargetInfo> targets)
	{
		if (MainForm == null || !MainForm.IsHandleCreated || MainForm.IsDisposed)
		{
			UpdateTargetsView(targetProvider, targets);
			return;
		}
		MainForm.Invoke((MethodInvoker)delegate
		{
			UpdateTargetsView(targetProvider, targets);
		});
	}

	public void Activate(Control control)
	{
		m_userControl.ActiveControl = m_listView;
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		return true;
	}

	private void SelectTargets(IEnumerable<TargetInfo> targets, bool valueEqual)
	{
		if (MainForm != null && MainForm.IsHandleCreated && !MainForm.IsDisposed)
		{
			MainForm.Invoke((MethodInvoker)delegate
			{
				ListViewSelectTargets(targets, valueEqual);
			});
		}
	}

	protected Control SetUpTargetsView()
	{
		m_userControl = new UserControl
		{
			Margin = new Padding(3)
		};
		m_userControl.Dock = DockStyle.Fill;
		m_userControl.AutoSize = true;
		m_addTargetButton = new SplitButton();
		m_addTargetButton.Text = "Add Target".Localize();
		m_addTargetButton.Location = new Point(m_userControl.Margin.Left, m_userControl.Height - m_userControl.Margin.Bottom - m_addTargetButton.Height - m_addTargetButton.Margin.Size.Height);
		m_addTargetButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
		m_addTargetButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		m_listView = new DataBoundListView();
		m_listView.DataSource = m_targets;
		m_listView.BindingContext = m_userControl.BindingContext;
		m_listView.AlternatingRowColors = true;
		m_listView.MultiSelect = false;
		m_listView.CheckBoxes = true;
		m_listView.Location = new Point(m_userControl.Margin.Left, m_userControl.Margin.Top);
		m_listView.Size = new Size(m_userControl.Width - m_userControl.Margin.Left - m_userControl.Margin.Right, m_userControl.Height - m_userControl.Margin.Top - m_userControl.Margin.Bottom - m_addTargetButton.Height - m_addTargetButton.Margin.Top - m_addTargetButton.Margin.Size.Height);
		m_listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_listView.Name = "targetsListView";
		m_listView.CellValidating += listView_CellValidating;
		m_listView.ItemChecked += listView_ItemChecked;
		m_listView.ItemCheck += listView_ItemCheck;
		m_listView.MouseUp += listView_MouseUp;
		m_userControl.Controls.Add(m_listView);
		m_userControl.Controls.Add(m_addTargetButton);
		if (ShowAsDialog)
		{
			m_okButton = new Button();
			m_okButton.Name = "m_okButton";
			m_okButton.Text = "OK";
			m_okButton.DialogResult = DialogResult.OK;
			m_okButton.Location = new Point(m_userControl.Width - m_userControl.Margin.Right - m_okButton.Width, m_userControl.Height - m_userControl.Margin.Bottom - m_okButton.Height - m_okButton.Margin.Size.Height);
			m_okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			m_userControl.Controls.Add(m_okButton);
		}
		m_addTargetButton.ShowSplit = true;
		m_addTargetButton.ContextMenuStrip = new ContextMenuStrip();
		foreach (ITargetProvider targetProvider in TargetProviders)
		{
			if (targetProvider.CanCreateNew)
			{
				m_addTargetButton.ContextMenuStrip.Items.Add("Add New " + targetProvider.Name);
			}
		}
		m_addTargetButton.ContextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
		if (m_addTargetButton.ContextMenuStrip.Items.Count == 1)
		{
			m_addTargetButton.ShowSplit = false;
			ToolStripItem toolStripItem = m_addTargetButton.ContextMenuStrip.Items[0];
			m_addTargetButton.Text = toolStripItem.Text;
			m_addTargetButton.Click += delegate
			{
				foreach (ITargetProvider targetProvider2 in TargetProviders)
				{
					if ("Add New " + targetProvider2.Name == m_addTargetButton.Text)
					{
						targetProvider2.AddTarget(targetProvider2.CreateNew());
						break;
					}
				}
			};
		}
		m_userControl.Name = "Targets".Localize();
		return m_userControl;
	}

	private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		foreach (ITargetProvider targetProvider in TargetProviders)
		{
			if ("Add New " + targetProvider.Name == e.ClickedItem.Text)
			{
				targetProvider.AddTarget(targetProvider.CreateNew());
				break;
			}
		}
	}

	private void listView_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Right)
		{
			ContextMenuCommandProviders.GetCommands(this, null);
		}
		else
		{
			if (CommandService == null || ContextMenuCommandProviders == null)
			{
				return;
			}
			m_targetsPicked.Clear();
			foreach (int selectedIndex in m_listView.SelectedIndices)
			{
				m_targetsPicked.Add(m_targets[selectedIndex]);
			}
			IEnumerable<object> commands = ContextMenuCommandProviders.GetCommands(this, m_targetsPicked);
			Point screenPoint = m_listView.PointToScreen(new Point(e.X, e.Y));
			CommandService.RunContextMenu(commands, screenPoint);
		}
	}

	private void listView_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		if (!m_listView.SortingItems)
		{
			if (m_listView.MultiSelect)
			{
				m_targetsLastChecked = SelectedTargets.ToList();
			}
			else if (e.NewValue == CheckState.Checked)
			{
				m_targetsLastChecked = SelectedTargets.ToList();
			}
		}
	}

	private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		if (m_listView.SortingItems)
		{
			return;
		}
		if (m_listView.MultiSelect)
		{
			List<TargetInfo> source = SelectedTargets.ToList();
			if (!m_targetsLastChecked.OrderBy((TargetInfo x) => x.Endpoint).SequenceEqual(source.OrderBy((TargetInfo x) => x.Endpoint)))
			{
				OnSelectedTargetsChanged(new SelectedTargetsChangedArgs(m_targetsLastChecked, SelectedTargets));
			}
		}
		else
		{
			if (!e.Item.Checked)
			{
				return;
			}
			foreach (ListViewItem item in m_listView.Items)
			{
				if (item != null && item.Checked && item != e.Item)
				{
					item.Checked = false;
				}
			}
			List<TargetInfo> source2 = SelectedTargets.ToList();
			if (!m_targetsLastChecked.OrderBy((TargetInfo x) => x.Endpoint).SequenceEqual(source2.OrderBy((TargetInfo x) => x.Endpoint)))
			{
				OnSelectedTargetsChanged(new SelectedTargetsChangedArgs(m_targetsLastChecked, SelectedTargets));
			}
		}
	}

	protected virtual void OnSelectedTargetsChanged(SelectedTargetsChangedArgs e)
	{
		this.SelectedTargetsChanged.Raise(this, e);
	}

	private void UpdateTargetsView(ITargetProvider targetProvider, IEnumerable<TargetInfo> targets)
	{
		List<TargetInfo> list = new List<TargetInfo>();
		List<TargetInfo> list2 = new List<TargetInfo>();
		List<TargetInfo> list3 = new List<TargetInfo>();
		m_providerTargets.Remove(targetProvider);
		foreach (TargetInfo target2 in targets)
		{
			m_providerTargets.Add(targetProvider, target2);
			if (!m_targets.Contains(target2))
			{
				list.Add(target2);
			}
		}
		if (list.Count > 3)
		{
			m_targets.AddRange(list);
		}
		else
		{
			foreach (TargetInfo item in list)
			{
				m_targets.Add(item);
			}
		}
		foreach (TargetInfo target in m_targets)
		{
			bool flag = true;
			foreach (ITargetProvider key in m_providerTargets.Keys)
			{
				if (m_providerTargets.ContainsKeyValue(key, target))
				{
					flag = false;
				}
				TargetInfo targetInfo = m_targetsToSelect.FirstOrDefault((TargetInfo n) => n.Scope == target.Scope && n.Protocol == target.Protocol && n.Name == target.Name && n.Endpoint == target.Endpoint);
				if (targetInfo != null)
				{
					list3.Add(target);
					m_targetsToSelect.Remove(targetInfo);
				}
			}
			if (flag)
			{
				list2.Add(target);
			}
		}
		foreach (TargetInfo item2 in list2)
		{
			m_targets.Remove(item2);
		}
		if (list2.Count > 0)
		{
			m_listView.Refresh();
		}
		if (list3.Count > 0)
		{
			ListViewSelectTargets(list3, valueEqual: true);
		}
	}

	private void ListViewSelectTargets(IEnumerable<TargetInfo> targets, bool valueEqual)
	{
		List<TargetInfo> list = null;
		if (valueEqual)
		{
			list = new List<TargetInfo>();
			foreach (TargetInfo target in m_targets)
			{
				TargetInfo targetInfo = targets.FirstOrDefault((TargetInfo n) => n.Scope == target.Scope && n.Protocol == target.Protocol && n.Name == target.Name && n.Endpoint == target.Endpoint);
				if (targetInfo != null)
				{
					list.Add(target);
					m_targetsToSelect.RemoveAll((TargetInfo n) => n.Scope == target.Scope && n.Protocol == target.Protocol && n.Name == target.Name && n.Endpoint == target.Endpoint);
					break;
				}
			}
		}
		else
		{
			list = targets.ToList();
			m_targetsToSelect.Clear();
		}
		foreach (TargetInfo item in list)
		{
			int num = m_targets.IndexOf(item);
			if (num != -1 && num < m_listView.Items.Count)
			{
				m_listView.Items[num].Checked = true;
			}
		}
	}

	private void listView_CellValidating(object sender, DataBoundListView.ListViewCellValidatingEventArgs e)
	{
		string name = m_listView.ItemProperties[e.ColumnIndex].Name;
		TargetInfo targetInfo = m_targets[e.RowIndex];
		if (targetInfo is IPropertyValueValidator)
		{
			e.Cancel = !((IPropertyValueValidator)targetInfo).Validate(name, e.FormattedValue, out var errorMessage);
			if (e.Cancel)
			{
				Outputs.WriteLine(OutputMessageType.Warning, errorMessage);
			}
		}
	}
}
