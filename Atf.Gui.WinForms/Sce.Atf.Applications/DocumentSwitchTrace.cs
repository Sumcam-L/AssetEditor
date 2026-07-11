using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

internal static class DocumentSwitchTrace
{
	private const int WmPaint = 0x000F;
	private const int WmEraseBackground = 0x0014;
	private const int WmShowWindow = 0x0018;
	private const int WmWindowPositionChanging = 0x0046;
	private const int WmWindowPositionChanged = 0x0047;
	private const uint SwpShowWindow = 0x0040;
	private const uint SwpHideWindow = 0x0080;

	private static long s_nextGeneration;
	private static long s_activeGeneration;

	internal static bool IsActive => Volatile.Read(ref s_activeGeneration) != 0;

	internal static long Begin(string reason)
	{
		long generation = Interlocked.Increment(ref s_nextGeneration);
		Volatile.Write(ref s_activeGeneration, generation);
		PaintTimingLog.Write("NativeSwitchTrace begin generation={0}, reason={1}", generation, reason);
		return generation;
	}

	internal static long BeginOrReuse(string reason)
	{
		long generation = Volatile.Read(ref s_activeGeneration);
		return generation != 0 ? generation : Begin(reason);
	}

	internal static void End(long generation)
	{
		if (Interlocked.CompareExchange(ref s_activeGeneration, 0, generation) == generation)
			PaintTimingLog.Write("NativeSwitchTrace end generation={0}", generation);
	}

	internal static void Trace(
		Control control,
		string role,
		string phase,
		ref Message message,
		Func<string> context,
		Func<bool> isActiveSurface)
	{
		long generation = Volatile.Read(ref s_activeGeneration);
		if (generation == 0 || !IsSelectedMessage(message.Msg))
			return;

		try
		{
			string windowPosition = string.Empty;
			if (message.Msg == WmWindowPositionChanging || message.Msg == WmWindowPositionChanged)
				windowPosition = ", " + DecodeWindowPosition(message.LParam);

			Control parent = control.Parent;
			IntPtr parentHandle = parent != null && parent.IsHandleCreated
				? parent.Handle
				: IntPtr.Zero;
			string traceContext = context?.Invoke() ?? string.Empty;
			bool active = isActiveSurface?.Invoke() ?? false;

			PaintTimingLog.Write(
				"NativeSwitchTrace generation={0}, role={1}, phase={2}, type={3}, message={4}, hwnd=0x{5:X}, parentHwnd=0x{6:X}, visible={7}, isHandleCreated={8}, backColor={9}, bounds={10}, clientRectangle={11}, {12}, active={13}{14}",
				generation, role, phase, control.GetType().FullName,
				GetMessageName(message.Msg), message.HWnd.ToInt64(), parentHandle.ToInt64(), control.Visible,
				control.IsHandleCreated, control.BackColor, control.Bounds, control.ClientRectangle,
				traceContext, active, windowPosition);
		}
		catch
		{
			// Native tracing must never affect target message processing.
		}
	}

	internal static IDisposable Observe(
		Control control,
		string role,
		Func<string> context,
		Func<bool> isActiveSurface)
	{
		return new Observer(control, role, context, isActiveSurface);
	}

	private static bool IsSelectedMessage(int message)
	{
		return message == WmEraseBackground || message == WmPaint || message == WmShowWindow ||
			message == WmWindowPositionChanging || message == WmWindowPositionChanged;
	}

	private static string GetMessageName(int message)
	{
		switch (message)
		{
			case WmEraseBackground: return "WM_ERASEBKGND";
			case WmPaint: return "WM_PAINT";
			case WmShowWindow: return "WM_SHOWWINDOW";
			case WmWindowPositionChanging: return "WM_WINDOWPOSCHANGING";
			case WmWindowPositionChanged: return "WM_WINDOWPOSCHANGED";
			default: return "UNKNOWN";
		}
	}

