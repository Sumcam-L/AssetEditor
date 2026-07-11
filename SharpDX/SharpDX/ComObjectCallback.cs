using System;

namespace SharpDX;

public class ComObjectCallback : ComObject, ICallbackable, IDisposable
{
	public IDisposable Shadow
	{
		get
		{
			throw new InvalidOperationException("Invalid access to Callback. This is used internally.");
		}
		set
		{
			throw new InvalidOperationException("Invalid access to Callback. This is used internally.");
		}
	}

	protected ComObjectCallback(IntPtr pointer)
		: base(pointer)
	{
	}

	protected ComObjectCallback()
	{
	}
}
