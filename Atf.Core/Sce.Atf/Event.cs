using System;
using System.ComponentModel;

namespace Sce.Atf;

public static class Event
{
	public static void Raise(this EventHandler handler, object sender, EventArgs e)
	{
		handler?.Invoke(sender, e);
	}

	public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
	{
		handler?.Invoke(sender, e);
	}

	public static bool RaiseCancellable(this CancelEventHandler handler, object sender, CancelEventArgs e)
	{
		if (handler != null)
		{
			Delegate[] invocationList = handler.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				CancelEventHandler cancelEventHandler = (CancelEventHandler)invocationList[i];
				cancelEventHandler(sender, e);
				if (e.Cancel)
				{
					break;
				}
			}
		}
		return e.Cancel;
	}

	public static bool RaiseCancellable<T>(this EventHandler<T> handler, object sender, T e) where T : CancelEventArgs
	{
		if (handler != null)
		{
			Delegate[] invocationList = handler.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				EventHandler<T> eventHandler = (EventHandler<T>)invocationList[i];
				eventHandler(sender, e);
				if (e.Cancel)
				{
					break;
				}
			}
		}
		return e.Cancel;
	}
}
