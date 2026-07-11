using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX.Collections;

namespace SharpDX.Win32;

public class MessageFilterHook
{
	private static readonly Dictionary<IntPtr, MessageFilterHook> RegisteredHooks = new Dictionary<IntPtr, MessageFilterHook>(EqualityComparer.DefaultIntPtr);

	private readonly IntPtr defaultWndProc;

	private readonly IntPtr hwnd;

	private readonly Win32Native.WndProc newWndProc;

	private readonly IntPtr newWndProcPtr;

	private List<IMessageFilter> currentFilters;

	private bool isDisposed;

	private MessageFilterHook(IntPtr hwnd)
	{
		currentFilters = new List<IMessageFilter>();
		this.hwnd = hwnd;
		defaultWndProc = Win32Native.GetWindowLong(new HandleRef(this, hwnd), Win32Native.WindowLongType.WndProc);
		newWndProc = WndProc;
		newWndProcPtr = Marshal.GetFunctionPointerForDelegate((Delegate)newWndProc);
		Win32Native.SetWindowLong(new HandleRef(this, hwnd), Win32Native.WindowLongType.WndProc, newWndProcPtr);
	}

	public static void AddMessageFilter(IntPtr hwnd, IMessageFilter messageFilter)
	{
		lock (RegisteredHooks)
		{
			hwnd = GetSafeWindowHandle(hwnd);
			if (!RegisteredHooks.TryGetValue(hwnd, out var value))
			{
				value = new MessageFilterHook(hwnd);
				RegisteredHooks.Add(hwnd, value);
			}
			value.AddMessageMilter(messageFilter);
		}
	}

	public static void RemoveMessageFilter(IntPtr hwnd, IMessageFilter messageFilter)
	{
		lock (RegisteredHooks)
		{
			hwnd = GetSafeWindowHandle(hwnd);
			if (RegisteredHooks.TryGetValue(hwnd, out var value))
			{
				value.RemoveMessageFilter(messageFilter);
				if (value.isDisposed)
				{
					RegisteredHooks.Remove(hwnd);
					value.RestoreWndProc();
				}
			}
		}
	}

	private void AddMessageMilter(IMessageFilter filter)
	{
		List<IMessageFilter> list = new List<IMessageFilter>(currentFilters);
		if (!list.Contains(filter))
		{
			list.Add(filter);
		}
		currentFilters = list;
	}

	private void RemoveMessageFilter(IMessageFilter filter)
	{
		List<IMessageFilter> list = new List<IMessageFilter>(currentFilters);
		list.Remove(filter);
		if (list.Count == 0)
		{
			isDisposed = true;
			RestoreWndProc();
		}
		currentFilters = list;
	}

	private void RestoreWndProc()
	{
		IntPtr windowLong = Win32Native.GetWindowLong(new HandleRef(this, hwnd), Win32Native.WindowLongType.WndProc);
		if (windowLong == newWndProcPtr)
		{
			Win32Native.SetWindowLong(new HandleRef(this, hwnd), Win32Native.WindowLongType.WndProc, defaultWndProc);
		}
	}

	private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
	{
		if (isDisposed)
		{
			RestoreWndProc();
		}
		else
		{
			Message m = new Message
			{
				HWnd = hwnd,
				LParam = lParam,
				Msg = msg,
				WParam = wParam
			};
			foreach (IMessageFilter currentFilter in currentFilters)
			{
				if (currentFilter.PreFilterMessage(ref m))
				{
					return m.Result;
				}
			}
		}
		return Win32Native.CallWindowProc(defaultWndProc, hWnd, msg, wParam, lParam);
	}

	private static IntPtr GetSafeWindowHandle(IntPtr hwnd)
	{
		if (!(hwnd == IntPtr.Zero))
		{
			return hwnd;
		}
		return Process.GetCurrentProcess().MainWindowHandle;
	}
}
