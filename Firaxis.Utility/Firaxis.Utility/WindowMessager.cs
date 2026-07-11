using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Firaxis.Utility;

public class WindowMessager
{
	public delegate void MessageRecievedListener(object sender, WindowMessageRecievedArgs args);

	private int m_iWindowsMessage = 32768;

	private Dictionary<IntPtr, string> m_Messages = new Dictionary<IntPtr, string>();

	private static WindowMessager globalMsg;

	public IntPtr OwnerWindow { get; private set; }

	public int WindowsMessage
	{
		get
		{
			return m_iWindowsMessage;
		}
		set
		{
			if (value < 32768 || value > 49151)
			{
				throw new Exception("WindowsMessage outside of app message range");
			}
			m_iWindowsMessage = value;
		}
	}

	public event MessageRecievedListener MessageRecieved;

	public static event MessageRecievedListener RecievedMessage
	{
		add
		{
			globalMsg.MessageRecieved += value;
		}
		remove
		{
			globalMsg.MessageRecieved -= value;
		}
	}

	public WindowMessager(IntPtr hOwnerWindow)
	{
		OwnerWindow = hOwnerWindow;
	}

	public void SendMessage(IntPtr hWindow, string sMessage)
	{
		HandleRef hHandleRef = new HandleRef(this, hWindow);
		foreach (char c in sMessage)
		{
			SendChar(hHandleRef, c);
		}
		SendChar(hHandleRef, '\0');
	}

	private void SendChar(HandleRef hHandleRef, char c)
	{
		NativeMethods.SendMessage(hHandleRef, WindowsMessage, OwnerWindow, (IntPtr)c);
	}

	public bool HandleWindowMessage(ref Message m)
	{
		if (m.Msg == WindowsMessage)
		{
			IntPtr wParam = m.WParam;
			char c = (char)(int)m.LParam;
			if (!m_Messages.TryGetValue(wParam, out var value))
			{
				value = string.Empty;
			}
			if (c != 0)
			{
				m_Messages[wParam] = value + c;
			}
			else if (value != string.Empty)
			{
				WindowMessageRecievedArgs windowMessageRecievedArgs = new WindowMessageRecievedArgs();
				windowMessageRecievedArgs.Sender = wParam;
				windowMessageRecievedArgs.Message = value;
				this.MessageRecieved?.Invoke(this, windowMessageRecievedArgs);
			}
			return true;
		}
		return false;
	}

	public static void Send(IntPtr hWindow, string sMessage)
	{
		lock (globalMsg)
		{
			globalMsg.OwnerWindow = new IntPtr((int)DateTime.Now.Ticks);
			globalMsg.SendMessage(hWindow, sMessage);
		}
	}

	public static bool ProcessWindowMessage(Message m)
	{
		return globalMsg.HandleWindowMessage(ref m);
	}

	static WindowMessager()
	{
		globalMsg = new WindowMessager(IntPtr.Zero);
		globalMsg.WindowsMessage = 32769;
	}
}
