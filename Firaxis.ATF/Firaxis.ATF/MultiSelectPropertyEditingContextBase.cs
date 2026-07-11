using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public abstract class MultiSelectPropertyEditingContextBase : IPropertyEditingContext, IAdaptable
{
	protected ITransactionContext TransactionContext { get; set; }

	public IEnumerable<object> Items => GetItems();

	public IEnumerable<PropertyDescriptor> PropertyDescriptors => GetPropertyDescriptors();

	public MultiSelectPropertyEditingContextBase(ITransactionContext transCtx)
	{
		TransactionContext = transCtx;
	}

	public object GetAdapter(Type type)
	{
		if (type.IsAssignableFrom(TransactionContext.GetType()))
		{
			return TransactionContext;
		}
		return null;
	}

	protected abstract IEnumerable<PropertyDescriptor> GetPropertyDescriptors();

	protected abstract IEnumerable<object> GetItems();
}
