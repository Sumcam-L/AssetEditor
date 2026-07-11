using System.Windows.Data;

namespace Sce.Atf.Wpf.Markup;

public class TransactionMultiBinding : MultiBinding
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

	public TransactionMultiBinding()
	{
		m_core = new TransactionBindingCore(base.ValidationRules);
	}
}
