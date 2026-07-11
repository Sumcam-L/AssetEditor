using System;

namespace Sce.Atf.Dom;

public class DomDocument : DomResource, IDocument, IResource
{
	private bool m_dirty;

	public virtual bool IsReadOnly => false;

	public virtual bool Dirty
	{
		get
		{
			return m_dirty;
		}
		set
		{
			if (value != m_dirty)
			{
				m_dirty = value;
				OnDirtyChanged(EventArgs.Empty);
			}
		}
	}

	public event EventHandler DirtyChanged;

	protected virtual void OnDirtyChanged(EventArgs e)
	{
		this.DirtyChanged.Raise(this, e);
	}

	protected virtual void OnReloaded(EventArgs args)
	{
	}
}
