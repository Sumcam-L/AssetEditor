using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class LayeringContext : SelectionContext, IInstancingContext, IHierarchicalInsertionContext, ILayeringContext, IVisibilityContext, ITreeView, IItemView, IObservableContext, INamingContext
{
	private object m_activeItem;

	protected abstract AttributeInfo VisibleAttribute { get; }

	protected abstract ChildInfo LayerFolderChildInfo { get; }

	protected abstract DomNodeType LayerFolderType { get; }

	protected abstract DomNodeType ElementRefType { get; }

	public IList<LayerFolder> Layers => GetChildList<LayerFolder>(LayerFolderChildInfo);

	object ITreeView.Root => this;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded
	{
		add
		{
		}
		remove
		{
		}
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		m_activeItem = this;
	}

	public bool CanCopy()
	{
		return base.Selection.Count > 0;
	}

	public object Copy()
	{
		return this.Cast<CircuitEditingContext>().Copy();
	}

	public bool CanInsert(object insertingObject)
	{
		return CanInsert(m_activeItem, insertingObject);
	}

	public void Insert(object insertingObject)
	{
		Insert(m_activeItem, insertingObject);
	}

	public bool CanDelete()
	{
		foreach (DomNode item in GetSelection<DomNode>())
		{
			if (item.Parent != null)
			{
				return true;
			}
		}
		return false;
	}

	public void Delete()
	{
		IEnumerable<DomNode> selection = GetSelection<DomNode>();
		foreach (DomNode root in DomNode.GetRoots(selection))
		{
			SetVisible(root, value: true);
			root.RemoveFromParent();
		}
	}

	public bool CanInsert(object parent, object insertingObject)
	{
		if (parent == null)
		{
			parent = m_activeItem ?? this;
		}
		else if (!(parent is LayeringContext) && !(parent is LayerFolder))
		{
			return false;
		}
		IEnumerable<DomNode> compatibleNodes = GetCompatibleNodes((IDataObject)insertingObject);
		if (compatibleNodes == null)
		{
			return false;
		}
		DomNode domNode = parent.As<DomNode>();
		if (domNode == null)
		{
			return false;
		}
		foreach (DomNode item in compatibleNodes)
		{
			if (!IsLayerItem(item) || base.DomNode == null || item.GetRoot() != base.DomNode.GetRoot())
			{
				return false;
			}
			foreach (DomNode item2 in domNode.Lineage)
			{
				if (item.Equals(item2))
				{
					return false;
				}
			}
			if (domNode != base.DomNode && item.Parent == domNode)
			{
				return false;
			}
		}
		return true;
	}

	public LayerFolder InsertAuto(object parent, object objectToInsert)
	{
		DataObject dataObject = null;
		if (objectToInsert is IEnumerable)
		{
			List<object> list = new List<object>();
			foreach (object item in (IEnumerable)objectToInsert)
			{
				list.Add(item);
			}
			dataObject = new DataObject(list.ToArray());
		}
		else
		{
			dataObject = new DataObject(new object[1] { objectToInsert });
		}
		int count = Layers.Count;
		ITransactionContext context = this.As<ITransactionContext>();
		context.DoTransaction(delegate
		{
			Insert(parent, dataObject);
		}, "Scripted Insert Layer");
		int count2 = Layers.Count;
		LayerFolder result = null;
		if (count2 > count)
		{
			result = Layers[count2 - 1];
		}
		return result;
	}

	public void Insert(object parent, object insertingObject)
	{
		if (parent == null)
		{
			parent = m_activeItem ?? this;
		}
		else if (!(parent is LayeringContext) && !(parent is LayerFolder))
		{
			return;
		}
		IEnumerable<DomNode> compatibleNodes = GetCompatibleNodes((IDataObject)insertingObject);
		if (compatibleNodes == null)
		{
			return;
		}
		LayerFolder layerFolder = parent.As<LayerFolder>();
		if (layerFolder == null && compatibleNodes.Any())
		{
			DomNode adaptable = new DomNode(LayerFolderType);
			LayerFolder layerFolder2 = adaptable.As<LayerFolder>();
			layerFolder2.Name = "New Layer".Localize();
			Layers.Add(layerFolder2);
			layerFolder = layerFolder2;
		}
		if (layerFolder == null)
		{
			return;
		}
		foreach (DomNode item in compatibleNodes)
		{
			Element element = item.As<Element>();
			if (element != null && !layerFolder.Contains(element))
			{
				DomNode adaptable2 = new DomNode(ElementRefType);
				ElementRef elementRef = adaptable2.As<ElementRef>();
				elementRef.Element = element;
				layerFolder.ElementRefs.Add(elementRef);
			}
			ElementRef elementRef2 = item.As<ElementRef>();
			if (elementRef2 != null && elementRef2.Element != null && !layerFolder.Contains(elementRef2.Element))
			{
				layerFolder.ElementRefs.Add(elementRef2);
			}
			LayerFolder layerFolder3 = item.As<LayerFolder>();
			if (layerFolder3 != null)
			{
				layerFolder.Folders.Add(layerFolder3);
			}
		}
	}

	private static IEnumerable<DomNode> GetCompatibleNodes(IDataObject dataObject)
	{
		IList<DomNode> list = new List<DomNode>();
		IEnumerable<object> enumerable = dataObject.GetData(typeof(object[])) as object[];
		if (enumerable != null)
		{
			foreach (object item in enumerable)
			{
				if (item.Is<Element>() || item.Is<ElementRef>() || item.Is<LayerFolder>())
				{
					list.Add(item.Cast<DomNode>());
				}
			}
		}
		return list;
	}

	public void SetActiveItem(object item)
	{
		m_activeItem = item;
	}

	IEnumerable<object> ITreeView.GetChildren(object parent)
	{
		LayerFolder layer = parent.As<LayerFolder>();
		if (layer != null)
		{
			foreach (LayerFolder folder in layer.Folders)
			{
				yield return folder;
			}
			foreach (ElementRef elementRef in layer.ElementRefs)
			{
				yield return elementRef;
			}
		}
		else
		{
			if (!parent.Is<LayeringContext>())
			{
				yield break;
			}
			foreach (LayerFolder layer2 in Layers)
			{
				yield return layer2;
			}
		}
	}

	public void GetInfo(object item, ItemInfo info)
	{
		LayerFolder layerFolder = item.As<LayerFolder>();
		if (layerFolder != null)
		{
			info.Label = layerFolder.Name;
			info.HasCheck = true;
			info.SetCheckState(GetCheckState(layerFolder));
			return;
		}
		ElementRef elementRef = item.As<ElementRef>();
		if (elementRef != null)
		{
			Element element = elementRef.Element;
			if (element != null)
			{
				info.Label = element.Id;
				info.IsLeaf = true;
			}
			IVisible iVisible = GetIVisible(item);
			if (iVisible != null)
			{
				info.HasCheck = true;
				info.Checked = iVisible.Visible;
			}
		}
	}

	private static CheckState GetCheckState(LayerFolder layer)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (LayerFolder folder in layer.Folders)
		{
			switch (GetCheckState(folder))
			{
			case CheckState.Checked:
				flag2 = true;
				break;
			case CheckState.Unchecked:
				flag = true;
				break;
			case CheckState.Indeterminate:
				flag2 = true;
				flag = true;
				break;
			}
		}
		foreach (Element element in layer.GetElements())
		{
			IVisible visible = element.As<IVisible>();
			if (visible != null)
			{
				if (visible.Visible)
				{
					flag2 = true;
				}
				else
				{
					flag = true;
				}
			}
		}
		if (flag2 && !flag)
		{
			return CheckState.Checked;
		}
		if (flag && !flag2)
		{
			return CheckState.Unchecked;
		}
		return CheckState.Indeterminate;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (!e.AttributeInfo.Equivalent(VisibleAttribute))
		{
			return;
		}
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		foreach (DomNode item in base.DomNode.Subtree)
		{
			if (IsLayerItem(item))
			{
				this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(item));
			}
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (IsLayerItem(e.Child))
		{
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (IsLayerItem(e.Child))
		{
			this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	public bool IsVisible(object item)
	{
		LayerFolder layerFolder = item.As<LayerFolder>();
		return layerFolder == null || GetCheckState(layerFolder) == CheckState.Checked;
	}

	public bool CanSetVisible(object item)
	{
		return item.Is<LayerFolder>() || item.Is<IVisible>();
	}

	public void SetVisible(object item, bool value)
	{
		LayerFolder layerFolder = item.As<LayerFolder>();
		if (layerFolder != null)
		{
			PropagateVisible(layerFolder, value);
			return;
		}
		IVisible iVisible = GetIVisible(item);
		if (iVisible != null)
		{
			iVisible.Visible = value;
		}
	}

	private static void PropagateVisible(LayerFolder layer, bool visible)
	{
		foreach (LayerFolder folder in layer.Folders)
		{
			PropagateVisible(folder, visible);
		}
		foreach (Element element in layer.GetElements())
		{
			IVisible visible2 = element.As<IVisible>();
			if (visible2 != null)
			{
				visible2.Visible = visible;
			}
		}
	}

	private static IVisible GetIVisible(object item)
	{
		ElementRef elementRef = item.As<ElementRef>();
		if (elementRef != null)
		{
			return elementRef.Element.As<IVisible>();
		}
		return item.As<IVisible>();
	}

	public string GetName(object item)
	{
		return item.As<LayerFolder>()?.Name;
	}

	public bool CanSetName(object item)
	{
		return item.Is<LayerFolder>();
	}

	public void SetName(object item, string name)
	{
		LayerFolder layerFolder = item.As<LayerFolder>();
		if (layerFolder != null)
		{
			layerFolder.Name = name;
		}
	}

	private static bool IsLayerItem(DomNode node)
	{
		return node.Is<LayerFolder>() || node.Is<ElementRef>() || node.Is<Element>();
	}
}
