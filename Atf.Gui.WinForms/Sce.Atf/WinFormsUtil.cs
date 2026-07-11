using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf;

public static class WinFormsUtil
{
	public delegate void FormEventHandler(Form form);

	private const int Margin = 10;

	private static readonly List<FormEventHandler> s_windowCreatedHandlers = new List<FormEventHandler>();

	private static readonly List<FormEventHandler> s_windowDestroyedHandlers = new List<FormEventHandler>();

	private static readonly List<ControlEventHandler> s_controlCreatedHandlers = new List<ControlEventHandler>();

	private static readonly List<ControlEventHandler> s_controlDestroyedHandlers = new List<ControlEventHandler>();

	private static readonly List<int> s_handlesCreatedBeforeForms = new List<int>();

	private static readonly User32.WindowsHookCallback s_callbackDelegate = ShellHookCallback;

	private static IntPtr s_windowsHookHandle = IntPtr.Zero;

	private static int NumShellListeners => s_windowCreatedHandlers.Count + s_windowDestroyedHandlers.Count + s_controlCreatedHandlers.Count + s_controlDestroyedHandlers.Count;

	public static event FormEventHandler WindowCreated
	{
		add
		{
			if (!s_windowCreatedHandlers.Contains(value))
			{
				s_windowCreatedHandlers.Add(value);
			}
			CheckShellProc();
		}
		remove
		{
			s_windowCreatedHandlers.Remove(value);
			CheckShellProc();
		}
	}

	public static event FormEventHandler WindowDestroyed
	{
		add
		{
			if (!s_windowDestroyedHandlers.Contains(value))
			{
				s_windowDestroyedHandlers.Add(value);
			}
			CheckShellProc();
		}
		remove
		{
			s_windowDestroyedHandlers.Remove(value);
			CheckShellProc();
		}
	}

	public static event ControlEventHandler ControlCreated
	{
		add
		{
			if (!s_controlCreatedHandlers.Contains(value))
			{
				s_controlCreatedHandlers.Add(value);
			}
			CheckShellProc();
		}
		remove
		{
			s_controlCreatedHandlers.Remove(value);
			CheckShellProc();
		}
	}

	public static event ControlEventHandler ControlDestroyed
	{
		add
		{
			if (!s_controlDestroyedHandlers.Contains(value))
			{
				s_controlDestroyedHandlers.Add(value);
			}
			CheckShellProc();
		}
		remove
		{
			s_controlDestroyedHandlers.Remove(value);
			CheckShellProc();
		}
	}

	public static bool IsOnScreen(Rectangle rect)
	{
		using Region region = new Region();
		region.MakeEmpty();
		Screen[] allScreens = Screen.AllScreens;
		foreach (Screen screen in allScreens)
		{
			region.Union(screen.WorkingArea);
		}
		rect.Inflate(-10, -10);
		return region.IsVisible(rect);
	}

	public static int CalculateDistance(Rectangle startRect, Keys arrow, Rectangle targetRect)
	{
		int num;
		int num2;
		int num3;
		int num4;
		int num5;
		int num6;
		int num7;
		switch (arrow)
		{
		case Keys.Up:
			num = startRect.Left;
			num2 = startRect.Right;
			num3 = -startRect.Top;
			num4 = targetRect.Left;
			num5 = targetRect.Right;
			num6 = -targetRect.Bottom;
			num7 = -targetRect.Top;
			break;
		case Keys.Right:
			num = startRect.Top;
			num2 = startRect.Bottom;
			num3 = startRect.Right;
			num4 = targetRect.Top;
			num5 = targetRect.Bottom;
			num6 = targetRect.Left;
			num7 = targetRect.Right;
			break;
		case Keys.Down:
			num = startRect.Left;
			num2 = startRect.Right;
			num3 = startRect.Bottom;
			num4 = targetRect.Left;
			num5 = targetRect.Right;
			num6 = targetRect.Top;
			num7 = targetRect.Bottom;
			break;
		case Keys.Left:
			num = startRect.Top;
			num2 = startRect.Bottom;
			num3 = -startRect.Left;
			num4 = targetRect.Top;
			num5 = targetRect.Bottom;
			num6 = -targetRect.Right;
			num7 = -targetRect.Right;
			break;
		default:
			throw new ArgumentException("'arrow' must be a single arrow key");
		}
		if (num3 > num7)
		{
			return int.MaxValue;
		}
		int num8 = num7 - num3;
		if (num5 < num - num8 || num4 > num2 + num8)
		{
			return int.MaxValue;
		}
		int num9 = num6 - num3;
		if (num5 < num)
		{
			int num10 = num - num5;
			return num10 * num10 + num9 * num9;
		}
		if (num4 > num2)
		{
			int num11 = num4 - num2;
			return num11 * num11 + num9 * num9;
		}
		return num9 * num9;
	}

