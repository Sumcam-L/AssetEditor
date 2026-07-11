using System;
using System.Windows.Threading;

namespace Sce.Atf.Wpf;

public static class DispatcherExtensions
{
	public static void InvokeIfRequired(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
	{
		if (!dispatcher.CheckAccess())
		{
			dispatcher.Invoke(priority, action);
		}
		else
		{
			action();
		}
	}

	public static void InvokeIfRequired(this Dispatcher dispatcher, Action action)
	{
		if (!dispatcher.CheckAccess())
		{
			dispatcher.Invoke(DispatcherPriority.Normal, action);
		}
		else
		{
			action();
		}
	}

	public static void InvokeIfRequired<T>(this Dispatcher dispatcher, Action<T> action, T arg)
	{
		if (!dispatcher.CheckAccess())
		{
			dispatcher.Invoke(DispatcherPriority.Normal, action, arg);
		}
		else
		{
			action(arg);
		}
	}

	public static TResult InvokeIfRequired<TResult>(this Dispatcher dispatcher, Func<TResult> action)
	{
		if (!dispatcher.CheckAccess())
		{
			return (TResult)dispatcher.Invoke(DispatcherPriority.Normal, action);
		}
		return action();
	}

	public static TResult InvokeIfRequired<T, TResult>(this Dispatcher dispatcher, Func<T, TResult> action, T arg)
	{
		if (!dispatcher.CheckAccess())
		{
			return (TResult)dispatcher.Invoke(DispatcherPriority.Normal, action, arg);
		}
		return action(arg);
	}

	public static void BeginInvokeIfRequired(this Dispatcher dispatcher, Action action, DispatcherPriority priority)
	{
		if (!dispatcher.CheckAccess())
		{
			dispatcher.BeginInvoke(priority, action);
		}
		else
		{
			action();
		}
	}

	public static void BeginInvokeIfRequired(this Dispatcher dispatcher, Action action)
	{
		if (!dispatcher.CheckAccess())
		{
			dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
		}
		else
		{
			action();
		}
	}

	public static void BeginInvokeIfRequired<T>(this Dispatcher dispatcher, Action<T> action, T arg)
	{
		if (!dispatcher.CheckAccess())
		{
			dispatcher.BeginInvoke(DispatcherPriority.Normal, action, arg);
		}
		else
		{
			action(arg);
		}
	}

	public static void WaitForPriority(this Dispatcher dispatcher, DispatcherPriority priority)
	{
		DispatcherFrame dispatcherFrame = new DispatcherFrame();
		DispatcherOperation dispatcherOperation = dispatcher.BeginInvoke(priority, new DispatcherOperationCallback(ExitFrameOperation), dispatcherFrame);
		Dispatcher.PushFrame(dispatcherFrame);
		if (dispatcherOperation.Status != DispatcherOperationStatus.Completed)
		{
			dispatcherOperation.Abort();
		}
	}

	private static object ExitFrameOperation(object obj)
	{
		((DispatcherFrame)obj).Continue = false;
		return null;
	}
}
