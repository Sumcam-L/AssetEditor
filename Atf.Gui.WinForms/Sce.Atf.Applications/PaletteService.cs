using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

[Export(typeof(IPaletteService))]
[Export(typeof(IInitializable))]
[Export(typeof(PaletteService))]
[PartCreationPolicy(CreationPolicy.Any)]
public class PaletteService : TreeControlEditor, IPaletteService, IControlHostClient, IInitializable
{
	private class PaletteTreeAdapter : ITreeView, IItemView, IObservableContext
	{
		private readonly PaletteService m_paletteService;

		private readonly StringSearchInputUI m_searchInput;

		private readonly SortedDictionary<string, List<object>> m_categories;

		public object Root => this;

		public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

		public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

		public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

		public event EventHandler Reloaded;

		public PaletteTreeAdapter(PaletteService paletteService, StringSearchInputUI searchInput, IComparer<string> categoryComparer)
		{
			m_paletteService = paletteService;
			m_searchInput = searchInput;
			m_categories = new SortedDictionary<string, List<object>>(categoryComparer);
			if (this.ItemChanged != null && this.Reloaded != null)
			{
			}
		}

		public void AddItem(object item, string categoryName)
		{
			int count;
			if (!m_categories.TryGetValue(categoryName, out var value))
			{
				count = m_categories.Count;
				value = new List<object>();
				m_categories.Add(categoryName, value);
				this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(count, categoryName, this));
			}
			count = value.Count;
			value.Add(item);
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(count, item, categoryName));
		}

		public void RemoveItem(object item)
		{
			foreach (KeyValuePair<string, List<object>> category in m_categories)
			{
				int num = category.Value.IndexOf(item);
				if (num >= 0)
				{
					category.Value.RemoveAt(num);
					this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(num, item, category.Key));
					break;
				}
			}
		}

		public void RemoveAllItems()
		{
			m_categories.Clear();
		}

		public void RefreshControl()
		{
			this.Reloaded.Raise(this, null);
		}

		public IEnumerable<object> GetChildren(object parent)
		{
			if (parent == this)
			{
				foreach (string key in m_categories.Keys)
				{
					yield return key;
				}
			}
			else
			{
				if (!(parent is string categoryName) || !m_categories.TryGetValue(categoryName, out var category))
				{
					yield break;
				}
				foreach (object item in category)
				{
					ItemInfo info = new WinFormsItemInfo();
					GetInfo(item, info);
					if (m_searchInput.IsNullOrEmpty() || m_searchInput.Matches(info.Label))
					{
						yield return item;
					}
				}
			}
		}

		public void GetInfo(object item, ItemInfo info)
		{
			info.AllowLabelEdit = false;
			if (item != this)
			{
				if (item is string text && m_categories.ContainsKey(text))
				{
					info.Label = text;
					return;
				}
				IPaletteClient paletteClient = m_paletteService.m_objectClients[item];
				paletteClient.GetInfo(item, info);
			}
		}
	}

	private IComparer<string> m_categoryComparer;

	private readonly IControlHostService m_controlHostService;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	[Import(AllowDefault = true)]
	private IMainWindow m_mainWindow;

	[Import(AllowDefault = true)]
	private Form m_mainForm;

	private readonly UserControl m_control;

	private readonly StringSearchInputUI m_searchInput;

	private bool m_searching;

	private bool m_persistExpandedCategories = true;

	private readonly List<string> m_expandedCollections = new List<string>();

	private PaletteTreeAdapter m_paletteTreeAdapter;

	private readonly Dictionary<object, IPaletteClient> m_objectClients = new Dictionary<object, IPaletteClient>();

	public IComparer<string> CategoryComparer
	{
		get
		{
			return m_categoryComparer;
		}
		set
		{
			if (value != m_categoryComparer)
			{
				if (m_paletteTreeAdapter != null)
				{
					throw new InvalidOperationException("CategoryComparer can only be set before palette items are added");
				}
				m_categoryComparer = value;
			}
		}
	}

	internal string ExpandedCategories
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("Categories");
			xmlDocument.AppendChild(xmlElement);
			Tree<object> expansion = base.TreeControlAdapter.GetExpansion();
			foreach (Tree<object> child in expansion.Children)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("Category");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("Name", (string)child.Value);
				xmlElement2.SetAttribute("Expanded", (!child.IsLeaf).ToString());
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
			XmlElement documentElement = xmlDocument.DocumentElement;
			if (documentElement == null || documentElement.Name != "Categories")
			{
				throw new Exception("Invalid DomPalette settings");
			}
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Category");
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute = item.GetAttribute("Name");
				string attribute2 = item.GetAttribute("Expanded");
				if (bool.TryParse(attribute2, out var result))
				{
					if (result)
					{
						base.TreeControlAdapter.Expand(attribute);
					}
					else
					{
						base.TreeControlAdapter.Collapse(attribute);
					}
				}
			}
		}
	}

	private PaletteTreeAdapter TreeAdapter
	{
		get
		{
			if (m_paletteTreeAdapter == null)
			{
				m_paletteTreeAdapter = new PaletteTreeAdapter(this, m_searchInput, CategoryComparer);
			}
			return m_paletteTreeAdapter;
		}
	}

	public bool PersistExpandedCategories
	{
		get
		{
			return m_persistExpandedCategories;
		}
		set
		{
			m_persistExpandedCategories = value;
		}
	}

	[ImportingConstructor]
	public PaletteService(ICommandService commandService, IControlHostService controlHostService)
		: base(commandService)
	{
		m_controlHostService = controlHostService;
		m_searchInput = new StringSearchInputUI();
		m_searchInput.Updated += searchInput_Updated;
		m_control = new UserControl();
		m_control.Dock = DockStyle.Fill;
		m_control.SuspendLayout();
		m_control.Name = "Palette".Localize();
		m_control.Text = "Palette".Localize();
		m_control.Controls.Add(m_searchInput);
		m_control.Controls.Add(base.TreeControl);
		m_control.Layout += controls_Layout;
		m_control.ResumeLayout();
		m_controlHostService.RegisterControl(m_control, new ControlInfo("Palette".Localize(), "Creates new instances".Localize(), StandardControlGroup.Left, "https://github.com/SonyWWS/ATF/search?utf8=%E2%9C%93&q=PaletteService+or+Palette".Localize()), this);
	}

	protected override void Configure(out TreeControl treeControl, out TreeControlAdapter treeControlAdapter)
	{
		treeControl = new TreeControl(TreeControl.Style.CategorizedPalette);
		treeControl.ImageList = ResourceUtil.GetImageList16();
		treeControl.AllowDrop = true;
		treeControl.SelectionMode = SelectionMode.MultiExtended;
		treeControlAdapter = new TreeControlAdapter(treeControl);
	}

	void IInitializable.Initialize()
	{
		if (m_mainWindow == null && m_mainForm != null)
		{
			m_mainWindow = new MainFormAdapter(m_mainForm);
		}
		if (m_mainWindow == null)
		{
			throw new InvalidOperationException("Can't get main window");
		}
		m_mainWindow.Loading += mainWindow_Loaded;
	}

	public void AddItem(object item, string categoryName, IPaletteClient client)
	{
		if (m_objectClients.ContainsKey(item))
		{
			throw new InvalidOperationException("duplicate item");
		}
		if (categoryName != null)
		{
			m_objectClients.Add(item, client);
			TreeAdapter.AddItem(item, categoryName);
		}
	}

	public void RemoveItem(object item)
	{
		TreeAdapter.RemoveItem(item);
		m_objectClients.Remove(item);
	}

	public void RemoveAllItems()
	{
		m_searchInput.ClearSearch();
		TreeAdapter.RemoveAllItems();
		m_objectClients.Clear();
		base.TreeControl.Root.Clear();
		TreeAdapter.RefreshControl();
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	private void controls_Layout(object sender, LayoutEventArgs e)
	{
		int num = (m_searchInput.Visible ? m_searchInput.Height : 0);
		base.TreeControl.Bounds = new Rectangle(0, num, m_control.Width, m_control.Height - num);
	}

	protected override void OnStartDrag(IEnumerable<object> items)
	{
		List<object> list = new List<object>();
		foreach (object item in items)
		{
			if (m_objectClients.TryGetValue(item, out var value))
			{
				object obj = value.Convert(item);
				if (obj != null)
				{
					list.Add(obj);
				}
			}
		}
		if (list.Count > 0)
		{
			base.TreeControl.DoDragDrop(list.ToArray(), DragDropEffects.All);
		}
	}

	private void mainWindow_Loaded(object sender, EventArgs e)
	{
		base.TreeView = TreeAdapter;
		if (PersistExpandedCategories && m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => ExpandedCategories, "ExpandedCategories", null, null));
		}
		else
		{
			base.TreeControl.ExpandAll();
		}
	}

	private void searchInput_Updated(object sender, EventArgs e)
	{
		if (base.TreeControl.Root == null)
		{
			return;
		}
		if (m_searchInput.IsNullOrEmpty())
		{
			if (m_searching)
			{
				TreeAdapter.RefreshControl();
				RestoreExpansion();
			}
			m_searching = false;
			return;
		}
		if (!m_searching)
		{
			RememberExpansion();
		}
		m_searching = true;
		TreeAdapter.RefreshControl();
		foreach (object child in TreeAdapter.GetChildren(TreeAdapter))
		{
			foreach (object child2 in TreeAdapter.GetChildren(child))
			{
				ItemInfo itemInfo = new WinFormsItemInfo();
				TreeAdapter.GetInfo(child2, itemInfo);
				if (m_searchInput.Matches(itemInfo.Label))
				{
					base.TreeControlAdapter.Expand(child);
					break;
				}
			}
		}
		RestoreExpansion();
	}

	private void RememberExpansion()
	{
		m_expandedCollections.Clear();
		foreach (string child in TreeAdapter.GetChildren(TreeAdapter))
		{
			if (base.TreeControlAdapter.IsExpanded(child))
			{
				m_expandedCollections.Add(child);
			}
		}
	}

	private void RestoreExpansion()
	{
		foreach (string expandedCollection in m_expandedCollections)
		{
			base.TreeControlAdapter.Expand(expandedCollection);
		}
	}
}