	public static Control GetFocusedControl()
	{
		Control result = null;
		IntPtr focus = User32.GetFocus();
		if (focus != IntPtr.Zero)
		{
			result = Control.FromHandle(focus);
		}
		return result;
	}

	public static void CheckForIllegalCrossThreadCall(this Control control)
	{
		if (control != null && Control.CheckForIllegalCrossThreadCalls && control.InvokeRequired)
		{
			throw new InvalidOperationException("Illegal cross-thread call");
		}
	}

	public static void InvokeIfRequired(this Control control, Action action)
	{
		if (control != null && control.InvokeRequired)
		{
			control.Invoke(action);
		}
		else
		{
			action();
		}
	}

	public static void BeginInvokeIfRequired(this Control control, Action action)
	{
		if (control != null && control.InvokeRequired)
		{
			control.BeginInvoke(action);
		}
		else
		{
			action();
		}
	}

	public static bool UpdateScrollbars(VScrollBar vScrollBar, HScrollBar hScrollBar, Size visibleSize, Size canvasSize)
	{
		bool result = false;
		if (vScrollBar != null)
		{
			int num = visibleSize.Height;
			if (hScrollBar != null && hScrollBar.Visible)
			{
				num -= hScrollBar.Height;
			}
			num = Math.Max(1, num);
			int num2 = Math.Max(0, canvasSize.Height - num);
			result = vScrollBar.Visible != num2 > 0;
			vScrollBar.Visible = num2 > 0;
			int num3 = (vScrollBar.LargeChange = num);
			vScrollBar.Minimum = 0;
			int maximum = num2 + num3;
			vScrollBar.Maximum = maximum;
			vScrollBar.Value = Math.Min(vScrollBar.Value, num2);
		}
		if (hScrollBar != null)
		{
			int num5 = visibleSize.Width;
			if (vScrollBar != null && vScrollBar.Visible)
			{
				num5 -= vScrollBar.Width;
			}
			num5 = Math.Max(1, num5);
			int num6 = Math.Max(0, canvasSize.Width - num5);
			hScrollBar.Visible = num6 > 0;
			int num7 = (hScrollBar.LargeChange = num5);
			hScrollBar.Minimum = 0;
			int maximum2 = num6 + num7;
			hScrollBar.Maximum = maximum2;
			hScrollBar.Value = Math.Min(hScrollBar.Value, num6);
		}
		return result;
	}

	public static Rectangle UpdateScrollbars(VScrollBar vScrollBar, HScrollBar hScrollBar, Rectangle visibleArea, Rectangle contentArea)
	{
		Rectangle rectangle = Rectangle.Union(visibleArea, contentArea);
		int num = Math.Max(0, rectangle.Width - visibleArea.Width);
		int num2 = Math.Max(0, rectangle.Height - visibleArea.Height);
		bool flag = num > 0;
		if (vScrollBar != null && contentArea.Right > visibleArea.Right - vScrollBar.Width && num2 > 0)
		{
			flag = true;
		}
		if (hScrollBar != null && flag)
		{
			visibleArea.Height -= hScrollBar.Height;
			rectangle = Rectangle.Union(visibleArea, contentArea);
			num2 = Math.Max(0, rectangle.Height - visibleArea.Height);
		}
		if (vScrollBar != null)
		{
			if (num2 > 0)
			{
				visibleArea.Width -= vScrollBar.Width;
				rectangle = Rectangle.Union(visibleArea, contentArea);
				num = Math.Max(0, rectangle.Width - visibleArea.Width);
				vScrollBar.Visible = true;
				vScrollBar.LargeChange = Math.Max(visibleArea.Height, 1);
				vScrollBar.SmallChange = Math.Max(1, vScrollBar.LargeChange / 10);
				vScrollBar.Minimum = rectangle.Top;
				vScrollBar.Maximum = rectangle.Bottom;
			}
			else
			{
				vScrollBar.Visible = false;
			}
		}
		if (hScrollBar != null)
		{
			if (num > 0)
			{
				hScrollBar.Visible = true;
				hScrollBar.LargeChange = Math.Max(visibleArea.Width, 1);
				hScrollBar.SmallChange = Math.Max(1, hScrollBar.LargeChange / 10);
				hScrollBar.Minimum = rectangle.Left;
				hScrollBar.Maximum = rectangle.Right;
			}
			else
			{
				hScrollBar.Visible = false;
			}
		}
		return visibleArea;
	}

