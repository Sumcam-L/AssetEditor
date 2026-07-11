using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefSetTreeView : DomNodeAdapter, ITreeView, IItemView, IObservableContext
{
	public object Root => base.DomNode;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler Reloaded;

	public IEnumerable<object> GetChildren(object parent)
	{
		if (!(parent is DomNode node))
		{
			yield break;
		}
		if (node.As<ArtDefCollectionAdapter>() != null)
		{
			IATFEditorTarget iATFEditorTarget = node.As<IATFEditorTarget>();
			if (iATFEditorTarget.UseCustomEditor && (!iATFEditorTarget.UseCustomEditor || EditorCatalog.IsEditorSupported(iATFEditorTarget.CustomEditor)))
			{
				yield break;
			}
			foreach (ChildInfo child2 in node.Type.Children)
			{
				if (child2.IsList)
				{
					foreach (DomNode child3 in node.GetChildList(child2))
					{
						yield return child3;
					}
					continue;
				}
				DomNode child = node.GetChild(child2);
				if (child != null)
				{
					yield return child;
				}
			}
			yield break;
		}
		if (node.As<ArtDefElementAdapter>() != null)
		{
			foreach (ChildInfo child4 in node.Type.Children)
			{
				if (!child4.IsList)
				{
					continue;
				}
				foreach (DomNode child5 in node.GetChildList(child4))
				{
					if (child5.Is<ArtDefCollectionAdapter>())
					{
						yield return child5;
					}
				}
			}
		}
		ArtDefSetAdapter artDefSetAdapter = node.As<ArtDefSetAdapter>();
		if (artDefSetAdapter == null)
		{
			yield break;
		}
		foreach (ArtDefCollectionAdapter rootCollection in artDefSetAdapter.RootCollections)
		{
			yield return rootCollection.DomNode;
		}
	}

	public void GetInfo(object item, ItemInfo info)
	{
		if (!(item is DomNode domNode))
		{
			return;
		}
		info.IsLeaf = true;
		info.AllowLabelEdit = false;
		if (domNode.Type == ArtDefSchema.ArtDefType.Type)
		{
			info.IsLeaf = false;
			info.ImageIndex = -1;
		}
		else if (domNode.Type == ArtDefSchema.ArtDefCollectionType.Type)
		{
			ArtDefCollectionAdapter artDefCollectionAdapter = domNode.As<ArtDefCollectionAdapter>();
			info.Label = artDefCollectionAdapter.Name;
			info.IsLeaf = artDefCollectionAdapter.UseCustomEditor && EditorCatalog.IsEditorSupported(artDefCollectionAdapter.CustomEditor);
			if (info.IsLeaf)
			{
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.CollectionPackageImage);
			}
			else
			{
				info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.FolderImage);
			}
		}
		else if (ArtDefSchema.ArtDefElementType.Type.IsAssignableFrom(domNode.Type))
		{
			ArtDefElementAdapter artDefElementAdapter = domNode.As<ArtDefElementAdapter>();
			info.Label = artDefElementAdapter.Name;
			info.IsLeaf = artDefElementAdapter.ArtDefElementTmpl.Children.Count() == 0 && artDefElementAdapter.Collections.Count == 0;
			info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.ElementDotImage);
		}
		else if (domNode.Is<FieldValueAdapter>())
		{
			FieldValueAdapter fieldValueAdapter = domNode.As<FieldValueAdapter>();
			info.Label = fieldValueAdapter.Name;
		}
		else if (domNode.Is<CollectionFieldValueAdapter>())
		{
			info.IsLeaf = false;
		}
		info.HoverText = GetDescriptionString(domNode);
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += root_AttributeChanged;
		base.DomNode.ChildInserted += root_ChildInserted;
		base.DomNode.ChildRemoved += root_ChildRemoved;
		this.Reloaded.Raise(this, EventArgs.Empty);
		base.OnNodeSet();
	}

	private int GetChildIndex(object child, object parent)
	{
		IEnumerable<object> children = GetChildren(parent);
		int num = 0;
		foreach (object item in (IEnumerable)children)
		{
			if (item.Equals(child))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private string GetDescriptionString(DomNode node)
	{
		_ = string.Empty;
		string result = string.Empty;
		IDescriptionProvider descriptionProvider = node.As<IDescriptionProvider>();
		if (descriptionProvider != null)
		{
			_ = descriptionProvider.Name;
			result = descriptionProvider.Description;
		}
		else
		{
			IArtDefSetContext artDefSetContext = node.As<IArtDefSetContext>();
			if (artDefSetContext != null)
			{
				_ = artDefSetContext.TemplateName;
				result = artDefSetContext.Description;
			}
		}
		return result;
	}

	private void root_AttributeChanged(object sender, AttributeEventArgs e)
	{
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void root_ChildInserted(object sender, ChildEventArgs e)
	{
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
	}

	private void root_ChildRemoved(object sender, ChildEventArgs e)
	{
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
	}
}
