using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyEditorControlContext : ITypeDescriptorContext, IServiceProvider
{
	private readonly IPropertyEditingControlOwner m_editingControlOwner;

	private readonly System.ComponentModel.PropertyDescriptor m_descriptor;

	private readonly IContextRegistry m_contextRegistry;

	private List<object> m_cachedSelection;

	private ITransactionContext m_transactionContext;

	public System.ComponentModel.PropertyDescriptor Descriptor => m_descriptor;

	public IPropertyEditingControlOwner EditingControlOwner => m_editingControlOwner;

	public IEnumerable<object> SelectedObjects
	{
		get
		{
			IEnumerable<object> cachedSelection = m_cachedSelection;
			return cachedSelection ?? m_editingControlOwner.SelectedObjects;
		}
	}

	public object LastSelectedObject
	{
		get
		{
			if (m_cachedSelection != null)
			{
				return (m_cachedSelection.Count > 0) ? m_cachedSelection[m_cachedSelection.Count - 1] : null;
			}
			object[] selectedObjects = m_editingControlOwner.SelectedObjects;
			return (selectedObjects.Length != 0) ? selectedObjects[selectedObjects.Length - 1] : null;
		}
	}

	public ITransactionContext TransactionContext
	{
		get
		{
			return m_transactionContext;
		}
		set
		{
			m_transactionContext = value;
		}
	}

	public IContextRegistry ContextRegistry => m_contextRegistry;

	public bool IsDefaultValue => !m_descriptor.CanResetValue(LastSelectedObject);

	public bool IsDefaultValueForAll
	{
		get
		{
			foreach (object selectedObject in SelectedObjects)
			{
				if (m_descriptor.CanResetValue(selectedObject))
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool IsReadOnly => m_descriptor.IsReadOnly;

	IContainer ITypeDescriptorContext.Container => null;

	object ITypeDescriptorContext.Instance => LastSelectedObject;

	System.ComponentModel.PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => m_descriptor;

	public PropertyEditorControlContext(IPropertyEditingControlOwner editingControlOwner, System.ComponentModel.PropertyDescriptor descriptor, ITransactionContext transactionContext)
	{
		m_editingControlOwner = editingControlOwner;
		m_descriptor = descriptor;
		m_transactionContext = transactionContext;
		if (descriptor is MultiPropertyDescriptor multiPropertyDescriptor)
		{
			multiPropertyDescriptor.GetSelectionFunc = () => SelectedObjects;
		}
	}

	public PropertyEditorControlContext(IPropertyEditingControlOwner editingControlOwner, System.ComponentModel.PropertyDescriptor descriptor, ITransactionContext transactionContext, IContextRegistry contextRegistry)
		: this(editingControlOwner, descriptor, transactionContext)
	{
		m_contextRegistry = contextRegistry;
	}

	public void CacheSelection()
	{
		object[] selectedObjects = m_editingControlOwner.SelectedObjects;
		m_cachedSelection = new List<object>(selectedObjects.Length);
		m_cachedSelection.AddRange(selectedObjects);
	}

	public void ClearCachedSelection()
	{
		m_cachedSelection = null;
	}

	public virtual object GetValue()
	{
		return m_descriptor.GetValue(LastSelectedObject);
	}

	public object[] GetValues()
	{
		List<object> list = new List<object>();
		foreach (object selectedObject in SelectedObjects)
		{
			list.Add(m_descriptor.GetValue(selectedObject));
		}
		return list.ToArray();
	}

	public virtual void ResetValue()
	{
		m_transactionContext.DoTransaction(delegate
		{
			PropertyUtils.ResetProperty(SelectedObjects, m_descriptor);
		}, string.Format("Reset: {0}".Localize(), m_descriptor.DisplayName));
	}

	public virtual void SetValue(object newValue)
	{
		m_transactionContext.DoTransaction(delegate
		{
			foreach (object selectedObject in SelectedObjects)
			{
				PropertyUtils.SetProperty(selectedObject, m_descriptor, newValue);
			}
		}, string.Format("Edit: {0}".Localize(), m_descriptor.DisplayName));
	}

	public void SetValue(Array refArray, Array newArray)
	{
		bool[] equalComponents = GetEqualComponents(refArray, newArray);
		m_transactionContext.DoTransaction(delegate
		{
			foreach (object selectedObject in SelectedObjects)
			{
				Array oldArray = (Array)m_descriptor.GetValue(selectedObject);
				Array mergedArray = GetMergedArray(selectedObject, oldArray, newArray, equalComponents);
				PropertyUtils.SetProperty(selectedObject, m_descriptor, mergedArray);
			}
		}, string.Format("Edit: {0}".Localize(), m_descriptor.DisplayName));
	}

	private static bool[] GetEqualComponents(Array refArray, Array newArray)
	{
		bool[] array = new bool[newArray.Length];
		if (refArray != null)
		{
			for (int i = 0; i < newArray.Length; i++)
			{
				array[i] = PropertyUtils.AreEqual(newArray.GetValue(i), refArray.GetValue(i));
			}
		}
		return array;
	}

	private static Array GetMergedArray(object selected, Array oldArray, Array newArray, bool[] equalComponents)
	{
		Array array = newArray;
		if (oldArray != null)
		{
			array = (Array)oldArray.Clone();
			for (int i = 0; i < newArray.Length; i++)
			{
				if (!equalComponents[i])
				{
					array.SetValue(newArray.GetValue(i), i);
				}
			}
		}
		return array;
	}

	public UITypeEditor GetUITypeEditor()
	{
		return WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
	}

	public string GetPropertyText()
	{
		return PropertyUtils.GetPropertyText(LastSelectedObject, m_descriptor);
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		if (serviceType.IsAssignableFrom(GetType()))
		{
			return this;
		}
		return null;
	}

	void ITypeDescriptorContext.OnComponentChanged()
	{
	}

	bool ITypeDescriptorContext.OnComponentChanging()
	{
		return true;
	}
}
