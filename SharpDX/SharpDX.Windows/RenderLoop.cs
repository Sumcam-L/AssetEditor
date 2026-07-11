using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX.Win32;

namespace SharpDX.Windows;

public class RenderLoop : IDisposable
{
	public delegate void RenderCallback();

	private IntPtr controlHandle;

	private Control control;

	private bool isControlAlive;

	private bool switchControl;

	public Control Control
	{
		get
		{
			return control;
		}
		set
		{
			if (control != value)
			{
				if (control != null && !switchControl)
				{
					isControlAlive = false;
					control.Disposed -= ControlDisposed;
					controlHandle = IntPtr.Zero;
				}
				if (value != null && value.IsDisposed)
				{
					throw new InvalidOperationException("Control is already disposed");
				}
				control = value;
				switchControl = true;
			}
		}
	}

	public bool UseApplicationDoEvents { get; set; }

	public static bool IsIdle
	{
		get
		{
			NativeMessage lpMsg;
			return Win32Native.PeekMessage(out lpMsg, IntPtr.Zero, 0, 0, 0) == 0;
		}
	}

	public RenderLoop()
	{
	}

	public RenderLoop(Control control)
	{
		Control = control;
	}

	public bool NextFrame()
	{
		if (switchControl && control != null)
		{
			controlHandle = control.Handle;
			control.Disposed += ControlDisposed;
			isControlAlive = true;
			switchControl = false;
		}
		if (isControlAlive)
		{
			if (UseApplicationDoEvents)
			{
				Application.DoEvents();
			}
			else
			{
				IntPtr intPtr = controlHandle;
				if (intPtr != IntPtr.Zero)
				{
					NativeMessage lpMsg;
					while (Win32Native.PeekMessage(out lpMsg, IntPtr.Zero, 0, 0, 0) != 0)
					{
						if (Win32Native.GetMessage(out lpMsg, IntPtr.Zero, 0, 0) == -1)
						{
							throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "An error happened in rendering loop while processing windows messages. Error: {0}", new object[1] { Marshal.GetLastWin32Error() }));
						}
						if (lpMsg.msg == 130)
						{
							isControlAlive = false;
						}
						Message message = new Message
						{
							HWnd = lpMsg.handle,
							LParam = lpMsg.lParam,
							Msg = (int)lpMsg.msg,
							WParam = lpMsg.wParam
						};
						if (!Application.FilterMessage(ref message))
						{
							Win32Native.TranslateMessage(ref lpMsg);
							Win32Native.DispatchMessage(ref lpMsg);
						}
					}
				}
			}
		}
		if (!isControlAlive)
		{
			return switchControl;
		}
		return true;
	}

	private void ControlDisposed(object sender, EventArgs e)
	{
		isControlAlive = false;
	}

	public void Dispose()
	{
		Control = null;
	}

	public static void Run(ApplicationContext context, RenderCallback renderCallback)
	{
		Run(context.MainForm, renderCallback);
	}

	public static void Run(Control form, RenderCallback renderCallback, bool useApplicationDoEvents = false)
	{
		if (form == null)
		{
			throw new ArgumentNullException("form");
		}
		if (renderCallback == null)
		{
			throw new ArgumentNullException("renderCallback");
		}
		form.Show();
		RenderLoop renderLoop = new RenderLoop(form);
		renderLoop.UseApplicationDoEvents = useApplicationDoEvents;
		using RenderLoop renderLoop2 = renderLoop;
		while (renderLoop2.NextFrame())
		{
			renderCallback();
		}
	}
}
