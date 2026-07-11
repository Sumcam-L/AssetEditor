using System;
using System.Windows.Forms;
using Sce.Atf.Input;

namespace Sce.Atf;

internal class MessageInterop
{
	public IntPtr HWnd { get; private set; }

	public int Msg { get; private set; }

	public IntPtr WParam { get; private set; }

	public IntPtr LParam { get; private set; }

	public IntPtr Result { get; private set; }

	private MessageInterop()
	{
	}

	public MessageInterop(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam, IntPtr result)
	{
		HWnd = hWnd;
		Msg = msg;
		WParam = wparam;
		LParam = lparam;
		Result = result;
	}

	public MessageInterop(Sce.Atf.Input.Message msg)
		: this(msg.HWnd, msg.Msg, msg.WParam, msg.LParam, msg.Result)
	{
	}

	public MessageInterop(System.Windows.Forms.Message msg)
		: this(msg.HWnd, msg.Msg, msg.WParam, msg.LParam, msg.Result)
	{
	}

	public static implicit operator MessageInterop(Sce.Atf.Input.Message msg)
	{
		return new MessageInterop(msg);
	}

	public static implicit operator MessageInterop(System.Windows.Forms.Message msg)
	{
		return new MessageInterop(msg);
	}

	public static implicit operator Sce.Atf.Input.Message(MessageInterop msg)
	{
		return ToAtf(msg);
	}

	public static implicit operator System.Windows.Forms.Message(MessageInterop msg)
	{
		return ToWf(msg);
	}

	public static System.Windows.Forms.Message ToWf(Sce.Atf.Input.Message msg)
	{
		return new System.Windows.Forms.Message
		{
			HWnd = msg.Result,
			Msg = msg.Msg,
			WParam = msg.WParam,
			LParam = msg.LParam,
			Result = msg.Result
		};
	}

	public static Sce.Atf.Input.Message ToAtf(System.Windows.Forms.Message msg)
	{
		Sce.Atf.Input.Message message = new Sce.Atf.Input.Message();
		message.HWnd = msg.Result;
		message.Msg = msg.Msg;
		message.WParam = msg.WParam;
		message.LParam = msg.LParam;
		message.Result = msg.Result;
		return message;
	}
}