	private static string DecodeWindowPosition(IntPtr value)
	{
		if (value == IntPtr.Zero)
			return "windowPos=unavailable";

		try
		{
			WindowPosition position = Marshal.PtrToStructure<WindowPosition>(value);
			return string.Format(
				"windowPos=(hwnd=0x{0:X}, insertAfter=0x{1:X}, x={2}, y={3}, cx={4}, cy={5}, flags=0x{6:X})",
				position.Hwnd.ToInt64(), position.HwndInsertAfter.ToInt64(), position.X, position.Y,
				position.Width, position.Height, position.Flags);
		}
		catch
		{
			return "windowPos=unavailable";
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct WindowPosition
	{
		public IntPtr Hwnd;
		public IntPtr HwndInsertAfter;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public uint Flags;
	}

	private sealed class Observer : NativeWindow, IDisposable
	{
		private Control m_control;
		private readonly string m_role;
		private readonly Func<string> m_context;
		private readonly Func<bool> m_isActiveSurface;

		public Observer(Control control, string role, Func<string> context, Func<bool> isActiveSurface)
		{
			m_control = control ?? throw new ArgumentNullException(nameof(control));
			m_role = role;
			m_context = context;
			m_isActiveSurface = isActiveSurface;
			control.HandleCreated += ControlHandleCreated;
			control.HandleDestroyed += ControlHandleDestroyed;
			control.Disposed += ControlDisposed;
			if (control.IsHandleCreated)
				AttachObservedHandle(control.Handle);
		}

		public void Dispose()
		{
			Control control = m_control;
			if (control == null)
				return;

			m_control = null;
			control.HandleCreated -= ControlHandleCreated;
			control.HandleDestroyed -= ControlHandleDestroyed;
			control.Disposed -= ControlDisposed;
			ReleaseObservedHandle();
		}

		protected override void WndProc(ref Message message)
		{
			Control control = m_control;
			if (control == null)
			{
				base.WndProc(ref message);
				return;
			}

			Trace(control, m_role, "before", ref message, m_context, m_isActiveSurface);
			TraceWindowPositionVisibility(control, ref message);
			base.WndProc(ref message);
			if (message.Msg == WmPaint || message.Msg == WmEraseBackground || message.Msg == WmShowWindow)
				Trace(control, m_role, "after", ref message, m_context, m_isActiveSurface);
		}

		private void TraceWindowPositionVisibility(Control control, ref Message message)
		{
			if (!IsActive || message.Msg != WmWindowPositionChanging || message.LParam == IntPtr.Zero)
				return;

			try
			{
				WindowPosition position = Marshal.PtrToStructure<WindowPosition>(message.LParam);
				if ((position.Flags & (SwpShowWindow | SwpHideWindow)) == 0)
					return;

				Message visibilityMessage = Message.Create(message.HWnd, WmShowWindow,
					(position.Flags & SwpShowWindow) != 0 ? new IntPtr(1) : IntPtr.Zero, IntPtr.Zero);
				Trace(control, m_role, "before", ref visibilityMessage, m_context, m_isActiveSurface);
			}
			catch
			{
				// Native tracing must never affect target message processing.
			}
		}

		private void ControlHandleCreated(object sender, EventArgs e)
		{
			Control control = m_control;
			if (control != null && control.IsHandleCreated)
				AttachObservedHandle(control.Handle);
		}

		private void ControlHandleDestroyed(object sender, EventArgs e)
		{
			ReleaseObservedHandle();
		}

		private void ControlDisposed(object sender, EventArgs e)
		{
			Dispose();
		}

		private void ReleaseObservedHandle()
		{
			try
			{
				if (Handle != IntPtr.Zero)
					ReleaseHandle();
			}
			catch
			{
				// Observer failures must not affect the target control lifecycle.
			}
		}

		private void AttachObservedHandle(IntPtr targetHandle)
		{
			try
			{
				if (Handle != IntPtr.Zero && Handle != targetHandle)
					ReleaseHandle();
				if (Handle == IntPtr.Zero)
					AssignHandle(targetHandle);
				if (Handle != targetHandle)
					throw new InvalidOperationException("Native observer did not attach to the target HWND.");
			}
			catch (Exception ex)
			{
				try
				{
					PaintTimingLog.Write(
						"NativeSwitchTrace observerAttachFailed role={0}, targetHwnd=0x{1:X}, observerHwnd=0x{2:X}, error={3}",
						m_role, targetHandle.ToInt64(), Handle.ToInt64(), ex.GetType().FullName);
				}
				catch
				{
					// Observer failures must not affect the target control lifecycle.
				}
			}
		}
	}
}
