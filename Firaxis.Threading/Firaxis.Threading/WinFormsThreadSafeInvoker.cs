using System;
using System.Windows.Forms;
using Firaxis.Error;

namespace Firaxis.Threading;

public class WinFormsThreadSafeInvoker : IThreadSafeInvoker
{
	public Control Control { get; private set; }

	public WinFormsThreadSafeInvoker(Control control)
	{
		if (control == null)
		{
			throw new ArgumentNullException("control");
		}
		Control = control;
	}

	public bool Invoke(Delegate method)
	{
		return SafeInvoke(method, null);
	}

	public bool Invoke(Delegate method, params object[] args)
	{
		if (args == null)
		{
			args = new object[1] { null };
		}
		return SafeInvoke(method, args);
	}

	private bool SafeInvoke(Delegate method, object[] args)
	{
		if ((object)method != null)
		{
			if (Control.InvokeRequired)
			{
				try
				{
					if (args != null)
					{
						Control.BeginInvoke(method, args);
					}
					else
					{
						Control.BeginInvoke(method);
					}
					return true;
				}
				catch (ObjectDisposedException exception)
				{
					ErrorHandling.Error(exception, "Control used for invoke target has been disposed.", ErrorLevel.Log);
				}
				catch (InvalidOperationException exception2)
				{
					ErrorHandling.Error(exception2, "A control may have been assigned to an invoke target before its window handle had been created.  Do not assign a control to an invoke target in that controls constructor!  Wait for the control to load.", ErrorLevel.SendReport);
				}
			}
			else if (args != null)
			{
				method.DynamicInvoke(args);
			}
			else
			{
				method.DynamicInvoke();
			}
		}
		return false;
	}
}
