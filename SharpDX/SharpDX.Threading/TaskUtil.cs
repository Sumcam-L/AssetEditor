using System;
using System.Threading;

namespace SharpDX.Threading;

public class TaskUtil
{
	public static void Run(Action action, string taskName = "SharpDXTask")
	{
		Thread thread = new Thread((ThreadStart)delegate
		{
			action();
		});
		thread.IsBackground = true;
		thread.Name = taskName;
		Thread thread2 = thread;
		thread2.Start();
	}
}
