using System;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Markup;

public class TransactionBinding : Binding
{
	private readonly TransactionBindingCore m_core;

	public string Transaction
	{
		get
		{
			return m_core.Transaction;
		}
		set
		{
			m_core.Transaction = value;
		}
	}

	public TransactionBinding()
	{
		m_core = new TransactionBindingCore(base.ValidationRules);
		base.UpdateSourceExceptionFilter = UpdateSourceExceptionFilterCallback;
		base.ValidatesOnDataErrors = true;
		base.ValidatesOnExceptions = true;
		base.Mode = BindingMode.TwoWay;
	}

	public TransactionBinding(string path)
		: base(path)
	{
		m_core = new TransactionBindingCore(base.ValidationRules);
		base.UpdateSourceExceptionFilter = UpdateSourceExceptionFilterCallback;
		base.ValidatesOnDataErrors = true;
		base.ValidatesOnExceptions = true;
		base.Mode = BindingMode.TwoWay;
	}

	private object UpdateSourceExceptionFilterCallback(object bindExpression, Exception exception)
	{
		m_core.CancelTransaction();
		return exception;
	}
}
