using System;

namespace Sce.Atf.Controls.Adaptable;

public abstract class DiagramRenderer : IDisposable
{
	private bool m_isPrinting;

	public Func<object, DiagramDrawingStyle> GetStyle;

	private bool m_disposed;

	public bool IsPrinting
	{
		get
		{
			return m_isPrinting;
		}
		protected set
		{
			m_isPrinting = value;
		}
	}

	public event EventHandler Redraw;

	protected virtual void OnRedraw()
	{
		this.Redraw.Raise(this, EventArgs.Empty);
	}

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

	~DiagramRenderer()
	{
		Dispose(disposing: false);
	}
}
