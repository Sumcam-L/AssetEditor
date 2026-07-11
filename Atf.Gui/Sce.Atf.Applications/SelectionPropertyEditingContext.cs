using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

public class SelectionPropertyEditingContext : IPropertyEditingContext, IObservableContext, IAdaptable
{
	private ISelectionContext m_selectionContext;

	private IObservableContext m_observableContext;

	private IValidationContext m_validationContext;

	private bool m_validating;

	private bool m_invalid;

	public ISelectionContext SelectionContext
	{
		get
		{
			return m_selectionContext;
		}
		set
		{
			if (m_selectionContext == value)
			{
				return;
			}
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged -= selection_Changed;
				if (m_observableContext != null)
				{
					m_observableContext.ItemChanged -= observableContext_ItemChanged;
					m_observableContext.ItemInserted -= observableContext_ItemInserted;
					m_observableContext.ItemRemoved -= observableContext_ItemRemoved;
				}
				if (m_validationContext != null)
				{
					m_validationContext.Beginning -= validationContext_Beginning;
					m_validationContext.Ended -= validationContext_Ended;
					m_validationContext.Cancelled -= validationContext_Cancelled;
				}
			}
			m_selectionContext = value;
			m_observableContext = null;
			m_validationContext = null;
			if (m_selectionContext != null)
			{
				m_selectionContext.SelectionChanged += selection_Changed;
				m_observableContext = m_selectionContext.As<IObservableContext>();
				if (m_observableContext != null)
				{
					m_observableContext.ItemChanged += observableContext_ItemChanged;
					m_observableContext.ItemInserted += observableContext_ItemInserted;
					m_observableContext.ItemRemoved += observableContext_ItemRemoved;
				}
				m_validationContext = m_selectionContext.As<IValidationContext>();
				if (m_validationContext != null)
				{
					m_validationContext.Beginning += validationContext_Beginning;
					m_validationContext.Ended += validationContext_Ended;
					m_validationContext.Cancelled += validationContext_Cancelled;
				}
			}
			OnReloaded(EventArgs.Empty);
		}
	}

	public IEnumerable<object> Items
	{
		get
		{
			if (m_selectionContext != null)
			{
				return m_selectionContext.Selection;
			}
			return EmptyEnumerable<object>.Instance;
		}
	}

	public IEnumerable<PropertyDescriptor> PropertyDescriptors => GetPropertyDescriptors();

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted
	{
		add
		{
		}
		remove
		{
		}
	}

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
	{
		add
		{
		}
		remove
		{
		}
	}

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	public event EventHandler Reloaded;

	private void selection_Changed(object sender, EventArgs e)
	{
		OnReloaded(e);
	}

	object IAdaptable.GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(GetType()))
		{
			return this;
		}
		return m_selectionContext.As(type);
	}

	protected virtual void OnReloaded(EventArgs e)
	{
		this.Reloaded.Raise(this, e);
	}

	protected virtual IEnumerable<PropertyDescriptor> GetPropertyDescriptors()
	{
		return PropertyUtils.GetSharedProperties(Items);
	}

	private void validationContext_Beginning(object sender, EventArgs e)
	{
		m_validating = true;
	}

	private void validationContext_Ended(object sender, EventArgs e)
	{
		EndValidation();
	}

	private void validationContext_Cancelled(object sender, EventArgs e)
	{
		EndValidation();
	}

	private void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		Invalidate();
	}

	private void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		Invalidate();
	}

	private void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		Invalidate();
	}

	private void Invalidate()
	{
		if (m_validating)
		{
			m_invalid = true;
			return;
		}
		OnReloaded(EventArgs.Empty);
		m_invalid = false;
	}

	private void EndValidation()
	{
		m_validating = false;
		if (m_invalid)
		{
			OnReloaded(EventArgs.Empty);
		}
		m_invalid = false;
	}
}
