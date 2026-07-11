using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class TransactionPropertyNode : DynamicPropertyNode
{
	private readonly WeakReference m_contextRef;

	public ITransactionContext TransactionContext
	{
		get
		{
			if (m_contextRef != null && m_contextRef.IsAlive)
			{
				return m_contextRef.Target as ITransactionContext;
			}
			return null;
		}
	}

	public TransactionPropertyNode(ITransactionContext context)
	{
		if (context != null)
		{
			m_contextRef = new WeakReference(context);
		}
		else
		{
			IsReadOnly = true;
		}
	}

	public override void UnBind()
	{
		base.UnBind();
		if (m_contextRef != null)
		{
			m_contextRef.Target = null;
		}
	}

	public override void ResetValue()
	{
		ITransactionContext transactionContext = TransactionContext;
		if (transactionContext == null)
		{
			throw new InvalidOperationException("No transaction context available for TransactionPropertyNode");
		}
		transactionContext.DoTransaction(ResetValueInternal, "Reset Property".Localize());
	}

	protected override void SetValue(object value)
	{
		ITransactionContext transactionContext = TransactionContext;
		if (transactionContext == null)
		{
			throw new InvalidOperationException("No transaction context available for TransactionPropertyNode");
		}
		transactionContext.DoTransaction(delegate
		{
			SetValueInternal(value);
		}, "Edit Property".Localize());
	}

	private void SetValueInternal(object value)
	{
		base.SetValue(value);
	}

	private void ResetValueInternal()
	{
		base.ResetValue();
	}
}
