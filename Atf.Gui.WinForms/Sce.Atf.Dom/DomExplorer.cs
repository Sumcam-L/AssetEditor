using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Dom;

[Export(typeof(IInitializable))]
[Export(typeof(DomExplorer))]
[PartCreationPolicy(CreationPolicy.Any)]
public class DomExplorer : IControlHostClient, IInitializable
{
	private class TreeView : ITreeView, IItemView, IObservableContext
	{
		private DomNode m_root;

		private int m_lastRemoveIndex;

		private bool m_showAdapters = true;

		public DomNode RootNode
		{
			get
			{
				return m_root;
			}
			set
			{
				if (m_root != null)
				{
					m_root.AttributeChanged -= root_AttributeChanged;
					m_root.ChildInserted -= root_ChildInserted;
					m_root.ChildRemoving -= root_ChildRemoving;
					m_root.ChildRemoved -= root_ChildRemoved;
				}
				m_root = value;
				if (m_root != null)
				{
					m_root.AttributeChanged += root_AttributeChanged;
					m_root.ChildInserted += root_ChildInserted;
					m_root.ChildRemoving += root_ChildRemoving;
					m_root.ChildRemoved += root_ChildRemoved;
				}
				this.Reloaded.Raise(this, EventArgs.Empty);
			}
		}

		public bool ShowAdapters
		{
			get
			{
				return m_showAdapters;
			}
			set
			{
				m_showAdapters = value;
			}
		}

		public object Root => m_root;

		public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

		public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

		public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

		public event EventHandler Reloaded;

		public IEnumerable<object> GetChildren(object parent)
		{
			if (!(parent is DomNode node))
			{
				yield break;
			}
			if (m_showAdapters)
			{
				IEnumerable<DomNodeAdapter> adapters = node.AsAll<DomNodeAdapter>();
				foreach (DomNodeAdapter adapter in adapters)
				{
					yield return new Adapter(adapter);
				}
			}
			foreach (DomNode child in node.Children)
			{
				yield return child;
			}
		}

		public void GetInfo(object item, ItemInfo info)
		{
			info.IsLeaf = !HasChildren(item);
			if (item is DomNode { ChildInfo: not null } domNode)
			{
				info.Label = domNode.ChildInfo.Name;
			}
			else if (item is Adapter adapter)
			{
				DomNodeAdapter domNodeAdapter = adapter.Adaptee as DomNodeAdapter;
				StringBuilder stringBuilder = new StringBuilder();
				Type type = domNodeAdapter.GetType();
				stringBuilder.Append(type.Name);
				stringBuilder.Append(" (");
				Type[] interfaces = type.GetInterfaces();
				foreach (Type type2 in interfaces)
				{
					stringBuilder.Append(type2.Name);
					stringBuilder.Append(",");
				}
				stringBuilder[stringBuilder.Length - 1] = ')';
				info.Label = stringBuilder.ToString();
			}
		}

		public bool HasChildren(object item)
		{
			using (IEnumerator<object> enumerator = GetChildren(item).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					return true;
				}
			}
			return false;
		}

		private void root_AttributeChanged(object sender, AttributeEventArgs e)
		{
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}

		private void root_ChildInserted(object sender, ChildEventArgs e)
		{
			int childIndex = GetChildIndex(e.Child, e.Parent);
			if (childIndex >= 0)
			{
				this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(childIndex, e.Child, e.Parent));
			}
		}

		private void root_ChildRemoving(object sender, ChildEventArgs e)
		{
			m_lastRemoveIndex = GetChildIndex(e.Child, e.Parent);
		}

		private void root_ChildRemoved(object sender, ChildEventArgs e)
		{
			if (m_lastRemoveIndex >= 0)
			{
				this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(m_lastRemoveIndex, e.Child, e.Parent));
			}
		}

		private int GetChildIndex(object child, object parent)
		{
			IEnumerable<object> children = GetChildren(parent);
			int num = 0;
			foreach (object item in children)
			{
				if (item.Equals(child))
				{
					return num;
				}
				num++;
			}
			return -1;
		}
	}

	private readonly IControlHostService m_controlHostService;

	private readonly TreeControl m_treeControl;

	private readonly SplitContainer m_splitContainer;

	private readonly TreeControlAdapter m_treeControlAdapter;

	private readonly Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;

	private readonly TreeView m_treeView;

	public DomNode Root
	{
		get
		{
			return m_treeView.RootNode;
		}
		set
		{
			if (value != null)
			{
				m_treeView.RootNode = value;
				m_treeControlAdapter.TreeView = m_treeView;
			}
			else
			{
				m_treeControlAdapter.TreeView = null;
				m_treeView.RootNode = null;
			}
		}
	}

	public bool ShowAdapters
	{
		get
		{
			return m_treeView.ShowAdapters;
		}
		set
		{
			m_treeView.ShowAdapters = value;
		}
	}

	public TreeControl TreeControl => m_treeControl;

	public TreeControlAdapter TreeControlAdapter => m_treeControlAdapter;

	[ImportingConstructor]
	public DomExplorer(IControlHostService controlHostService)
	{
		m_controlHostService = controlHostService;
		m_treeControl = new TreeControl();
		m_treeControl.Dock = DockStyle.Fill;
		m_treeControl.AllowDrop = true;
		m_treeControl.SelectionMode = SelectionMode.MultiExtended;
		m_treeControl.ImageList = ResourceUtil.GetImageList16();
		m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;
		m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
		m_treeView = new TreeView();
		m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
		m_propertyGrid.Dock = DockStyle.Fill;
		m_splitContainer = new SplitContainer();
		m_splitContainer.Text = "Dom Explorer";
		m_splitContainer.Panel1.Controls.Add(m_treeControl);
		m_splitContainer.Panel2.Controls.Add(m_propertyGrid);
	}

	public virtual void Initialize()
	{
		ControlInfo controlInfo = new ControlInfo("DOM Explorer".Localize(), "Generic View of DOM".Localize(), StandardControlGroup.Bottom);
		controlInfo.ControlVisibility = ControlInitialVisibility.AlwaysHidden;
		controlInfo.MenuText = "Tool Debug\\DOM Explorer";
		controlInfo.MenuGroupOverride = StandardCommandGroup.UILayout;
		m_controlHostService.RegisterControl(m_splitContainer, controlInfo, this);
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

	private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
	{
		if (!e.Node.Selected)
		{
			return;
		}
		object tag = e.Node.Tag;
		if (tag is DomNode domNode)
		{
			List<PropertyDescriptor> list = new List<PropertyDescriptor>();
			foreach (AttributeInfo attribute in domNode.Type.Attributes)
			{
				list.Add(new AttributePropertyDescriptor(attribute.Name, attribute, "Attributes", null, isReadOnly: true));
			}
			m_propertyGrid.Bind(new PropertyCollectionWrapper(list.ToArray(), domNode));
		}
		else
		{
			DomNodeAdapter selectedObject = tag as DomNodeAdapter;
			m_propertyGrid.Bind(selectedObject);
		}
	}
}
