using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Firaxis.Utility;

public class LowLevelKeyHandler : IDisposable
{
	private NativeMethods.HookHandlerDelegate m_proc;

	private IntPtr m_hookID = IntPtr.Zero;

	private ISet<Keys> m_keys = new HashSet<Keys>();

	private bool m_sendUp = false;

	public event KeyEventHandler KeyDown;

	public event KeyEventHandler KeyUp;

	public LowLevelKeyHandler()
	{
		m_proc = HookCallback;
		HookKeyboardInput();
	}

	public LowLevelKeyHandler(IEnumerable<Keys> keys, bool sendUp)
		: this()
	{
		foreach (Keys key in keys)
		{
			m_keys.Add(key);
		}
		m_sendUp = sendUp;
	}

	public void Dispose()
	{
		UnhookKeyboardInput();
	}

	public void HookKeyboardInput()
	{
		using Process process = Process.GetCurrentProcess();
		using ProcessModule processModule = process.MainModule;
		m_hookID = NativeMethods.SetWindowsHookEx(13, m_proc, NativeMethods.GetModuleHandle(processModule.ModuleName), 0u);
	}

	public void UnhookKeyboardInput()
	{
		if (m_hookID != IntPtr.Zero)
		{
			NativeMethods.UnhookWindowsHookEx(m_hookID);
		}
	}

	private IntPtr HookCallback(int nCode, IntPtr wParam, ref NativeMethods.KBDLLHOOKSTRUCT lParam)
	{
		if (nCode >= 0)
		{
			Keys vkCode = (Keys)lParam.vkCode;
			if (m_keys.Count == 0 || m_keys.Contains(vkCode))
			{
				if ((lParam.flags & 0x80) == 0)
				{
					this.KeyDown?.Invoke(this, new KeyEventArgs(vkCode));
					return (IntPtr)1;
				}
				this.KeyUp?.Invoke(this, new KeyEventArgs(vkCode));
				if (!m_sendUp)
				{
					return (IntPtr)1;
				}
			}
		}
		return NativeMethods.CallNextHookEx(m_hookID, nCode, wParam, ref lParam);
	}
}
