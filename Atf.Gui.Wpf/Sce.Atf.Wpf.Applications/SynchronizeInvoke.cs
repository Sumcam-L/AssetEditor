using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows.Threading;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(ISynchronizeInvoke))]
public class SynchronizeInvoke : ISynchronizeInvoke, IInitializable
{
	private class DispatcherAsyncResultAdapter : IAsyncResult
	{
		private DispatcherOperation m_op;

		private object m_state;

		public DispatcherOperation Operation => m_op;

		public object AsyncState => m_state;

		public WaitHandle AsyncWaitHandle => null;

		public bool CompletedSynchronously => false;

		public bool IsCompleted => m_op.Status == DispatcherOperationStatus.Completed;

		public DispatcherAsyncResultAdapter(DispatcherOperation operation)
		{
			m_op = operation;
		}

		public DispatcherAsyncResultAdapter(DispatcherOperation operation, object state)
			: this(operation)
		{
			m_state = state;
		}
	}

	private Dispatcher m_uiDispatcher;

	public bool InvokeRequired => m_uiDispatcher.Thread != Thread.CurrentThread;

	public void Initialize()
	{
		m_uiDispatcher = Dispatcher.CurrentDispatcher;
	}

	public IAsyncResult BeginInvoke(Delegate method, object[] args)
	{
		if (args != null && args.Length > 1)
		{
			object[] argsAfterFirst = GetArgsAfterFirst(args);
			DispatcherOperation operation = m_uiDispatcher.BeginInvoke(DispatcherPriority.Normal, method, args[0], argsAfterFirst);
			return new DispatcherAsyncResultAdapter(operation);
		}
		if (args != null)
		{
			return new DispatcherAsyncResultAdapter(m_uiDispatcher.BeginInvoke(DispatcherPriority.Normal, method, args[0]));
		}
		return new DispatcherAsyncResultAdapter(m_uiDispatcher.BeginInvoke(DispatcherPriority.Normal, method));
	}

	public object EndInvoke(IAsyncResult result)
	{
		if (!(result is DispatcherAsyncResultAdapter dispatcherAsyncResultAdapter))
		{
			throw new InvalidCastException();
		}
		while (dispatcherAsyncResultAdapter.Operation.Status != DispatcherOperationStatus.Completed || dispatcherAsyncResultAdapter.Operation.Status == DispatcherOperationStatus.Aborted)
		{
			Thread.Sleep(50);
		}
		return dispatcherAsyncResultAdapter.Operation.Result;
	}

	public object Invoke(Delegate method, object[] args)
	{
		if (args != null && args.Length > 1)
		{
			object[] argsAfterFirst = GetArgsAfterFirst(args);
			return m_uiDispatcher.Invoke(DispatcherPriority.Normal, method, args[0], argsAfterFirst);
		}
		if (args != null)
		{
			return m_uiDispatcher.Invoke(DispatcherPriority.Normal, method, args[0]);
		}
		return m_uiDispatcher.Invoke(DispatcherPriority.Normal, method);
	}

	private static object[] GetArgsAfterFirst(object[] args)
	{
		object[] array = new object[args.Length - 1];
		Array.Copy(args, 1, array, 0, args.Length - 1);
		return array;
	}
}
