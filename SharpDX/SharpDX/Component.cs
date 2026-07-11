using System;

namespace SharpDX;

public abstract class Component : ComponentBase, IDisposable
{
	protected DisposeCollector DisposeCollector { get; set; }

	internal bool IsAttached { get; set; }

	protected internal bool IsDisposed { get; private set; }

	protected internal bool IsDisposing { get; private set; }

	public event EventHandler<EventArgs> Disposing;

	protected internal Component()
	{
	}

	protected Component(string name)
		: base(name)
	{
	}

	public void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposing = true;
			this.Disposing?.Invoke(this, EventArgs.Empty);
			Dispose(disposeManagedResources: true);
			IsDisposed = true;
		}
	}

	protected virtual void Dispose(bool disposeManagedResources)
	{
		if (disposeManagedResources)
		{
			if (DisposeCollector != null)
			{
				DisposeCollector.Dispose();
			}
			DisposeCollector = null;
		}
	}

	protected internal T ToDispose<T>(T toDisposeArg)
	{
		if (!object.ReferenceEquals(toDisposeArg, null))
		{
			if (DisposeCollector == null)
			{
				DisposeCollector = new DisposeCollector();
			}
			return DisposeCollector.Collect(toDisposeArg);
		}
		return default(T);
	}

	protected internal void RemoveAndDispose<T>(ref T objectToDispose)
	{
		if (!object.ReferenceEquals(objectToDispose, null) && DisposeCollector != null)
		{
			DisposeCollector.RemoveAndDispose(ref objectToDispose);
		}
	}

	protected internal void RemoveToDispose<T>(T toDisposeArg)
	{
		if (!object.ReferenceEquals(toDisposeArg, null) && DisposeCollector != null)
		{
			DisposeCollector.Remove(toDisposeArg);
		}
	}
}
