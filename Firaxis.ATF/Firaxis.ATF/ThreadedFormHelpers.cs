using System;
using System.Windows.Forms;

namespace Firaxis.ATF;

internal static class ThreadedFormHelpers
{
	public static void BeginInvokeIfPossible(this Control target, Action action)
	{
		if (target == null || target.IsDisposed)
		{
			return;
		}
		if (target.InvokeRequired)
		{
			if (target.IsHandleCreated)
			{
				target.BeginInvoke(action);
			}
		}
		else
		{
			action();
		}
	}

	public static void InvokeIfRequired(this Control target, Action action)
	{
		if (target == null || target.IsDisposed)
		{
			return;
		}
		if (target.InvokeRequired)
		{
			if (target.IsHandleCreated)
			{
				target.Invoke(action);
			}
		}
		else
		{
			action();
		}
	}
}
