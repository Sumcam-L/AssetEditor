using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom;

public class HistoryContext : TransactionContext, IHistoryContext, IHistoryContextRefCount
{
	private class Suspender : IDisposable
	{
		private bool disposedValue = false;

		private IHistoryContextRefCount RefCounter { get; set; }

		public Suspender(IHistoryContextRefCount refCounter)
		{
			RefCounter = refCounter;
			RefCounter.Suspend();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					RefCounter?.Resume();
					RefCounter = null;
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}

	private class TransactionCommand : TransactionCommandBase
	{
		private readonly HistoryContext m_context;

		private readonly Operation[] m_operations;

		private readonly SetSelectionCommand m_setSelectionCommand;

		public override IEnumerable<Operation> Operations => m_operations;

		public TransactionCommand(HistoryContext context, string name, Operation[] operations, SetSelectionCommand setSelectionCommand)
			: base(name)
		{
			m_context = context;
			m_operations = operations;
			m_setSelectionCommand = setSelectionCommand;
			base.ToolTip = string.Join("\n", (IEnumerable<Operation>)m_operations);
		}

		public override void Do()
		{
			m_context.DoTransaction(delegate
			{
				Operation[] operations = m_operations;
				foreach (Operation operation in operations)
				{
					operation.Do();
				}
			}, base.Description);
			if (m_setSelectionCommand != null)
			{
				m_setSelectionCommand.Do();
			}
		}

		public override void Undo()
		{
			m_context.DoTransaction(delegate
			{
				for (int num = m_operations.Length - 1; num >= 0; num--)
				{
					m_operations[num].Undo();
				}
			}, base.Description);
			if (m_setSelectionCommand != null)
			{
				m_setSelectionCommand.Undo();
			}
		}
	}

	private readonly Dictionary<Pair<DomNode, AttributeInfo>, AttributeChangedOperation> m_pendingChanges = new Dictionary<Pair<DomNode, AttributeInfo>, AttributeChangedOperation>();

	private DateTime m_lastSetOperationTime;

	private TimeSpan m_pendingSetOperationLifetime = new TimeSpan(0, 0, 0, 0, 500);

	private CommandHistory m_history;

	private ISelectionContext m_selectionContext;

	private object[] m_lastSelection;

	private bool m_undoingOrRedoing;

	public CommandHistory History
	{
		get
		{
			return m_history;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_history != value)
			{
				if (m_history != null)
				{
					m_history.CommandUndone -= history_CommandUndone;
					m_history.DirtyChanged -= history_DirtyChanged;
				}
				m_history = value;
				if (m_history != null)
				{
					m_history.CommandUndone += history_CommandUndone;
					m_history.DirtyChanged += history_DirtyChanged;
				}
			}
		}
	}

	public bool UndoSelection { get; set; }

	public bool Recording => SuspendCount == 0;

	private int SuspendCount { get; set; } = 0;

	public bool UndoingOrRedoing
	{
		get
		{
			return m_undoingOrRedoing;
		}
		internal set
		{
			m_undoingOrRedoing = value;
		}
	}

	public TimeSpan PendingSetOperationLifetime
	{
		get
		{
			return m_pendingSetOperationLifetime;
		}
		set
		{
			m_pendingSetOperationLifetime = value;
		}
	}

	public virtual bool CanUndo => m_history.CanUndo;

	public virtual bool CanRedo => m_history.CanRedo;

	public virtual string UndoDescription => m_history.UndoDescription;

	public virtual string RedoDescription => m_history.RedoDescription;

	public virtual bool Dirty
	{
		get
		{
			return m_history.Dirty;
		}
		set
		{
			m_history.Dirty = value;
		}
	}

	public event EventHandler DirtyChanged;

	public HistoryContext()
	{
		History = new CommandHistory();
		UndoSelection = true;
	}

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		if (UndoSelection)
		{
			m_selectionContext = this.As<ISelectionContext>();
		}
	}

	public IDisposable SuspendRecording()
	{
		return new Suspender(this);
	}

	void IHistoryContextRefCount.Suspend()
	{
		int suspendCount = SuspendCount + 1;
		SuspendCount = suspendCount;
	}

	void IHistoryContextRefCount.Resume()
	{
		int suspendCount = SuspendCount - 1;
		SuspendCount = suspendCount;
	}

	public virtual void Clear()
	{
		m_history.Clear();
	}

	public virtual void Undo()
	{
		DomNode domNode = base.DomNode.Lineage.FirstOrDefault((DomNode x) => x.Is<GlobalHistoryContext>());
		try
		{
			m_undoingOrRedoing = true;
			domNode?.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
			m_history.Undo();
		}
		finally
		{
			m_undoingOrRedoing = false;
			domNode?.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
		}
	}

	public virtual void Redo()
	{
		DomNode domNode = base.DomNode.Lineage.FirstOrDefault((DomNode x) => x.Is<GlobalHistoryContext>());
		try
		{
			m_undoingOrRedoing = true;
			domNode?.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
			m_history.Redo();
		}
		finally
		{
			m_undoingOrRedoing = false;
			domNode?.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
		}
	}

	protected override void OnBeginning()
	{
		m_lastSelection = SnapshotSelection();
		base.OnBeginning();
	}

	protected override void OnEnded()
	{
		if (m_undoingOrRedoing || !Recording)
		{
			return;
		}
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = utcNow.Subtract(m_lastSetOperationTime);
		m_lastSetOperationTime = utcNow;
		if (timeSpan > PendingSetOperationLifetime)
		{
			m_pendingChanges.Clear();
		}
		IList<Operation> transactionOperations = base.TransactionOperations;
		int num = 0;
		while (num < transactionOperations.Count)
		{
			Operation operation = transactionOperations[num];
			if (operation is AttributeChangedOperation attributeChangedOperation)
			{
				Pair<DomNode, AttributeInfo> key = new Pair<DomNode, AttributeInfo>(attributeChangedOperation.DomNode, attributeChangedOperation.AttributeInfo);
				if (m_pendingChanges.TryGetValue(key, out var value))
				{
					value.NewValue = attributeChangedOperation.NewValue;
					transactionOperations.RemoveAt(num);
					continue;
				}
				m_pendingChanges.Add(key, attributeChangedOperation);
			}
			num++;
		}
		if (transactionOperations.Count > 0)
		{
			SetSelectionCommand setSelectionCommand = null;
			if (m_selectionContext != null)
			{
				setSelectionCommand = new SetSelectionCommand(m_selectionContext, m_lastSelection, SnapshotSelection());
			}
			m_history.Add(new TransactionCommand(this, base.TransactionName, transactionOperations.ToArray(), setSelectionCommand));
		}
		m_lastSelection = null;
		base.OnEnded();
	}

	private void history_CommandUndone(object sender, EventArgs e)
	{
		m_pendingChanges.Clear();
	}

	private void history_DirtyChanged(object sender, EventArgs e)
	{
		this.DirtyChanged.Raise(this, e);
	}

	private object[] SnapshotSelection()
	{
		if (m_selectionContext != null)
		{
			return m_selectionContext.GetSelection<object>().ToArray();
		}
		return EmptyArray<object>.Instance;
	}
}
