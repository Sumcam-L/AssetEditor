using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom;

public class TransactionContext : DomNodeAdapter, ITransactionContext, IValidationContext
{
	private class TransactionSuspensionReceiptImpl : TransactionSuspensionReceipt, IDisposable
	{
		private TransactionContext m_context;

		public TransactionSuspensionReceiptImpl(TransactionContext context)
		{
			m_context = context;
			m_context.TransactionsActive = false;
		}

		public void Dispose()
		{
			m_context.TransactionsActive = true;
		}
	}

	public abstract class Operation
	{
		public abstract void Do();

		public abstract void Undo();
	}

	public class AttributeChangedOperation : Operation
	{
		private readonly DomNode m_node;

		private readonly AttributeInfo m_attributeInfo;

		private readonly object m_oldValue;

		private object m_newValue;

		public DomNode DomNode => m_node;

		public AttributeInfo AttributeInfo => m_attributeInfo;

		public object NewValue
		{
			get
			{
				return m_newValue;
			}
			set
			{
				m_newValue = value;
			}
		}

		public AttributeChangedOperation(AttributeEventArgs e)
		{
			m_node = e.DomNode;
			m_attributeInfo = e.AttributeInfo;
			m_oldValue = e.OldValue;
			m_newValue = e.NewValue;
		}

		public override string ToString()
		{
			return $"Change {m_node.Type} named {m_attributeInfo.Name}";
		}

		public override void Do()
		{
			m_node.SetAttribute(m_attributeInfo, m_newValue);
		}

		public override void Undo()
		{
			m_node.SetAttribute(m_attributeInfo, m_oldValue);
		}
	}

	public class ChildInsertedOperation : Operation
	{
		private readonly DomNode m_parent;

		private readonly ChildInfo m_childInfo;

		private readonly DomNode m_child;

		private readonly int m_index;

		public ChildInsertedOperation(ChildEventArgs e)
		{
			m_parent = e.Parent;
			m_child = e.Child;
			m_childInfo = e.ChildInfo;
			m_index = e.Index;
		}

		public override string ToString()
		{
			return $"Insert {m_child.Type} into {m_childInfo.Name} at index {m_index}";
		}

		public override void Do()
		{
			if (m_childInfo.IsList)
			{
				IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
				childList.Insert(m_index, m_child);
			}
			else
			{
				m_parent.SetChild(m_childInfo, m_child);
			}
		}

		public override void Undo()
		{
			if (m_childInfo.IsList)
			{
				IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
				childList.RemoveAt(m_index);
			}
			else
			{
				m_parent.SetChild(m_childInfo, null);
			}
		}
	}

	public class ChildRemovedOperation : Operation
	{
		private readonly DomNode m_parent;

		private readonly ChildInfo m_childInfo;

		private readonly DomNode m_child;

		private readonly int m_index;

		public ChildRemovedOperation(ChildEventArgs e)
		{
			m_parent = e.Parent;
			m_child = e.Child;
			m_childInfo = e.ChildInfo;
			m_index = e.Index;
		}

		public override string ToString()
		{
			return $"Remove {m_child.Type} from {m_childInfo.Name} at index {m_index}";
		}

		public ChildRemovedOperation(DomNode parent, DomNode child)
		{
			m_parent = parent;
			m_child = child;
			m_childInfo = child.ChildInfo;
			m_index = (m_childInfo.IsList ? parent.GetChildList(m_childInfo).IndexOf(child) : 0);
		}

		public override void Do()
		{
			if (m_childInfo.IsList)
			{
				IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
				childList.RemoveAt(m_index);
			}
			else
			{
				m_parent.SetChild(m_childInfo, null);
			}
		}

		public override void Undo()
		{
			if (m_childInfo.IsList)
			{
				IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
				childList.Insert(m_index, m_child);
			}
			else
			{
				m_parent.SetChild(m_childInfo, m_child);
			}
		}
	}

