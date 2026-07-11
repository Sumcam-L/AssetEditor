using System;

namespace Sce.Atf.Direct2D;

public abstract class D2dResource : IDisposable
{
	private bool m_disposed;

	public bool IsDisposed => m_disposed;

	public void Dispose()
	{
		if (!m_disposed)
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		m_disposed = true;
	}

	~D2dResource()
	{
		Dispose(disposing: false);
	}
}
