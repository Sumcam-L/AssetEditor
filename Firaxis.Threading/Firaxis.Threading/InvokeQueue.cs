using System;
using System.Collections.Generic;
using System.Threading;

namespace Firaxis.Threading;

public class InvokeQueue : IThreadSafeInvoker
{
	private struct InvokeEnty
	{
		public Delegate Method;

		public object[] Arguments;

		public InvokeEnty(Delegate method)
		{
			Method = method;
			Arguments = null;
		}

		public InvokeEnty(Delegate method, object[] args)
		{
			Method = method;
			Arguments = args;
		}

		public void Invoke()
		{
			if (Arguments != null)
			{
				Method.DynamicInvoke(Arguments);
			}
			else
			{
				Method.DynamicInvoke();
			}
		}
	}

	private int PendingCount = 0;

	private Queue<InvokeEnty> Queue = new Queue<InvokeEnty>();

	public ManualResetEvent ManualResetEvent { get; private set; }

	public bool PendingInvokes
	{
		get
		{
			lock (Queue)
			{
				return Queue.Count > 0;
			}
		}
	}

	public bool PendingCompletion => PendingCount > 0;

	public InvokeQueue()
	{
		ManualResetEvent = null;
	}

	public InvokeQueue(ManualResetEvent manualResetEvent)
	{
		ManualResetEvent = manualResetEvent;
	}

	public bool Invoke(Delegate method)
	{
		if ((object)method != null)
		{
			Interlocked.Increment(ref PendingCount);
			lock (Queue)
			{
				Queue.Enqueue(new InvokeEnty(method));
				if (ManualResetEvent != null)
				{
					ManualResetEvent.Set();
				}
			}
			return true;
		}
		return false;
	}

	public bool Invoke(Delegate method, params object[] args)
	{
		if ((object)method != null)
		{
			if (args == null)
			{
				args = new object[1] { null };
			}
			Interlocked.Increment(ref PendingCount);
			lock (Queue)
			{
				Queue.Enqueue(new InvokeEnty(method, args));
				if (ManualResetEvent != null)
				{
					ManualResetEvent.Set();
				}
			}
			return true;
		}
		return false;
	}

	public void DispatchInvoke()
	{
		InvokeEnty? invokeEnty = null;
		lock (Queue)
		{
			if (Queue.Count != 0)
			{
				invokeEnty = Queue.Dequeue();
			}
			else if (ManualResetEvent != null)
			{
				ManualResetEvent.Reset();
			}
		}
		if (invokeEnty.HasValue)
		{
			invokeEnty.Value.Invoke();
			Interlocked.Decrement(ref PendingCount);
		}
	}
}
