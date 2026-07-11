using System;
using Sce.Atf;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public abstract class CompositeDomDocument : DomNodeAdapter, IDocument, IResource, IDisposable
{
	private bool m_dirty;

	private Uri m_uri;

	public virtual string Type => "Unknown".Localize();

	public virtual Uri Uri
	{
		get
		{
			return m_uri;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value != m_uri)
			{
				Uri uri = m_uri;
				m_uri = value;
				OnUriChanged(new UriChangedEventArgs(uri, m_uri));
			}
		}
	}

	public virtual bool Dirty
	{
		get
		{
			return m_dirty;
		}
		set
		{
			if (m_dirty != value)
			{
				m_dirty = value;
				OnDirtyChanged(EventArgs.Empty);
			}
		}
	}

	public virtual bool IsReadOnly => false;

	public event EventHandler DirtyChanged;

	public event EventHandler<UriChangedEventArgs> UriChanged;

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public abstract void UpdateControlInfo();

	protected abstract void Dispose(bool disposing);

	protected virtual void OnDirtyChanged(EventArgs e)
	{
		this.DirtyChanged.Raise(this, e);
	}

	protected virtual void OnReloaded(EventArgs args)
	{
	}

	protected virtual void OnUriChanged(UriChangedEventArgs e)
	{
		this.UriChanged.Raise(this, e);
	}
}