	private int m_transactionPauseCount = 0;

	private string m_transactionName;

	private List<Operation> m_transactionOperations;

	private bool m_requireTransactions;

	private bool m_transactionCancelled;

	public virtual bool InTransaction => m_transactionOperations != null;

	public virtual int PendingOperationCount => m_transactionOperations?.Count ?? 0;

	private bool TransactionsActive
	{
		get
		{
			return m_transactionPauseCount == 0;
		}
		set
		{
			if (value)
			{
				m_transactionPauseCount--;
			}
			else
			{
				m_transactionPauseCount++;
			}
			if (m_transactionPauseCount < 0)
			{
				throw new Exception("TransactionsActive turned off when they weren't on!");
			}
		}
	}

	public bool RequireTransactions
	{
		get
		{
			return m_requireTransactions;
		}
		set
		{
			m_requireTransactions = value;
		}
	}

	public IList<Operation> TransactionOperations => m_transactionOperations;

	public string TransactionName => m_transactionName;

	public event EventHandler Beginning;

	public event EventHandler Cancelled;

	public event EventHandler Ending;

	public event EventHandler Ended;

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.OnNodeSet();
	}

	public virtual void Begin(string transactionName)
	{
		if (InTransaction)
		{
			throw new InvalidOperationException("already in transaction");
		}
		m_transactionCancelled = false;
		m_transactionName = transactionName;
		Outputs.WriteLine(OutputMessageType.Diagnostic, "Transaction \"{0}\" started", m_transactionName);
		OnBeginning();
		this.Beginning.Raise(this, EventArgs.Empty);
		if (!m_transactionCancelled)
		{
			m_transactionOperations = new List<Operation>();
		}
	}

	protected virtual void OnBeginning()
	{
	}

	public virtual void Cancel()
	{
		OnCancelling();
		this.Cancelled.Raise(this, EventArgs.Empty);
		if (InTransaction)
		{
			for (int num = m_transactionOperations.Count - 1; num >= 0; num--)
			{
				m_transactionOperations[num].Undo();
			}
			Outputs.WriteLine(OutputMessageType.Diagnostic, "Transaction \"{0}\" canceled with {1} operations", m_transactionName, m_transactionOperations?.Count ?? 0);
			m_transactionOperations = null;
			m_transactionName = null;
		}
		OnCancelled();
		m_transactionCancelled = true;
	}

	protected virtual void OnCancelling()
	{
	}

	protected virtual void OnCancelled()
	{
	}

	public virtual void End()
	{
		if (InTransaction)
		{
			OnEnding();
			this.Ending.Raise(this, EventArgs.Empty);
			OnEnded();
			this.Ended.Raise(this, EventArgs.Empty);
			Outputs.WriteLine(OutputMessageType.Diagnostic, "Transaction \"{0}\" ended with {1} operations", m_transactionName, m_transactionOperations?.Count ?? 0);
			m_transactionOperations = null;
			m_transactionName = null;
		}
	}

	protected virtual void OnEnding()
	{
	}

	protected virtual void OnEnded()
	{
	}

	public virtual TransactionSuspensionReceipt SuspendTransactions()
	{
		return new TransactionSuspensionReceiptImpl(this);
	}

	public virtual void AddOperation(Operation operation)
	{
		if (InTransaction)
		{
			m_transactionOperations.Add(operation);
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (TransactionsActive && CheckTransaction())
		{
			AddOperation(new AttributeChangedOperation(e));
		}
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		if (TransactionsActive && CheckTransaction())
		{
			AddOperation(new ChildInsertedOperation(e));
		}
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		if (TransactionsActive && CheckTransaction())
		{
			AddOperation(new ChildRemovedOperation(e));
		}
	}

	private bool CheckTransaction()
	{
		bool inTransaction = InTransaction;
		if (!inTransaction && m_requireTransactions)
		{
			throw new InvalidOperationException("data changed outside transaction");
		}
		return inTransaction;
	}
}
