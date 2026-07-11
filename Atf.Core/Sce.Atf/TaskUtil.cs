using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Sce.Atf;

public class TaskUtil
{
	public static class FireAndForgetTask
	{
		private static readonly Action<Exception> DefaultErrorHandler = delegate(Exception ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
		};

		public static void Run(Action action, Action<Exception> exHandler = null)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			Task task = Task.Factory.StartNew(action);
			task.ContinueWith(delegate(Task t)
			{
				(exHandler ?? DefaultErrorHandler)(t.Exception.GetBaseException());
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
		}
	}
}
