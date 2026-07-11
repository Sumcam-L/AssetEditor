using System;

namespace SharpDX;

public abstract class CallbackBase : DisposeBase, ICallbackable, IDisposable
{
	IDisposable ICallbackable.Shadow { get; set; }

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (((ICallbackable)this).Shadow != null)
			{
				((ICallbackable)this).Shadow.Dispose();
				((ICallbackable)this).Shadow = null;
			}
		}
	}
}
