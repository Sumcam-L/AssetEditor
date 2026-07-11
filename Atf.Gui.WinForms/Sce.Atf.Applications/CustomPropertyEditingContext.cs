using System;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications;

public class CustomPropertyEditingContext : PropertyEditingContext, IAdaptable
{
	private object m_context;

	public CustomPropertyEditingContext(object item, object context)
		: base(new object[1] { item })
	{
		m_context = context;
	}

	object IAdaptable.GetAdapter(Type type)
	{
		if (type == typeof(ITransactionContext))
		{
			return m_context.As<ITransactionContext>();
		}
		return null;
	}
}
