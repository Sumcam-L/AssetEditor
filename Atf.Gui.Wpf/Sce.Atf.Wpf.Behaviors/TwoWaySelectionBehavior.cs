using System.Collections;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors;

public class TwoWaySelectionBehavior : SelectionBehaviorBase
{
	private bool m_synchronising;

	private IList m_associatedObjectSelection;

	protected override void OnAttached()
	{
		base.OnAttached();
		if (base.AssociatedObject is MultiSelector)
		{
			MultiSelector multiSelector = base.AssociatedObject as MultiSelector;
			m_associatedObjectSelection = multiSelector.SelectedItems;
		}
		else if (base.AssociatedObject is ListBox)
		{
			ListBox listBox = base.AssociatedObject as ListBox;
			if (listBox.SelectionMode != SelectionMode.Single)
			{
				m_associatedObjectSelection = (base.AssociatedObject as ListBox).SelectedItems;
			}
		}
		Composer.Current.Container.SatisfyImportsOnce(this);
	}

	protected override void OnAssociatedObjectSelectionChanged(IList addeditems, IList removedItems)
	{
		base.OnAssociatedObjectSelectionChanged(addeditems, removedItems);
		if (m_synchronising)
		{
			return;
		}
		try
		{
			m_synchronising = true;
			if (base.SelectionContext == null)
			{
				return;
			}
			if (m_associatedObjectSelection != null)
			{
				if (removedItems.Count > 0)
				{
					IEnumerable items = ConvertToSelectionContext(removedItems);
					base.SelectionContext.RemoveRange(items);
				}
			}
			else
			{
				base.SelectionContext.Clear();
			}
			if (addeditems.Count > 0)
			{
				IEnumerable items2 = ConvertToSelectionContext(addeditems);
				base.SelectionContext.AddRange(items2);
			}
		}
		finally
		{
			m_synchronising = false;
		}
	}

	protected override void OnSelectionContextSelectionChanged()
	{
		base.OnSelectionContextSelectionChanged();
		if (m_synchronising || base.SelectionContext == null)
		{
			return;
		}
		try
		{
			object[] newSelection = base.SelectionContext.Selection.ToArray();
			if (m_associatedObjectSelection != null)
			{
				SynchoniseMultiSelectionToAssociateObject(m_associatedObjectSelection, newSelection);
			}
			else
			{
				SynchoniseSingleSelectionToAssociateObject(newSelection);
			}
		}
		finally
		{
			m_synchronising = false;
		}
	}

	protected virtual object ConvertFromSelectionContext(object item)
	{
		if (base.AssociatedObject != null && !base.AssociatedObject.Items.Contains(item))
		{
			return null;
		}
		return item;
	}

	protected virtual object ConvertToSelectionContext(object item)
	{
		return item;
	}

	private void SynchoniseMultiSelectionToAssociateObject(IList associatedObjectSelection, object[] newSelection)
	{
		object[] array = m_associatedObjectSelection.Cast<object>().ToArray();
		foreach (object obj in array)
		{
			object obj2 = ConvertToSelectionContext(obj);
			if (obj2 != null && !newSelection.Contains(obj2))
			{
				m_associatedObjectSelection.Remove(obj);
			}
		}
		foreach (object item in newSelection)
		{
			object obj3 = ConvertFromSelectionContextAndCheckExistance(item);
			if (obj3 != null)
			{
				m_associatedObjectSelection.Add(obj3);
			}
		}
	}

	private void SynchoniseSingleSelectionToAssociateObject(object[] newSelection)
	{
		object selectedItem = null;
		foreach (object item in newSelection)
		{
			object obj = ConvertFromSelectionContextAndCheckExistance(item);
			if (obj != null)
			{
				selectedItem = obj;
				break;
			}
		}
		base.AssociatedObject.SelectedItem = selectedItem;
	}

	private object ConvertFromSelectionContextAndCheckExistance(object item)
	{
		object obj = ConvertFromSelectionContext(item);
		if (obj != null && base.AssociatedObject != null && !base.AssociatedObject.Items.Contains(item))
		{
			return null;
		}
		return obj;
	}

	private IEnumerable ConvertToSelectionContext(IEnumerable items)
	{
		foreach (object item in items)
		{
			object converted = ConvertToSelectionContext(item);
			if (converted != null)
			{
				yield return converted;
			}
		}
	}
}