	public static Rectangle UpdateBounds(Rectangle origRect, Rectangle newRect, BoundsSpecified parts)
	{
		Rectangle result = origRect;
		if ((parts & BoundsSpecified.X) != BoundsSpecified.None)
		{
			result.X = newRect.X;
		}
		if ((parts & BoundsSpecified.Y) != BoundsSpecified.None)
		{
			result.Y = newRect.Y;
		}
		if ((parts & BoundsSpecified.Width) != BoundsSpecified.None)
		{
			result.Width = newRect.Width;
		}
		if ((parts & BoundsSpecified.Height) != BoundsSpecified.None)
		{
			result.Height = newRect.Height;
		}
		return result;
	}

	private static void OnWindowCreated(Form form)
	{
		foreach (FormEventHandler s_windowCreatedHandler in s_windowCreatedHandlers)
		{
			s_windowCreatedHandler(form);
		}
	}

	private static void OnWindowDestroyed(Form form)
	{
		foreach (FormEventHandler s_windowDestroyedHandler in s_windowDestroyedHandlers)
		{
			s_windowDestroyedHandler(form);
		}
	}

	private static void OnControlCreated(Control control)
	{
		foreach (ControlEventHandler s_controlCreatedHandler in s_controlCreatedHandlers)
		{
			s_controlCreatedHandler(null, new ControlEventArgs(control));
		}
	}

	private static void OnControlDestroyed(Control control)
	{
		foreach (ControlEventHandler s_controlDestroyedHandler in s_controlDestroyedHandlers)
		{
			s_controlDestroyedHandler(null, new ControlEventArgs(control));
		}
	}

	private static void CheckShellProc()
	{
		if (s_windowsHookHandle == IntPtr.Zero && NumShellListeners > 0)
		{
			int currentThreadId = AppDomain.GetCurrentThreadId();
			s_windowsHookHandle = User32.SetWindowsHookEx(User32.HookType.WH_CBT, s_callbackDelegate, IntPtr.Zero, currentThreadId);
		}
		else if (s_windowsHookHandle != IntPtr.Zero && NumShellListeners == 0)
		{
			User32.UnhookWindowsHookEx(s_windowsHookHandle);
			s_windowsHookHandle = IntPtr.Zero;
		}
	}

	private static int ShellHookCallback(int code, int wParam, int lParam)
	{
		int result = User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
		if (code < 0)
		{
			return result;
		}
		switch (code)
		{
		case 3:
			s_handlesCreatedBeforeForms.Add(wParam);
			break;
		case 4:
		{
			if (s_handlesCreatedBeforeForms.Remove(wParam))
			{
				break;
			}
			Control controlFromHandle2 = GetControlFromHandle(wParam);
			if (controlFromHandle2 != null)
			{
				OnControlDestroyed(controlFromHandle2);
				if (controlFromHandle2 is Form form2)
				{
					OnWindowDestroyed(form2);
				}
			}
			break;
		}
		case 5:
		case 9:
		{
			if (s_handlesCreatedBeforeForms.Count <= 0)
			{
				break;
			}
			int[] array = s_handlesCreatedBeforeForms.ToArray();
			s_handlesCreatedBeforeForms.Clear();
			int[] array2 = array;
			foreach (int handleInt in array2)
			{
				Control controlFromHandle = GetControlFromHandle(handleInt);
				if (controlFromHandle != null)
				{
					OnControlCreated(controlFromHandle);
					if (controlFromHandle is Form form)
					{
						OnWindowCreated(form);
					}
				}
			}
			break;
		}
		}
		return result;
	}

	private static Control GetControlFromHandle(int handleInt)
	{
		IntPtr handle = new IntPtr(handleInt);
		return Control.FromHandle(handle);
	}
}
