using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

public abstract class TransactionCommandBase : Command
{
	private string m_toolTip;

	public abstract IEnumerable<TransactionContext.Operation> Operations { get; }

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		protected set
		{
			m_toolTip = value;
		}
	}

	public TransactionCommandBase()
		: this(string.Empty, string.Empty)
	{
	}

	public TransactionCommandBase(string description)
		: this(description, string.Empty)
	{
	}

	public TransactionCommandBase(string description, string toolTip)
		: base(description)
	{
		m_toolTip = toolTip;
	}
}
