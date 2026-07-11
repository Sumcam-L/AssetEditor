using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public abstract class TemplatingContext : SelectionContext, IInstancingContext, ITemplatingContext, ITreeView, IItemView, IObservableContext, INamingContext
{
	private object m_activeItem;

	private Dictionary<object, object> m_lastPromoted = new Dictionary<object, object>();

	protected bool IsMovingItems { get; set; }

	public abstract TemplateFolder RootFolder { get; }

	protected abstract DomNodeType TemplateType { get; }

	object ITreeView.Root => RootFolder;

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

	public string GetName(object item)
	{
		TemplateFolder templateFolder = item.As<TemplateFolder>();
		if (templateFolder != null)
		{
			return templateFolder.Name;
		}
		return item.As<Template>()?.Name;
	}

	public bool CanSetName(object item)
	{
		return item.Is<TemplateFolder>() || item.Is<Template>();
	}

	public void SetName(object item, string name)
	{
		TemplateFolder templateFolder = item.As<TemplateFolder>();
		if (templateFolder != null)
		{
			templateFolder.Name = name;
			return;
		}
		Template template = item.As<Template>();
		if (template != null)
		{
			template.Name = name;
		}
	}

	public void SetActiveItem(object item)
	{
		if (item.Is<TemplateFolder>() || item.Is<Template>())
		{
			m_activeItem = item;
		}
		else
		{
			m_activeItem = RootFolder;
		}
	}

	public IDataObject GetInstances(IEnumerable<object> items)
	{
		List<object> list = new List<object>();
		foreach (object item in items)
		{
			if (item.Is<Template>())
			{
				list.Add(item.Cast<Template>());
			}
			else if (item.Is<TemplateFolder>())
			{
				list.Add(item.Cast<TemplateFolder>());
			}
		}
		return new DataObject(list.ToArray());
	}

	public abstract bool CanReference(object item);

	public abstract object CreateReference(object item);

	public bool CanCopy()
	{
		return base.Selection.Count > 0 && base.Selection.All((object x) => x.Is<Template>());
	}

	public object Copy()
	{
		return GetInstances(base.Selection);
	}

	public bool CanInsert(object insertingObject)
	{
		IDataObject dataObject = (IDataObject)insertingObject;
		if (!(dataObject.GetData(typeof(object[])) is object[] source) || !source.Any())
		{
			return false;
		}
		if (!m_activeItem.Is<TemplateFolder>())
		{
			return false;
		}
		if (!source.All((object item) => item.Is<Template>() || item.Is<TemplateFolder>()))
		{
			return false;
		}
		if (source.Any(IsExternalTemplate))
		{
			return false;
		}
		if (IsExternalTemplate(m_activeItem.Cast<TemplateFolder>()))
		{
			return false;
		}
		return true;
	}

	public void Insert(object insertingObject)
	{
		IDataObject dataObject = (IDataObject)insertingObject;
		if (!(dataObject.GetData(typeof(object[])) is object[] array))
		{
			return;
		}
		TemplateFolder templateFolder = m_activeItem.As<TemplateFolder>() ?? RootFolder;
		IEnumerable<DomNode> enumerable = array.AsIEnumerable<DomNode>();
		IsMovingItems = enumerable.All((DomNode x) => IsTemplateItem(x, x.Parent));
		DomNode[] array2 = (IsMovingItems ? enumerable.ToArray() : DomNode.Copy(enumerable));
		if (IsMovingItems)
		{
			DomNode[] array3 = array2;
			foreach (DomNode adaptable in array3)
			{
				if (adaptable.Is<Template>())
				{
					templateFolder.Templates.Add(adaptable.Cast<Template>());
				}
				else if (adaptable.Is<TemplateFolder>())
				{
					templateFolder.Folders.Add(adaptable.Cast<TemplateFolder>());
				}
			}
		}
		else
		{
			m_lastPromoted.Clear();
			if (IsExternalTemplate(templateFolder))
			{
				templateFolder = RootFolder;
			}
			for (int num2 = 0; num2 < array2.Length; num2++)
			{
				DomNode target = array2[num2];
				Template template = new DomNode(TemplateType).Cast<Template>();
				template.Target = target;
				template.Guid = Guid.NewGuid();
				templateFolder.Templates.Add(template);
				m_lastPromoted.Add(array[num2], template);
			}
		}
		IsMovingItems = false;
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
		TemplateFolder folder = parent.As<TemplateFolder>();
		if (folder == null)
		{
			yield break;
		}
		foreach (TemplateFolder folder2 in folder.Folders)
		{
			yield return folder2;
		}
		foreach (Template template in folder.Templates)
		{
			yield return template;
		}
	}

	public virtual void GetInfo(object item, ItemInfo info)
	{
		TemplateFolder templateFolder = item.As<TemplateFolder>();
		if (templateFolder != null)
		{
			info.Label = templateFolder.Name;
			if (templateFolder.Url != null)
			{
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(info.IsExpandedInView ? Resources.ReferenceFolderOpen : Resources.ReferenceFolderClosed);
				string hoverText = (info.Description = templateFolder.Url.LocalPath);
				info.HoverText = hoverText;
			}
			else
			{
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.FolderIcon);
			}
		}
		else
		{
			Template template = item.As<Template>();
			if (template != null)
			{
				info.Label = template.Name;
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.ComponentImage);
				info.IsLeaf = true;
			}
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (IsTemplateItem(e.DomNode, e.DomNode.Parent))
		{
			this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (IsTemplateItem(e.Child, e.Parent))
		{
			this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (IsTemplateItem(e.Child, e.Parent))
		{
			this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	private static bool IsTemplateItem(DomNode child, DomNode parent)
	{
		return child.Is<TemplateFolder>() || (parent?.Is<TemplateFolder>() ?? false);
	}

	public object LastPromoted(object original)
	{
		return m_lastPromoted.ContainsKey(original) ? m_lastPromoted[original] : null;
	}

	public bool IsExternalTemplate(object item)
	{
		DomNode domNode = item.As<DomNode>();
		if (domNode == null)
		{
			return false;
		}
		foreach (DomNode item2 in domNode.Lineage)
		{
			TemplateFolder templateFolder = item2.As<TemplateFolder>();
			if (templateFolder != null && templateFolder.Url != null)
			{
				return true;
			}
		}
		return false;
	}

	public Template SearchForTemplateByGuid(TemplateFolder parentFolder, Guid guid)
	{
		foreach (Template template2 in parentFolder.Templates)
		{
			if (template2.Guid == guid)
			{
				return template2;
			}
		}
		foreach (TemplateFolder folder in parentFolder.Folders)
		{
			Template template = SearchForTemplateByGuid(folder, guid);
			if (template != null)
			{
				return template;
			}
		}
		return null;
	}

	public void ReplaceTemplateModel(Template template, DomNode sourceModel)
	{
		template.Target = DomNode.Copy(new DomNode[1] { sourceModel }).First();
		m_lastPromoted.Add(sourceModel, template);
	}

	public bool ValidateNewFolderUri(Uri uri)
	{
		return ValidateNewFolderUri(RootFolder, uri);
	}

	private bool ValidateNewFolderUri(TemplateFolder parentFolder, Uri uri)
	{
		if (uri == null)
		{
			return false;
		}
		foreach (TemplateFolder folder in parentFolder.Folders)
		{
			if (folder.Url == uri)
			{
				return false;
			}
			if (!ValidateNewFolderUri(folder, uri))
			{
				return false;
			}
		}
		return true;
	}
}
