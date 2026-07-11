using System;

namespace Sce.Atf.Input;

public class Message
{
	private IntPtr hWnd;

	private int msg;

	private IntPtr wparam;

	private IntPtr lparam;

	private IntPtr result;

	public IntPtr HWnd
	{
		get
		{
			return hWnd;
		}
		set
		{
			hWnd = value;
		}
	}

	public int Msg
	{
		get
		{
			return msg;
		}
		set
		{
			msg = value;
		}
	}

	public IntPtr WParam
	{
		get
		{
			return wparam;
		}
		set
		{
			wparam = value;
		}
	}

	public IntPtr LParam
	{
		get
		{
			return lparam;
		}
		set
		{
			lparam = value;
		}
	}

	public IntPtr Result
	{
		get
		{
			return result;
		}
		set
		{
			result = value;
		}
	}

	public override bool Equals(object o)
	{
		if (!(o is Message))
		{
			return false;
		}
		Message message = (Message)o;
		return hWnd == message.hWnd && msg == message.msg && wparam == message.wparam && lparam == message.lparam && result == message.result;
	}

	public static bool operator !=(Message a, Message b)
	{
		return !a.Equals(b);
	}

	public static bool operator ==(Message a, Message b)
	{
		return a.Equals(b);
	}

	public override int GetHashCode()
	{
		return ((int)hWnd << 4) | msg;
	}
}
