using System;

namespace SharpDX;

public abstract class DisposeBase : IDisposable
{
	public bool IsDisposed { get; private set; }

	public event EventHandler<EventArgs> Disposing;

	public event EventHandler<EventArgs> Disposed;

	~DisposeBase()
	{
		CheckAndDispose(disposing: false);
	}

	public void Dispose()
	{
		CheckAndDispose(disposing: true);
	}

	private void CheckAndDispose(bool disposing)
	{
		if (!IsDisposed)
		{
			this.Disposing?.Invoke(this, EventArgs.Empty);
			Dispose(disposing);
			GC.SuppressFinalize(this);
			IsDisposed = true;
			this.Disposed?.Invoke(this, EventArgs.Empty);
		}
	}

	protected abstract void Dispose(bool disposing);
}
