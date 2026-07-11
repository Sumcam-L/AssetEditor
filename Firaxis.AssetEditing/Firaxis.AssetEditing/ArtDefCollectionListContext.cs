using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefCollectionListContext : IGridViewEditingContext, IAdaptable
{
	private ICollection<int> m_selectedIndices = new List<int>();

	private ArtDefCollectionAdapter Adapter { get; set; }

	public override string[] ColumnNames
	{
		get
		{
			ISet<string> set = new SortedSet<string>();
			foreach (ArtDefElementAdapter element in Adapter.Elements)
			{
				foreach (FieldValueAdapter field in element.Fields)
				{
					foreach (FieldPropertyDescriptorBase item in field.PropertyDescriptors.OfType<FieldPropertyDescriptorBase>())
					{
						set.Add(item.Name);
					}
				}
			}
			return set.ToArray();
		}
	}

	public override IEnumerable<object> Items
	{
		get
		{
			foreach (ArtDefElementAdapter element in Adapter.Elements)
			{
				yield return element.DomNode;
			}
		}
	}

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			if (Adapter.Elements.Count == 0)
			{
				yield break;
			}
			ArtDefElementAdapter adapter = Adapter.Elements.ElementAt(0);
			foreach (object item in adapter.DomNode.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
			foreach (FieldValueAdapter field in adapter.Fields)
			{
				foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in field.PropertyDescriptors)
				{
					yield return propertyDescriptor;
				}
			}
		}
	}

	public override IEnumerable<int> SelectedIndices
	{
		get
		{
			return m_selectedIndices;
		}
		set
		{
			m_selectedIndices = new List<int>(value);
		}
	}

	public override IEnumerable<object> SelectedObjects
	{
		get
		{
			if (!SelectedIndices.Any())
			{
				yield break;
			}
			List<ArtDefElementAdapter> elements = Adapter.Elements.ToList();
			foreach (int selectedIndex in SelectedIndices)
			{
				if (selectedIndex >= 0 && selectedIndex < elements.Count)
				{
					yield return elements[selectedIndex];
				}
			}
		}
	}

	public override event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public override event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public override event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public override event EventHandler Reloaded;

	public ArtDefCollectionListContext(ArtDefCollectionAdapter adapter)
	{
		Adapter = adapter;
		Adapter.DomNode.ChildInserted += DomNode_ChildInserted;
		Adapter.DomNode.ChildRemoved += DomNode_ChildRemoved;
		Adapter.DomNode.AttributeChanged += DomNode_AttributeChanged;
		Adapter.Reloaded += Adapter_Reloaded;
		if (Reloaded == null)
		{
			_ = ItemChanged;
		}
	}

	public object GetAdapter(Type type)
	{
		return Adapter.As(type);
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		EventHandler<ItemChangedEventArgs<object>> eventHandler = ItemChanged;
		if (eventHandler != null && e.DomNode.IsDescendantOf(Adapter.DomNode))
		{
			eventHandler(sender, new ItemChangedEventArgs<object>(e.DomNode));
		}
	}

	private void Adapter_Reloaded(object sender, EventArgs e)
	{
		Reloaded?.Invoke(sender, e);
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		EventHandler<ItemInsertedEventArgs<object>> eventHandler = ItemInserted;
		if (eventHandler != null && e.Parent == Adapter.DomNode)
		{
			eventHandler(sender, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		EventHandler<ItemRemovedEventArgs<object>> eventHandler = ItemRemoved;
		if (eventHandler != null && e.Parent == Adapter.DomNode)
		{
			eventHandler(sender, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
		}
	}
}
