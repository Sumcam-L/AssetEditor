using System;
using System.Threading;

namespace Firaxis.Threading;

public class JobManager
{
	private delegate void ExecuteJobDelegate();

	private InvokeQueue InvokeQueue;

	private ManualResetEvent ManualResetEvent;

	private Thread[] Threads;

	private bool ShuttingDown = false;

	public bool Running => Threads != null;

	public JobManager()
	{
		ManualResetEvent = new ManualResetEvent(initialState: false);
		InvokeQueue = new InvokeQueue(ManualResetEvent);
	}

	public void Startup(uint uiThreadCount)
	{
		lock (this)
		{
			if (!Running)
			{
				Thread[] array = new Thread[uiThreadCount];
				for (int i = 0; i < uiThreadCount; i++)
				{
					Thread thread = new Thread(ThreadProc);
					thread.Name = "Job Manager Thread " + i;
					array[i] = thread;
				}
				Threads = array;
				for (int j = 0; j < uiThreadCount; j++)
				{
					Threads[j].Start();
				}
			}
		}
	}

	public void ProcessJob()
	{
		InvokeQueue.DispatchInvoke();
	}

	public void WaitForAll()
	{
		while (InvokeQueue.PendingCompletion)
		{
			while (InvokeQueue.PendingInvokes)
			{
				InvokeQueue.DispatchInvoke();
			}
			Thread.Sleep(1);
		}
	}

	public void Shutdown()
	{
		lock (this)
		{
			if (!Running)
			{
				return;
			}
			ShuttingDown = true;
			ManualResetEvent.Set();
			Thread[] threads = Threads;
			Threads = null;
			bool flag;
			do
			{
				flag = true;
				Thread[] array = threads;
				foreach (Thread thread in array)
				{
					if (thread.IsAlive)
					{
						flag = false;
						Thread.Sleep(1);
						break;
					}
				}
			}
			while (!flag);
			ShuttingDown = false;
		}
	}

	public void AddJob(IJob job)
	{
		if (job != null)
		{
			Delegate method = new ExecuteJobDelegate(job.Execute);
			InvokeQueue.Invoke(method);
		}
	}

	public void AddJob(Action job)
	{
		if (job != null)
		{
			Delegate method = new ExecuteJobDelegate(job.Invoke);
			InvokeQueue.Invoke(method);
		}
	}

	private void ThreadProc()
	{
		ManualResetEvent.WaitOne();
		while (!ShuttingDown)
		{
			InvokeQueue.DispatchInvoke();
			ManualResetEvent.WaitOne();
		}
	}
}
