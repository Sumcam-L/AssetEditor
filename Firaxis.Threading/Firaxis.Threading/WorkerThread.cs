using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Firaxis.TypeGeneration;

namespace Firaxis.Threading;

public abstract class WorkerThread
{
	private Dictionary<MethodInfo, Type> CachedDelegateTypes = new Dictionary<MethodInfo, Type>();

	private Thread Thread;

	private InvokeQueue InvokeQueue;

	private ManualResetEvent ManualResetEvent;

	private bool ShutdownRequested = false;

	public IThreadSafeInvoker ThreadSafeInvoker => InvokeQueue;

	protected WorkerThread(string sThreadName)
		: this(sThreadName, bIsBackgroundThread: false)
	{
	}

	protected WorkerThread(string sThreadName, bool bIsBackgroundThread)
	{
		Thread = new Thread(ThreadProc);
		Thread.Name = sThreadName;
		Thread.IsBackground = bIsBackgroundThread;
		ManualResetEvent = new ManualResetEvent(initialState: false);
		InvokeQueue = new InvokeQueue(ManualResetEvent);
	}

	protected void Start()
	{
		lock (InvokeQueue)
		{
			ShutdownRequested = false;
			Thread.Start();
		}
	}

	protected void Stop(bool bBlock)
	{
		lock (InvokeQueue)
		{
			ShutdownRequested = true;
			ManualResetEvent.Set();
			if (bBlock)
			{
				while (Thread.IsAlive)
				{
					Thread.Sleep(1);
				}
			}
		}
	}

	protected bool SynchronizeCall()
	{
		if (Thread.CurrentThread != Thread)
		{
			Delegate method = CreateDelegate(new StackFrame(1));
			InvokeQueue.Invoke(method);
			return true;
		}
		return false;
	}

	protected bool SynchronizeCall(params object[] args)
	{
		if (Thread.CurrentThread != Thread)
		{
			Delegate method = CreateDelegate(new StackFrame(1));
			InvokeQueue.Invoke(method, args);
			return true;
		}
		return false;
	}

	private Delegate CreateDelegate(StackFrame frame)
	{
		MethodInfo methodInfo = frame.GetMethod() as MethodInfo;
		if (methodInfo != null)
		{
			Type value = null;
			lock (CachedDelegateTypes)
			{
				if (!CachedDelegateTypes.TryGetValue(methodInfo, out value))
				{
					value = DelegateTypeGenerator.CreateDelegateType(methodInfo);
					CachedDelegateTypes[methodInfo] = value;
				}
			}
			return Delegate.CreateDelegate(value, this, methodInfo);
		}
		return null;
	}

	private void ThreadProc()
	{
		ManualResetEvent.WaitOne();
		while (!ShutdownRequested)
		{
			InvokeQueue.DispatchInvoke();
			ManualResetEvent.WaitOne();
		}
	}
}
