using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Firaxis.Utility;

public class NativeWindow : IDisposable
{
	private IntPtr windowHandle;

	private NativeMethods.WndProc windowProc;

	private NativeMethods.WNDCLASS wndClass;

	public NativeMethods.WNDCLASS WNDCLASS => wndClass;

	public HandleRef WindowHandle => new HandleRef(this, windowHandle);

	public NativeWindow()
	{
		wndClass = new NativeMethods.WNDCLASS();
		windowProc = WindowProc;
		wndClass.lpfnWndProc = windowProc;
		windowHandle = IntPtr.Zero;
	}

	public void Dispose()
	{
		if (windowHandle != IntPtr.Zero)
		{
			Destroy();
		}
	}

	public void Destroy()
	{
		NativeMethods.DestroyWindow(windowHandle);
		windowHandle = IntPtr.Zero;
	}

	public void MoveWindow(int x, int y, int width, int height)
	{
		NativeMethods.MoveWindow(windowHandle, x, y, width, height, redraw: true);
	}

	public void SetFont(Font font)
	{
		NativeMethods.SendMessage(new HandleRef(this, windowHandle), 48, font.ToHfont(), new IntPtr(1));
	}

	public IntPtr CreateWindow(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, Control parent)
	{
		NativeMethods.WNDCLASS_I wc = new NativeMethods.WNDCLASS_I();
		IntPtr moduleHandle = NativeMethods.GetModuleHandle(null);
		if (!NativeMethods.GetClassInfo(new HandleRef(this, moduleHandle), className, wc) && NativeMethods.RegisterClass(wndClass) == 0)
		{
			return IntPtr.Zero;
		}
		HandleRef hWndParent = ((parent == null) ? NativeMethods.NullHandleRef : new HandleRef(parent, parent.Handle));
		windowHandle = NativeMethods.CreateWindowEx(0, className, windowName, style, x, y, width, height, hWndParent, NativeMethods.NullHandleRef, new HandleRef(this, moduleHandle), null);
		return windowHandle;
	}

	public virtual IntPtr WindowProc(IntPtr wnd, int msg, IntPtr wParam, IntPtr lParam)
	{
		return NativeMethods.DefWindowProc(wnd, msg, wParam, lParam);
	}
}
