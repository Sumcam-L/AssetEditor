using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public abstract class PrototypingContext : SelectionContext, IInstancingContext, IPrototypingContext, ITreeView, IItemView, IObservableContext, INamingContext
{
	private object m_activeItem;

	protected abstract ChildInfo PrototypeFolderChildInfo { get; }

	protected abstract DomNodeType PrototypeType { get; }

	public PrototypeFolder PrototypeFolder => GetChild<PrototypeFolder>(PrototypeFolderChildInfo);

	object ITreeView.Root => PrototypeFolder;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.OnNodeSet();
		if (this.Reloaded != null)
		{
		}
	}

	string INamingContext.GetName(object item)
	{
		Prototype prototype = item.As<Prototype>();
		if (prototype != null)
		{
			return prototype.Name;
		}
		return item.As<PrototypeFolder>()?.Name;
	}

	bool INamingContext.CanSetName(object item)
	{
		return item.Is<Prototype>() || item.Is<PrototypeFolder>();
	}

	void INamingContext.SetName(object item, string name)
	{
		Prototype prototype = item.As<Prototype>();
		if (prototype != null)
		{
			prototype.Name = name;
			return;
		}
		PrototypeFolder prototypeFolder = item.As<PrototypeFolder>();
		if (prototypeFolder != null)
		{
			prototypeFolder.Name = name;
		}
	}

	public void SetActiveItem(object item)
	{
		m_activeItem = item;
	}

	public IDataObject GetInstances(IEnumerable<object> items)
	{
		List<object> list = new List<object>();
		foreach (object item in items)
		{
			Prototype prototype = item.As<Prototype>();
			if (prototype != null)
			{
				list.AddRange(prototype.Modules.AsIEnumerable<object>());
				list.AddRange(prototype.Connections.AsIEnumerable<object>());
				continue;
			}
			PrototypeFolder prototypeFolder = item.As<PrototypeFolder>();
			if (prototypeFolder != null)
			{
				list.Add(prototypeFolder);
			}
		}
		return new DataObject(list.ToArray());
	}

	public bool CanCopy()
	{
		return base.Selection.Count > 0;
	}

	public object Copy()
	{
		return GetInstances(base.Selection);
	}

	public bool CanInsert(object insertingObject)
	{
		IDataObject dataObject = (IDataObject)insertingObject;
		if (!(dataObject.GetData(typeof(object[])) is object[] array))
		{
			return false;
		}
		object[] array2 = array;
		foreach (object reference in array2)
		{
			if (!reference.Is<Element>() && !reference.Is<Wire>() && !reference.Is<Annotation>())
			{
				return false;
			}
		}
		return true;
	}

	public void Insert(object insertingObject)
	{
		IDataObject dataObject = (IDataObject)insertingObject;
		if (!(dataObject.GetData(typeof(object[])) is object[] enumerable))
		{
			return;
		}
		object[] enumerable2 = DomNode.Copy(enumerable.AsIEnumerable<DomNode>());
		DomNode adaptable = new DomNode(PrototypeType);
		Prototype prototype = adaptable.As<Prototype>();
		prototype.Name = "Prototype".Localize("Circuit prototype");
		foreach (Element item in enumerable2.AsIEnumerable<Element>())
		{
			prototype.Modules.Add(item);
		}
		foreach (Wire item2 in enumerable2.AsIEnumerable<Wire>())
		{
			prototype.Connections.Add(item2);
		}
		PrototypeFolder prototypeFolder = m_activeItem.As<PrototypeFolder>();
		if (prototypeFolder == null)
		{
			prototypeFolder = PrototypeFolder;
		}
		prototypeFolder.Prototypes.Add(prototype);
	}

	public bool CanDelete()
	{
		return base.Selection.Count > 0;
	}

	public void Delete()
	{
		foreach (DomNode item in base.Selection.AsIEnumerable<DomNode>())
		{
			item.RemoveFromParent();
		}
		base.Selection.Clear();
	}

	IEnumerable<object> ITreeView.GetChildren(object parent)
	{
		PrototypeFolder folder = parent.As<PrototypeFolder>();
		if (folder == null)
		{
			yield break;
		}
		foreach (PrototypeFolder folder2 in folder.Folders)
		{
			yield return folder2;
		}
		foreach (Prototype prototype in folder.Prototypes)
		{
			yield return prototype;
		}
	}

	public void GetInfo(object item, ItemInfo info)
	{
		PrototypeFolder prototypeFolder = item.As<PrototypeFolder>();
		if (prototypeFolder != null)
		{
			info.Label = prototypeFolder.Name;
			return;
		}
		Prototype prototype = item.As<Prototype>();
		if (prototype != null)
		{
			info.Label = prototype.Name;
			info.IsLeaf = true;
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (IsPrototypeItem(e.DomNode, e.DomNode.Parent))
		{
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (IsPrototypeItem(e.Child, e.Parent))
		{
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (IsPrototypeItem(e.Child, e.Parent))
		{
			this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	private static bool IsPrototypeItem(DomNode child, DomNode parent)
	{
		return child.Is<PrototypeFolder>() || (parent?.Is<PrototypeFolder>() ?? false);
	}
}
